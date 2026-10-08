using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Effects;
using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.Extensions;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Layout;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Abstracts
{
    /// <summary>
    /// Base class of the buttons: drawn with textures (one per state) or with solid colors (with rounded corners), with
    /// an optional text and light effects. Other controls can be added to it (icons, labels...).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The texture or the color of the current <see cref="VisualState"/> is chosen when drawing. A button owns the
    /// presses it receives, so the controls below it are never clicked at the same time.
    /// </para>
    /// <para>
    /// Light effects (<see cref="AddEffect"/>, see <see cref="ButtonEffect"/> and <see cref="ButtonEffectPresets"/>)
    /// follow the size, the <see cref="CornerRadius"/>, the scale and the rotation of the button. They are drawn behind
    /// the button, inside it (clipped to its shape, see <see cref="EffectMask"/>) or in front of it.
    /// </para>
    /// </remarks>
    public abstract class ButtonAbstract<TButton> : ContainerAbstract<TButton>, IButtonEffectHost where TButton : ButtonAbstract<TButton>
    {
        private Texture2D defaultTexture;
        private Texture2D hoverTexture;
        private Texture2D mousePressedTexture;
        private Texture2D disabledTexture;
        private Vector2 backgroundScale = Vector2.One;
        private ButtonEffectRenderer effectRenderer;

        /// <summary>Creates a button drawn with textures; its size is the size of the default texture.</summary>
        protected ButtonAbstract(Texture2D defaultTexture, Vector2 position)
        {
            SetDefaultTexture(defaultTexture);
            SetPosition(position);
            RegisterInputHandlers();
        }

        /// <summary>Creates a button drawn with solid colors.</summary>
        protected ButtonAbstract(Vector2 position, Vector2 size, Color backgroundColor)
            : base(position, size)
        {
            SetBackgroundColors(backgroundColor);
            RegisterInputHandlers();
        }

        /// <summary>The texture drawn in the normal state (null for a button drawn with colors).</summary>
        public Texture2D DefaultTexture => defaultTexture;

        public Texture2D HoverTexture => hoverTexture;

        public Texture2D MousePressedTexture => mousePressedTexture;

        public Texture2D DisabledTexture => disabledTexture;

        /// <summary>The background color in the normal state (used when there is no texture).</summary>
        public Color? BackgroundColor { get; set; }

        /// <summary>The background color under the pointer (null = a lighter <see cref="BackgroundColor"/>).</summary>
        public Color? HoverBackgroundColor { get; set; }

        /// <summary>The background color while pressed (null = a darker <see cref="BackgroundColor"/>).</summary>
        public Color? PressedBackgroundColor { get; set; }

        /// <summary>The background color while disabled (null = a gray version of <see cref="BackgroundColor"/>).</summary>
        public Color? DisabledBackgroundColor { get; set; }

        /// <summary>The tint of the normal texture when disabled and there is no disabled texture.</summary>
        public Color DisabledTint { get; set; } = new Color(150, 150, 150);

        /// <summary>The color of the border (null = no border).</summary>
        public Color? BorderColor { get; set; }

        public float BorderThickness { get; set; } = 1f;

        /// <summary>
        /// The radius of the corners: it rounds the background color and the border (not the textures), and it is the
        /// shape followed by the light effects.
        /// </summary>
        public float CornerRadius { get; set; }

        /// <summary>
        /// Shrinks the shape of the light effects on every side (eg: for a texture with transparent margins).
        /// </summary>
        public float EffectPadding { get; set; }

        /// <summary>The shape that clips the effects drawn inside the button.</summary>
        public ButtonEffectMask EffectMask { get; set; } = ButtonEffectMask.Auto;

        /// <summary>
        /// When true, the effects ignore the time scale of the game clock (eg: buttons of a pause menu keep shining
        /// while the game is paused).
        /// </summary>
        public bool EffectsUseUnscaledTime { get; set; }

        /// <summary>The light effects of the button, in drawing order.</summary>
        public IReadOnlyList<ButtonEffect> Effects => effectRenderer != null ? effectRenderer.Effects : (IReadOnlyList<ButtonEffect>)Array.Empty<ButtonEffect>();

        /// <summary>The label created by <see cref="SetText(SpriteFontBase, string, Color?)"/>, or null.</summary>
        public Label TextLabel { get; private set; }

        /// <summary>A scale applied to the background texture (and to the size of the button).</summary>
        public Vector2 BackgroundScale
        {
            get => backgroundScale;
            set
            {
                backgroundScale = value;
                MarkAsDirty();
            }
        }

        /// <summary>The current visual state.</summary>
        public ButtonVisualState VisualState
            => !IsEnabled ? ButtonVisualState.Disabled
                : IsMousePressed && IsMouseHover ? ButtonVisualState.Pressed
                : IsMouseHover ? ButtonVisualState.Hover
                : ButtonVisualState.Normal;

        public TButton SetDefaultTexture(Texture2D defaultTexture)
        {
            this.defaultTexture = defaultTexture;
            MarkAsDirty();
            return ThisAsT;
        }

        public TButton SetHoverTexture(Texture2D hoverTexture)
        {
            this.hoverTexture = hoverTexture;
            return ThisAsT;
        }

        public TButton SetMousePressedTexture(Texture2D mousePressedTexture)
        {
            this.mousePressedTexture = mousePressedTexture;
            return ThisAsT;
        }

        public TButton SetDisabledTexture(Texture2D disabledTexture)
        {
            this.disabledTexture = disabledTexture;
            return ThisAsT;
        }

        /// <summary>Sets the textures of every state (null hover/pressed textures use the default one).</summary>
        public TButton SetTextures(Texture2D normal, Texture2D hover = null, Texture2D pressed = null, Texture2D disabled = null)
        {
            SetDefaultTexture(normal);
            hoverTexture = hover;
            mousePressedTexture = pressed;
            disabledTexture = disabled;
            return ThisAsT;
        }

        /// <summary>
        /// Sets the background colors (used when there is no texture). The colors not given are derived from
        /// <paramref name="normal"/>.
        /// </summary>
        public TButton SetBackgroundColors(Color normal, Color? hover = null, Color? pressed = null, Color? disabled = null)
        {
            BackgroundColor = normal;
            HoverBackgroundColor = hover;
            PressedBackgroundColor = pressed;
            DisabledBackgroundColor = disabled;
            return ThisAsT;
        }

        public TButton SetBorder(Color color, float thickness = 1f)
        {
            BorderColor = color;
            BorderThickness = thickness;
            return ThisAsT;
        }

        public TButton SetBackgroundScale(Vector2 backgroundScale)
        {
            BackgroundScale = backgroundScale;
            return ThisAsT;
        }

        /// <summary>Sets <see cref="CornerRadius"/>: rounds the background color and the border, and the shape of the effects.</summary>
        public TButton SetCornerRadius(float cornerRadius)
        {
            CornerRadius = Math.Max(0f, cornerRadius);
            return ThisAsT;
        }

        /// <summary>Sets <see cref="EffectPadding"/>.</summary>
        public TButton SetEffectPadding(float effectPadding)
        {
            EffectPadding = Math.Max(0f, effectPadding);
            return ThisAsT;
        }

        /// <summary>Sets <see cref="EffectMask"/>.</summary>
        public TButton SetEffectMask(ButtonEffectMask effectMask)
        {
            EffectMask = effectMask;
            return ThisAsT;
        }

        /// <summary>Sets <see cref="EffectsUseUnscaledTime"/>.</summary>
        public TButton SetEffectsUseUnscaledTime(bool effectsUseUnscaledTime)
        {
            EffectsUseUnscaledTime = effectsUseUnscaledTime;
            return ThisAsT;
        }

        /// <summary>
        /// Adds a light effect. The effects are drawn in the order they are added (within their layer). An effect
        /// belongs to one button: create a new one for each button.
        /// </summary>
        /// <example>
        /// <code>
        /// button.SetCornerRadius(12)
        ///     .AddEffect(new GlowEffect().SetColor(Color.Gold).SetRadius(16).SetPulse(1f, 0.4f))
        ///     .AddEffect(new ShineSweepEffect());
        /// </code>
        /// </example>
        public TButton AddEffect(ButtonEffect effect)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));
            if (ReferenceEquals(effect.Owner, this))
                return ThisAsT;
            if (effect.Owner != null)
                throw new InvalidOperationException("The effect already belongs to another button. Create a new effect for each button.");

            effect.Owner = this;
            (effectRenderer ??= new ButtonEffectRenderer(this)).Effects.Add(effect);
            return ThisAsT;
        }

        /// <summary>Adds light effects (eg: <c>AddEffects(ButtonEffectPresets.Gold())</c>).</summary>
        public TButton AddEffects(IEnumerable<ButtonEffect> effects)
        {
            if (effects == null)
                throw new ArgumentNullException(nameof(effects));
            foreach (var effect in effects)
                AddEffect(effect);
            return ThisAsT;
        }

        /// <summary>Adds light effects.</summary>
        public TButton AddEffects(params ButtonEffect[] effects) => AddEffects((IEnumerable<ButtonEffect>)effects);

        /// <summary>Removes a light effect (it can then be added to another button).</summary>
        public TButton RemoveEffect(ButtonEffect effect)
        {
            if (effect != null && effectRenderer != null && effectRenderer.Effects.Remove(effect))
                effect.Owner = null;
            return ThisAsT;
        }

        /// <summary>Removes every light effect.</summary>
        public TButton ClearEffects()
        {
            if (effectRenderer == null)
                return ThisAsT;
            var effects = effectRenderer.Effects;
            for (var i = 0; i < effects.Count; i++)
                effects[i].Owner = null;
            effects.Clear();
            return ThisAsT;
        }

        /// <summary>Removes every light effect and adds new ones (eg: <c>SetEffects(ButtonEffectPresets.Magic())</c>).</summary>
        public TButton SetEffects(IEnumerable<ButtonEffect> effects) => ClearEffects().AddEffects(effects);

        /// <summary>Returns the first effect of a type, or null.</summary>
        public TEffect FindEffect<TEffect>() where TEffect : ButtonEffect
        {
            if (effectRenderer == null)
                return null;
            var effects = effectRenderer.Effects;
            for (var i = 0; i < effects.Count; i++)
            {
                if (effects[i] is TEffect effect)
                    return effect;
            }

            return null;
        }

        /// <summary>Shows a text centered in the button (the label is created once and updated afterwards).</summary>
        public TButton SetText(SpriteFontBase font, string text, Color? color = null)
        {
            if (TextLabel == null || TextLabel.IsDisposed)
            {
                TextLabel = new Label(font, text, Vector2.Zero, color ?? Color.White)
                    .SetAnchor(Anchor.Center);
                AddChild(TextLabel);
            }
            else
            {
                TextLabel.SetFont(font).SetText(text);
                if (color.HasValue)
                    TextLabel.SetColor(color.Value);
            }
            return ThisAsT;
        }

        /// <summary>Changes the text of the label created by <see cref="SetText(SpriteFontBase, string, Color?)"/>.</summary>
        public TButton SetText(string text)
        {
            TextLabel?.SetText(text);
            return ThisAsT;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var effects = effectRenderer != null && effectRenderer.HasEffects ? effectRenderer : null;
            effects?.DrawLayer(spriteBatch, ButtonEffectLayer.Behind);

            var state = VisualState;
            var texture = GetTexture(state);
            var opacity = NestedOpacity;
            var rounded = CornerRadius > 0f && ServiceProvider.GraphicsDevice != null;
            var radius = Math.Min(CornerRadius, Math.Min(SizeWithoutScale.X, SizeWithoutScale.Y) / 2f);

            if (texture != null)
            {
                var tint = state == ButtonVisualState.Disabled && ReferenceEquals(texture, GetNormalTexture()) && disabledTexture == null
                    ? DisabledTint
                    : Color;
                var area = new RectangleF(Vector2.Zero, texture.Size().ToVector2() * backgroundScale);
                DrawLocalTexture(spriteBatch, texture, null, area, tint * opacity);
            }
            else
            {
                var background = GetBackgroundColor(state);
                if (background.HasValue)
                {
                    if (rounded)
                        DrawLocalFrame(spriteBatch, ButtonEffectResources.GetRoundedMask(radius, ButtonEffectResources.GetDensity(NestedScale)), background.Value * opacity);
                    else
                        DrawLocalRectangle(spriteBatch, LocalBounds, background.Value * opacity);
                }
            }

            if (BorderColor.HasValue)
            {
                if (rounded)
                    DrawLocalFrame(spriteBatch, ButtonEffectResources.GetRoundedBorder(radius, BorderThickness, ButtonEffectResources.GetDensity(NestedScale)), BorderColor.Value * opacity);
                else
                    DrawLocalBorder(spriteBatch, LocalBounds, BorderColor.Value * opacity, BorderThickness);
            }

            effects?.DrawInside(spriteBatch);
            base.Draw(spriteBatch);
            effects?.DrawLayer(spriteBatch, ButtonEffectLayer.Front);
        }

        public override void FireOnUpdateEvent(GameTime gameTime)
        {
            if (effectRenderer != null && effectRenderer.HasEffects)
            {
                var deltaSeconds = EffectsUseUnscaledTime ? (float)gameTime.ElapsedGameTime.TotalSeconds : GetScaledDeltaSeconds(gameTime);
                effectRenderer.Update(deltaSeconds, VisualState);
            }

            base.FireOnUpdateEvent(gameTime);
        }

        public override void OnBeforeDraw()
        {
            base.OnBeforeDraw();
            effectRenderer?.Prepare();
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing && effectRenderer != null)
            {
                ClearEffects();
                effectRenderer.Dispose();
                effectRenderer = null;
            }

            base.Dispose(disposing);
        }

        IControl IButtonEffectHost.Control => this;

        Color? IButtonEffectHost.EffectBackgroundColor
        {
            get
            {
                var state = VisualState;
                return GetTexture(state) != null ? (Color?)null : GetBackgroundColor(state);
            }
        }

        Texture2D IButtonEffectHost.GetEffectMaskTexture(out RectangleF area)
        {
            var texture = GetTexture(VisualState);
            area = texture != null ? new RectangleF(Vector2.Zero, texture.Size().ToVector2() * backgroundScale) : RectangleF.Empty;
            return texture;
        }

        /// <summary>Draws a generated frame (rounded background or border) over the whole button.</summary>
        private void DrawLocalFrame(SpriteBatch spriteBatch, LightFrame frame, Color color)
            => DrawLocalNineSlice(spriteBatch, frame.Texture, null, new Thickness(frame.SourceBorder), frame.GetArea(LocalBounds),
                new Thickness(frame.DestinationBorder), color);

        /// <summary>The texture of the normal state (overridden by toggle buttons).</summary>
        protected virtual Texture2D GetNormalTexture() => defaultTexture;

        /// <summary>The texture drawn in a state, or null to draw the background color.</summary>
        protected virtual Texture2D GetTexture(ButtonVisualState state)
        {
            var normal = GetNormalTexture();
            if (normal == null)
                return null;

            switch (state)
            {
                case ButtonVisualState.Pressed:
                    return mousePressedTexture ?? GetHoverTexture() ?? normal;
                case ButtonVisualState.Hover:
                    return GetHoverTexture() ?? normal;
                case ButtonVisualState.Disabled:
                    return disabledTexture ?? normal;
                default:
                    return normal;
            }
        }

        /// <summary>The hover texture (overridden by toggle buttons).</summary>
        protected virtual Texture2D GetHoverTexture() => hoverTexture;

        /// <summary>The background color of the normal state (overridden by toggle buttons).</summary>
        protected virtual Color? GetNormalBackgroundColor() => BackgroundColor;

        /// <summary>The background color drawn in a state when there is no texture.</summary>
        protected virtual Color? GetBackgroundColor(ButtonVisualState state)
        {
            var normal = GetNormalBackgroundColor();
            if (!normal.HasValue)
                return null;

            switch (state)
            {
                case ButtonVisualState.Pressed:
                    return PressedBackgroundColor ?? Shade(normal.Value, 0.75f);
                case ButtonVisualState.Hover:
                    return HoverBackgroundColor ?? Lighten(normal.Value, 0.2f);
                case ButtonVisualState.Disabled:
                    return DisabledBackgroundColor ?? Grayscale(normal.Value);
                default:
                    return normal;
            }
        }

        protected override Vector2 CalculateSize()
        {
            var normal = GetNormalTexture() ?? defaultTexture;
            return normal != null ? normal.Size().ToVector2() * backgroundScale : SizeWithoutScale;
        }

        /// <summary>Multiplies the RGB components of a color.</summary>
        protected static Color Shade(Color color, float factor)
            => new Color((int)(color.R * factor), (int)(color.G * factor), (int)(color.B * factor), color.A);

        /// <summary>Moves the RGB components of a color towards white.</summary>
        protected static Color Lighten(Color color, float amount)
            => new Color(
                (int)(color.R + (color.A - color.R) * amount),
                (int)(color.G + (color.A - color.G) * amount),
                (int)(color.B + (color.A - color.B) * amount),
                color.A);

        /// <summary>A gray color with the luminance of the given color.</summary>
        protected static Color Grayscale(Color color)
        {
            var luminance = (int)(color.R * 0.3f + color.G * 0.59f + color.B * 0.11f);
            return new Color(luminance, luminance, luminance, color.A);
        }

        private void RegisterInputHandlers()
        {
            // The handlers make the button own the press and the release: the controls below are not clicked.
            AddOnMousePressed(OnButtonPressed);
            AddOnMouseReleased(OnButtonReleased);
        }

        private void OnButtonPressed(ControlMouseEventArgs args)
        {
            if (effectRenderer != null && effectRenderer.HasEffects)
                effectRenderer.OnPressed(ToLocalPosition(args.Position));
        }

        private void OnButtonReleased(ControlMouseEventArgs args)
        {
            if (effectRenderer != null && effectRenderer.HasEffects)
                effectRenderer.OnReleased(ToLocalPosition(args.Position));
        }
    }
}
