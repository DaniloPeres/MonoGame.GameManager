using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Controls.Shading;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Layout;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Abstracts
{
    /// <summary>
    /// Base class of the controls that have a scale. The size and the origin are stored without the scale
    /// (<see cref="SizeWithoutScale"/>, <see cref="OriginWithoutScale"/>) and returned scaled by the nested scale.
    /// </summary>
    /// <remarks>
    /// The "local space" of a control is its unscaled space, where (0, 0) is the top-left corner of the control and
    /// <see cref="SizeWithoutScale"/> its bottom-right corner. The helpers <see cref="DrawLocalTexture"/>,
    /// <see cref="DrawLocalRectangle"/> and <see cref="ToLocalPosition(Vector2)"/> work in that space and follow the
    /// position, origin, scale and rotation of the control.
    /// </remarks>
    public abstract class ScalableControlAbstract<TControl> : Control<TControl>, IScalableControl, IShadingHost where TControl : IScalableControl
    {
        private ShadingRenderer shading;

        Vector2 IScalableControl.Scale {
            get => Scale;
            set => Scale = value;
        }
        public virtual Vector2 Scale
        {
            get => scale;
            set
            {
                scale = value;
                MarkAsDirty();
            }
        }
        private Vector2 scale = Vector2.One;

        public override Vector2 Size
        {
            get => base.Size * NestedScale;
            set => base.Size = value;
        }

        public Vector2 SizeWithoutScale => base.Size;

        public override Vector2 Origin
        {
            get => base.Origin * NestedScale;
            set => base.Origin = value;
        }

        Vector2 IScalableControl.OriginWithoutScale {
            get => OriginWithoutScale;
            set => OriginWithoutScale = value;
        }
        public virtual Vector2 OriginWithoutScale
        {
            get => base.Origin;
            set => base.Origin = value;
        }

        protected override Vector2 CalculateSize() => SizeWithoutScale;
        public override Vector2 CalculateNestedScale() => base.CalculateNestedScale() * Scale;
        IScalableControl IScalableControl.SetScale(float scale) => SetScale(scale);
        public TControl SetScale(float scale) => SetScale(new Vector2(scale));
        IScalableControl IScalableControl.SetScale(Vector2 scale) => SetScale(scale);
        public TControl SetScale(Vector2 scale)
        {
            Scale = scale;
            return (TControl)(object)this;
        }

        public override TControl SetOriginRate(Vector2 originRate) => SetOriginRate(originRate, SizeWithoutScale);

        /// <summary>
        /// Converts a point of the parent's coordinate space (eg: a pointer position) to the local space of the
        /// control. The rotation is considered.
        /// </summary>
        public Vector2 ToLocalPosition(Point point) => ToLocalPosition(point.ToVector2());

        /// <inheritdoc cref="ToLocalPosition(Point)"/>
        public Vector2 ToLocalPosition(Vector2 point)
        {
            var offset = point - DestinationRectangle.Location.ToVector2();
            if (Rotation != 0f)
                offset = offset.Rotated(-Rotation);

            var scale = NestedScale;
            var local = new Vector2(scale.X != 0f ? offset.X / scale.X : 0f, scale.Y != 0f ? offset.Y / scale.Y : 0f);
            return local + OriginWithoutScale;
        }

        /// <summary>Converts a point of the local space to the parent's coordinate space (inverse of <see cref="ToLocalPosition(Vector2)"/>).</summary>
        public Vector2 LocalToParentPosition(Vector2 localPoint)
        {
            var offset = (localPoint - OriginWithoutScale) * NestedScale;
            if (Rotation != 0f)
                offset = offset.Rotated(Rotation);
            return DestinationRectangle.Location.ToVector2() + offset;
        }

        /// <summary>
        /// Draws a texture (or a part of it) stretched over an area given in local space, following the position,
        /// origin, scale and rotation of the control.
        /// </summary>
        protected void DrawLocalTexture(SpriteBatch spriteBatch, Texture2D texture, Rectangle? sourceRectangle, RectangleF localArea, Color color)
        {
            if (texture == null || texture.IsDisposed || localArea.Width <= 0f || localArea.Height <= 0f)
                return;

            var source = sourceRectangle ?? texture.Bounds;
            if (source.Width <= 0 || source.Height <= 0)
                return;

            var sourceSize = new Vector2(source.Width, source.Height);
            var areaSize = new Vector2(localArea.Width, localArea.Height);
            var origin = (OriginWithoutScale - new Vector2(localArea.X, localArea.Y)) / areaSize * sourceSize;
            var scale = areaSize / sourceSize * NestedScale;
            spriteBatch.Draw(texture, GetPosition(), source, color, Rotation, origin, scale, SpriteEffects.None, LayerDepthDraw);
        }

        /// <summary>Draws a solid rectangle given in local space (see <see cref="DrawLocalTexture"/>).</summary>
        protected void DrawLocalRectangle(SpriteBatch spriteBatch, RectangleF localArea, Color color)
        {
            if (color.A == 0 && color.R == 0 && color.G == 0 && color.B == 0)
                return; // fully transparent
            DrawLocalTexture(spriteBatch, ShapeExtension.WhitePixelTexture, null, localArea, color);
        }

        /// <summary>Draws the outline of a rectangle given in local space, inside the rectangle.</summary>
        protected void DrawLocalBorder(SpriteBatch spriteBatch, RectangleF localArea, Color color, float thickness)
        {
            if (thickness <= 0f)
                return;

            var horizontal = Math.Min(thickness, localArea.Height / 2f);
            var vertical = Math.Min(thickness, localArea.Width / 2f);
            DrawLocalRectangle(spriteBatch, new RectangleF(localArea.X, localArea.Y, localArea.Width, horizontal), color);
            DrawLocalRectangle(spriteBatch, new RectangleF(localArea.X, localArea.Bottom - horizontal, localArea.Width, horizontal), color);
            DrawLocalRectangle(spriteBatch, new RectangleF(localArea.X, localArea.Y + horizontal, vertical, localArea.Height - horizontal * 2f), color);
            DrawLocalRectangle(spriteBatch, new RectangleF(localArea.Right - vertical, localArea.Y + horizontal, vertical, localArea.Height - horizontal * 2f), color);
        }

        /// <summary>
        /// Draws a texture with nine-slice scaling over an area given in local space (see <see cref="NineSlice"/>),
        /// with a border in texture pixels and another one in local units.
        /// </summary>
        protected void DrawLocalNineSlice(SpriteBatch spriteBatch, Texture2D texture, Rectangle? sourceRectangle, Thickness sourceBorder,
            RectangleF localArea, Thickness destinationBorder, Color color, bool drawCenter = true)
        {
            if (texture == null || texture.IsDisposed || localArea.Width <= 0f || localArea.Height <= 0f)
                return;

            Span<int> sourceX = stackalloc int[4];
            Span<int> sourceY = stackalloc int[4];
            Span<float> destinationX = stackalloc float[4];
            Span<float> destinationY = stackalloc float[4];
            NineSlice.ComputeSlices(sourceRectangle ?? texture.Bounds, sourceBorder, localArea.Size, destinationBorder, sourceX, sourceY, destinationX, destinationY);

            for (var row = 0; row < 3; row++)
            {
                for (var column = 0; column < 3; column++)
                {
                    if (row == 1 && column == 1 && !drawCenter)
                        continue;

                    var source = new Rectangle(sourceX[column], sourceY[row], sourceX[column + 1] - sourceX[column], sourceY[row + 1] - sourceY[row]);
                    var area = new RectangleF(localArea.X + destinationX[column], localArea.Y + destinationY[row],
                        destinationX[column + 1] - destinationX[column], destinationY[row + 1] - destinationY[row]);
                    DrawLocalTexture(spriteBatch, texture, source, area, color);
                }
            }
        }

        /// <summary>The whole area of the control in local space.</summary>
        protected RectangleF LocalBounds => new RectangleF(Vector2.Zero, SizeWithoutScale);

        // ---- Shading: shadows, glows and outlines that follow the silhouette of the control

        /// <summary>The shading effects of the control (shadows, glows and outlines), in drawing order.</summary>
        public IReadOnlyList<ShadingEffect> ShadingEffects => shading?.Effects ?? (IReadOnlyList<ShadingEffect>)Array.Empty<ShadingEffect>();

        /// <summary>
        /// Adds a shadow, a glow or an outline that follows the silhouette of the control (the opaque pixels of its
        /// texture, the glyphs of its text...). The effects are drawn in the order they are added: add shadows first and
        /// outlines last, so the outlines are drawn over the shadows. An effect belongs to one control at a time.
        /// </summary>
        /// <remarks>
        /// The effects are drawn when the control is drawn by its parent. A control drawn by hand with
        /// <see cref="Control{TControl}.Draw"/> is drawn without them.
        /// </remarks>
        /// <example>
        /// <code>
        /// title.AddShading(new Shadow(new Vector2(0, 5), 8, Color.Black * 0.6f))
        ///      .AddShading(new Glow(Color.Gold, 14))
        ///      .AddShading(new Outline(new Color(70, 30, 0), 3));
        /// </code>
        /// </example>
        public TControl AddShading(ShadingEffect effect)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));
            if (ReferenceEquals(effect.Owner, this))
                return ThisAsT;
            if (effect.Owner != null)
                throw new InvalidOperationException("The shading effect already belongs to another control. Create a new effect for each control (the presets return new effects on every call).");

            effect.Owner = this;
            (shading ??= new ShadingRenderer(this)).Add(effect);
            return ThisAsT;
        }

        /// <summary>Adds several shading effects (eg: a preset of <see cref="ShadingPresets"/>).</summary>
        public TControl AddShadings(IEnumerable<ShadingEffect> effects)
        {
            if (effects == null)
                throw new ArgumentNullException(nameof(effects));
            foreach (var effect in effects)
                AddShading(effect);
            return ThisAsT;
        }

        /// <summary>Adds several shading effects.</summary>
        public TControl AddShadings(params ShadingEffect[] effects) => AddShadings((IEnumerable<ShadingEffect>)effects);

        /// <summary>Replaces the shading effects of the control.</summary>
        public TControl SetShadings(IEnumerable<ShadingEffect> effects)
        {
            ClearShadings();
            return effects == null ? ThisAsT : AddShadings(effects);
        }

        public TControl RemoveShading(ShadingEffect effect)
        {
            if (effect == null || shading == null || !ReferenceEquals(effect.Owner, this))
                return ThisAsT;
            effect.Owner = null;
            shading.Remove(effect);
            return ThisAsT;
        }

        /// <summary>Removes every shading effect and releases their render targets.</summary>
        public TControl ClearShadings()
        {
            if (shading == null)
                return ThisAsT;
            foreach (var effect in shading.Effects)
                effect.Owner = null;
            shading.Clear();
            return ThisAsT;
        }

        /// <summary>The first shading effect of a type, or null.</summary>
        public T FindShading<T>() where T : ShadingEffect
        {
            if (shading == null)
                return null;
            foreach (var effect in shading.Effects)
            {
                if (effect is T found)
                    return found;
            }

            return null;
        }

        /// <summary>
        /// Renders the shading effects again before the next frame. Only needed when what the control draws changed in a
        /// way <see cref="GetContentSignature"/> does not see.
        /// </summary>
        public TControl InvalidateShading()
        {
            shading?.Invalidate();
            return ThisAsT;
        }

        /// <summary>
        /// A value that changes whenever what the control draws changes (its text, texture, frame...), so its shading
        /// effects are rendered again only then; the size, the scale, the alpha of the color and the opacity are already
        /// considered. Null, the default, renders them every frame: override it in custom controls that use shading.
        /// </summary>
        protected virtual int? GetContentSignature() => null;

        /// <summary>
        /// The box of what the control draws, in local units: the area a <see cref="GradientFill"/> is stretched over
        /// (<see cref="GradientBounds.Content"/>). The whole control by default; a label returns the box of its glyphs.
        /// </summary>
        protected virtual RectangleF GetContentBounds() => LocalBounds;

        /// <summary>
        /// How far the shading effects reach outside the control (blurs, outlines, offsets and animations included), in
        /// screen units (0 without effects): the room to keep around the control so its effects are not cut.
        /// </summary>
        public float ShadingMargin
        {
            get
            {
                if (shading == null || !shading.HasEffects)
                    return 0f;
                var scale = NestedScale;
                return shading.MaxExtentLocal * Math.Max(Math.Abs(scale.X), Math.Abs(scale.Y));
            }
        }

        /// <summary>Renders the shading effects of the control when it changed (called before the frame is drawn).</summary>
        protected void PrepareShading() => shading?.Prepare();

        public override void OnBeforeDraw()
        {
            base.OnBeforeDraw();
            // Containers prepare their shading after their children are laid out and prepared.
            if (!(this is IContainer))
                PrepareShading();
        }

        public override void FireOnUpdateEvent(GameTime gameTime)
        {
            if (shading != null && shading.HasEffects)
                shading.Update(GetScaledDeltaSeconds(gameTime));
            base.FireOnUpdateEvent(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing && shading != null)
            {
                ClearShadings();
                shading.Dispose();
                shading = null;
            }

            base.Dispose(disposing);
        }

        IControl IShadingHost.Control => this;

        float IShadingHost.ShadingMargin => ShadingMargin;

        int? IShadingHost.GetShadingContentSignature() => GetContentSignature();

        RectangleF IShadingHost.GetShadingContentBounds() => GetContentBounds();

        void IShadingHost.DrawShadingContent(SpriteBatch spriteBatch) => Draw(spriteBatch);

        void IShadingHost.DrawWithShading(SpriteBatch spriteBatch)
        {
            var renderer = shading;
            if (renderer == null || !renderer.HasEffects || ShadingRenderer.IsCapturing)
            {
                Draw(spriteBatch);
                return;
            }

            renderer.DrawLayer(spriteBatch, ShadingLayer.Behind);
            // Fills are drawn instead of the control (which is drawn when they are not ready).
            if (!renderer.DrawLayer(spriteBatch, ShadingLayer.Content))
                Draw(spriteBatch);
            renderer.DrawLayer(spriteBatch, ShadingLayer.Front);
        }
    }
}
