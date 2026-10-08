using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>What the <see cref="ButtonEffectRenderer"/> needs from its button.</summary>
    internal interface IButtonEffectHost
    {
        IControl Control { get; }
        Vector2 SizeWithoutScale { get; }
        Vector2 OriginWithoutScale { get; }
        Vector2 NestedScale { get; }
        float Rotation { get; }
        float NestedOpacity { get; }
        ButtonVisualState VisualState { get; }
        float CornerRadius { get; }
        float EffectPadding { get; }
        ButtonEffectMask EffectMask { get; }
        Label TextLabel { get; }

        /// <summary>The background color of the current state, or null when the button is drawn with a texture.</summary>
        Color? EffectBackgroundColor { get; }

        Vector2 GetPosition();
        Vector2 ToLocalPosition(Vector2 point);

        /// <summary>The texture of the current state and the local area where it is drawn, or null.</summary>
        Texture2D GetEffectMaskTexture(out RectangleF area);
    }

    /// <summary>
    /// Updates and draws the effects of a button in three layers (behind, inside and in front of it).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each layer is drawn with a transformation that maps the local space of the button to the screen. Consecutive
    /// effects of a layer with the same <see cref="ButtonEffectBlend"/> share one batch: lights use
    /// <see cref="ButtonEffectResources.LightBlendState"/>, painted effects the premultiplied alpha blend.
    /// </para>
    /// <para>
    /// The inside layer is clipped to the shape of the button: with the scissor rectangle when the shape is a rectangle
    /// that is not rotated, otherwise it is drawn into render targets (before the frame is drawn) multiplied by the
    /// shape. Its painted effects are drawn first and its lights after them.
    /// </para>
    /// </remarks>
    internal sealed class ButtonEffectRenderer : IDisposable
    {
        private const int MaxTargetSize = 4096;

        /// <summary>How far the effects can go outside the button, for culling (local units).</summary>
        private const float CullMargin = 160f;

        private readonly IButtonEffectHost host;
        private readonly List<ButtonEffect> effects = new List<ButtonEffect>();
        private readonly ButtonEffectContext context = new ButtonEffectContext();
        private RenderTarget2D lightTarget;
        private RenderTarget2D paintTarget;
        private SpriteBatch offscreenBatch;
        private bool insideUsesTargets;
        private bool isCulled;
        private bool hasState;
        private ButtonVisualState lastState;
        private float time;

        public ButtonEffectRenderer(IButtonEffectHost host)
        {
            this.host = host ?? throw new ArgumentNullException(nameof(host));
            // Each button starts at another time, so a group of buttons does not pulse in lockstep.
            time = ServiceProvider.Random.NextFloat(0f, 100f);
            context.Host = host.Control;
            context.HostInternal = host;
        }

        public List<ButtonEffect> Effects => effects;

        public ButtonEffectContext Context => context;

        public bool HasEffects => effects.Count > 0;

        public void Update(float deltaSeconds, ButtonVisualState state)
        {
            if (effects.Count == 0)
                return;

            time += Math.Max(0f, deltaSeconds);
            RefreshContext(deltaSeconds);

            if (hasState && lastState != state)
            {
                for (var i = 0; i < effects.Count; i++)
                {
                    if (effects[i].IsEnabled)
                        effects[i].OnStateChanged(context, lastState);
                }
            }

            hasState = true;
            lastState = state;

            for (var i = 0; i < effects.Count; i++)
            {
                if (effects[i].IsEnabled)
                    effects[i].Update(context);
            }
        }

        /// <summary>Renders the inside layer into its render targets when it needs a mask (before the frame is drawn).</summary>
        public void Prepare()
        {
            insideUsesTargets = false;
            if (effects.Count == 0)
            {
                ReleaseTargets();
                return;
            }

            var graphicsDevice = ServiceProvider.GraphicsDevice;
            var controlManager = ServiceProvider.ControlManager;
            if (graphicsDevice == null || controlManager == null)
                return;

            ButtonEffectResources.ReleaseUnused();
            RefreshContext(0f);

            // A button hidden by a clipping parent (eg: scrolled out of a scroll viewer) draws no effect.
            isCulled = IsOutsideClippingParents();
            if (isCulled)
                return;

            var mask = ResolveMask(out var maskTexture, out var maskArea);
            if (!HasVisibleEffects(ButtonEffectLayer.Inside, null) || mask == ButtonEffectMask.Rectangle && host.Rotation == 0f)
            {
                ReleaseTargets();
                return;
            }

            var size = host.SizeWithoutScale;
            var scale = host.NestedScale;
            var width = Math.Min(MaxTargetSize, (int)Math.Ceiling(size.X * Math.Abs(scale.X)));
            var height = Math.Min(MaxTargetSize, (int)Math.Ceiling(size.Y * Math.Abs(scale.Y)));
            if (width <= 0 || height <= 0 || size.X <= 0f || size.Y <= 0f)
            {
                ReleaseTargets();
                return;
            }

            offscreenBatch ??= new SpriteBatch(graphicsDevice);
            var previousTargets = graphicsDevice.GetRenderTargets();
            var transform = Matrix.CreateScale(width / size.X, height / size.Y, 1f);

            RenderInsideTarget(ref paintTarget, ButtonEffectBlend.Normal, graphicsDevice, controlManager, width, height, transform, mask, maskTexture, maskArea);
            RenderInsideTarget(ref lightTarget, ButtonEffectBlend.Light, graphicsDevice, controlManager, width, height, transform, mask, maskTexture, maskArea);

            graphicsDevice.SetRenderTargets(previousTargets);
            insideUsesTargets = true;
        }

        /// <summary>Draws the effects of the behind or front layer.</summary>
        public void DrawLayer(SpriteBatch spriteBatch, ButtonEffectLayer layer)
        {
            if (isCulled || !HasVisibleEffects(layer, null))
                return;

            var controlManager = ServiceProvider.ControlManager;
            if (controlManager == null)
                return;

            var state = controlManager.CurrentState.WithTransform(GetLocalTransform(controlManager.CurrentState.TransformMatrix));
            DrawRuns(spriteBatch, controlManager, state, layer, null);
        }

        /// <summary>Draws the inside layer, clipped to the shape of the button.</summary>
        public void DrawInside(SpriteBatch spriteBatch)
        {
            if (isCulled || !HasVisibleEffects(ButtonEffectLayer.Inside, null))
                return;

            var controlManager = ServiceProvider.ControlManager;
            if (controlManager == null)
                return;

            var state = controlManager.CurrentState.WithTransform(GetLocalTransform(controlManager.CurrentState.TransformMatrix));

            if (insideUsesTargets)
            {
                CompositeTarget(spriteBatch, controlManager, state, paintTarget, BlendState.AlphaBlend);
                CompositeTarget(spriteBatch, controlManager, state, lightTarget, ButtonEffectResources.LightBlendState);
                return;
            }

            // A rectangle that is not rotated: the scissor rectangle clips the effects.
            if (host.Rotation == 0f)
            {
                var bounds = context.Bounds;
                var scale = host.NestedScale;
                var topLeft = host.GetPosition() + (bounds.Position - host.OriginWithoutScale) * scale;
                var bottomRight = topLeft + bounds.Size * scale;
                var left = (int)Math.Floor(Math.Min(topLeft.X, bottomRight.X));
                var top = (int)Math.Floor(Math.Min(topLeft.Y, bottomRight.Y));
                var clip = new Rectangle(left, top,
                    (int)Math.Ceiling(Math.Max(topLeft.X, bottomRight.X)) - left,
                    (int)Math.Ceiling(Math.Max(topLeft.Y, bottomRight.Y)) - top);
                if (!controlManager.TryGetClipArea(clip, out var area))
                    return;
                state = state.WithScissor(area);
            }

            // The painted effects first (shades under the lights), then the lights.
            DrawRuns(spriteBatch, controlManager, state, ButtonEffectLayer.Inside, ButtonEffectBlend.Normal);
            DrawRuns(spriteBatch, controlManager, state, ButtonEffectLayer.Inside, ButtonEffectBlend.Light);
        }

        public void OnPressed(Vector2 localPosition)
        {
            for (var i = 0; i < effects.Count; i++)
            {
                if (effects[i].IsEnabled)
                    effects[i].OnPressed(context, localPosition);
            }
        }

        public void OnReleased(Vector2 localPosition)
        {
            for (var i = 0; i < effects.Count; i++)
            {
                if (effects[i].IsEnabled)
                    effects[i].OnReleased(context, localPosition);
            }
        }

        public void Dispose()
        {
            ReleaseTargets();
            offscreenBatch?.Dispose();
            offscreenBatch = null;
        }

        /// <summary>
        /// True when the button and its effects (with a margin for glows, rays and particles) are completely outside
        /// the area of a parent that hides its overflow. Parents with their own transformation (cameras) are not
        /// checked.
        /// </summary>
        private bool IsOutsideClippingParents()
            => EffectCulling.IsOutsideClippingParents(host.Control, host.GetPosition(), host.SizeWithoutScale, host.OriginWithoutScale,
                host.NestedScale, host.Rotation, CullMargin);

        private static BlendState GetBlendState(ButtonEffectBlend blend)
            => blend == ButtonEffectBlend.Normal ? BlendState.AlphaBlend : ButtonEffectResources.LightBlendState;

        /// <summary>
        /// Draws the visible effects of a layer (only those of a blend when <paramref name="onlyBlend"/> is set), one
        /// batch for each run of consecutive effects with the same blend.
        /// </summary>
        private void DrawRuns(SpriteBatch spriteBatch, ControlManager controlManager, SpriteBatchState state, ButtonEffectLayer layer, ButtonEffectBlend? onlyBlend)
        {
            context.DeltaSeconds = 0f;
            ButtonEffectBlend? current = null;
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (!IsVisible(effect, layer) || onlyBlend.HasValue && effect.Blend != onlyBlend.Value)
                    continue;

                if (current != effect.Blend)
                {
                    if (current.HasValue)
                        controlManager.PopState(spriteBatch);
                    current = effect.Blend;
                    controlManager.PushState(spriteBatch, state.WithBlendState(GetBlendState(effect.Blend)));
                }

                effect.Draw(spriteBatch, context);
            }

            if (current.HasValue)
                controlManager.PopState(spriteBatch);
        }

        /// <summary>Renders the inside effects of a blend into a render target, multiplied by the shape of the button.</summary>
        private void RenderInsideTarget(ref RenderTarget2D target, ButtonEffectBlend blend, GraphicsDevice graphicsDevice, ControlManager controlManager,
            int width, int height, Matrix transform, ButtonEffectMask mask, Texture2D maskTexture, RectangleF maskArea)
        {
            if (!HasVisibleEffects(ButtonEffectLayer.Inside, blend))
            {
                target?.Dispose();
                target = null;
                return;
            }

            if (target == null || target.IsDisposed || target.Width != width || target.Height != height)
            {
                target?.Dispose();
                target = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            }

            graphicsDevice.SetRenderTarget(target);
            graphicsDevice.Clear(Color.Transparent);

            controlManager.BeginOffscreen(offscreenBatch, controlManager.BaseState
                .WithTransform(transform)
                .WithScissor(null)
                .WithBlendState(GetBlendState(blend)));
            context.DeltaSeconds = 0f;
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (IsVisible(effect, ButtonEffectLayer.Inside) && effect.Blend == blend)
                    effect.Draw(offscreenBatch, context);
            }
            offscreenBatch.End();

            // Multiplies the effects by the shape: what is outside of it becomes transparent.
            offscreenBatch.Begin(SpriteSortMode.Deferred, ButtonEffectResources.MaskBlendState, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, transform);
            DrawMask(offscreenBatch, mask, maskTexture, maskArea, new RectangleF(Vector2.Zero, host.SizeWithoutScale));
            controlManager.EndOffscreen(offscreenBatch);
        }

        private void CompositeTarget(SpriteBatch spriteBatch, ControlManager controlManager, SpriteBatchState state, RenderTarget2D target, BlendState blendState)
        {
            if (target == null || target.IsDisposed)
                return;

            var size = host.SizeWithoutScale;
            controlManager.PushState(spriteBatch, state.WithBlendState(blendState));
            spriteBatch.Draw(target, Vector2.Zero, null, Color.White, 0f, Vector2.Zero,
                new Vector2(size.X / target.Width, size.Y / target.Height), SpriteEffects.None, 0f);
            controlManager.PopState(spriteBatch);
        }

        /// <summary>The mask that clips the inside layer, after resolving <see cref="ButtonEffectMask.Auto"/>.</summary>
        private ButtonEffectMask ResolveMask(out Texture2D maskTexture, out RectangleF maskArea)
        {
            maskTexture = host.GetEffectMaskTexture(out maskArea);
            var mask = host.EffectMask;
            if (mask == ButtonEffectMask.Auto || mask == ButtonEffectMask.Texture)
            {
                if (maskTexture != null && !maskTexture.IsDisposed && maskArea.Width > 0f && maskArea.Height > 0f)
                    return ButtonEffectMask.Texture;
                mask = ButtonEffectMask.Auto;
            }

            if (mask == ButtonEffectMask.Auto)
                return context.CornerRadius > 0f ? ButtonEffectMask.RoundedRectangle : ButtonEffectMask.Rectangle;
            return mask;
        }

        private void DrawMask(SpriteBatch spriteBatch, ButtonEffectMask mask, Texture2D maskTexture, RectangleF maskArea, RectangleF fullArea)
        {
            var shape = context.Bounds;
            switch (mask)
            {
                case ButtonEffectMask.Texture:
                    var source = new Vector2(maskTexture.Width, maskTexture.Height);
                    spriteBatch.Draw(maskTexture, maskArea.Position, null, Color.White, 0f, Vector2.Zero, maskArea.Size / source, SpriteEffects.None, 0f);
                    shape = maskArea;
                    break;
                case ButtonEffectMask.RoundedRectangle:
                    ButtonEffectResources.GetRoundedMask(context.CornerRadius, context.TextureDensity).Draw(spriteBatch, shape, Color.White);
                    break;
            }

            // Removes the effects outside of the shape.
            var transparent = ShapeExtension.TransparentPixelTexture;
            ClearArea(spriteBatch, transparent, fullArea.X, fullArea.Y, fullArea.Width, shape.Y - fullArea.Y);
            ClearArea(spriteBatch, transparent, fullArea.X, shape.Bottom, fullArea.Width, fullArea.Bottom - shape.Bottom);
            ClearArea(spriteBatch, transparent, fullArea.X, shape.Y, shape.X - fullArea.X, shape.Height);
            ClearArea(spriteBatch, transparent, shape.Right, shape.Y, fullArea.Right - shape.Right, shape.Height);
        }

        private static void ClearArea(SpriteBatch spriteBatch, Texture2D transparent, float x, float y, float width, float height)
        {
            if (width <= 0f || height <= 0f)
                return;
            spriteBatch.Draw(transparent, new Vector2(x, y), null, Color.White, 0f, Vector2.Zero, new Vector2(width, height), SpriteEffects.None, 0f);
        }

        private static bool IsVisible(ButtonEffect effect, ButtonEffectLayer layer)
            => effect.IsEnabled && effect.Layer == layer && effect.EffectiveIntensity > 0.001f;

        private bool HasVisibleEffects(ButtonEffectLayer layer, ButtonEffectBlend? blend)
        {
            if (context.Opacity <= 0f)
                return false;
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (IsVisible(effect, layer) && (!blend.HasValue || effect.Blend == blend.Value))
                    return true;
            }

            return false;
        }

        /// <summary>The transformation from the local space of the button to the coordinate space of the sprite batch.</summary>
        private Matrix GetLocalTransform(Matrix? parentTransform)
        {
            var position = host.GetPosition();
            var origin = host.OriginWithoutScale;
            var scale = host.NestedScale;
            var matrix = Matrix.CreateTranslation(-origin.X, -origin.Y, 0f)
                * Matrix.CreateScale(scale.X, scale.Y, 1f);
            if (host.Rotation != 0f)
                matrix *= Matrix.CreateRotationZ(host.Rotation);
            matrix *= Matrix.CreateTranslation(position.X, position.Y, 0f);
            return parentTransform.HasValue ? matrix * parentTransform.Value : matrix;
        }

        private void RefreshContext(float deltaSeconds)
        {
            var size = host.SizeWithoutScale;
            var padding = host.EffectPadding;
            var bounds = new RectangleF(padding, padding, Math.Max(0f, size.X - padding * 2f), Math.Max(0f, size.Y - padding * 2f));
            context.SetShape(bounds, host.CornerRadius);
            context.State = host.VisualState;
            context.Time = time;
            context.DeltaSeconds = deltaSeconds;
            context.Opacity = host.NestedOpacity;
            context.Scale = new Vector2(Math.Abs(host.NestedScale.X), Math.Abs(host.NestedScale.Y));
            context.Rotation = host.Rotation;
            context.Random = ServiceProvider.Random;
        }

        private void ReleaseTargets()
        {
            insideUsesTargets = false;
            lightTarget?.Dispose();
            lightTarget = null;
            paintTarget?.Dispose();
            paintTarget = null;
        }
    }
}
