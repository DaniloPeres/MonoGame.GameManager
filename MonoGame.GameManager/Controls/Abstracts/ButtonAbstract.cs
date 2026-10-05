using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.Extensions;
using MonoGame.GameManager.GameMath;

namespace MonoGame.GameManager.Controls.Abstracts
{
    /// <summary>
    /// Base class of the buttons: drawn with textures (one per state) or with solid colors, with an optional text.
    /// Other controls can be added to it (icons, labels...).
    /// </summary>
    /// <remarks>
    /// The texture or the color of the current <see cref="VisualState"/> is chosen when drawing. A button owns the
    /// presses it receives, so the controls below it are never clicked at the same time.
    /// </remarks>
    public abstract class ButtonAbstract<TButton> : ContainerAbstract<TButton> where TButton : ButtonAbstract<TButton>
    {
        private Texture2D defaultTexture;
        private Texture2D hoverTexture;
        private Texture2D mousePressedTexture;
        private Texture2D disabledTexture;
        private Vector2 backgroundScale = Vector2.One;

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

        /// <summary>The label created by <see cref="SetText(SpriteFont, string, Color?)"/>, or null.</summary>
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

        /// <summary>Shows a text centered in the button (the label is created once and updated afterwards).</summary>
        public TButton SetText(SpriteFont spriteFont, string text, Color? color = null)
        {
            if (TextLabel == null || TextLabel.IsDisposed)
            {
                TextLabel = new Label(spriteFont, text, Vector2.Zero, color ?? Color.White)
                    .SetAnchor(Anchor.Center);
                AddChild(TextLabel);
            }
            else
            {
                TextLabel.SetSpriteFont(spriteFont).SetText(text);
                if (color.HasValue)
                    TextLabel.SetColor(color.Value);
            }
            return ThisAsT;
        }

        /// <summary>Changes the text of the label created by <see cref="SetText(SpriteFont, string, Color?)"/>.</summary>
        public TButton SetText(string text)
        {
            TextLabel?.SetText(text);
            return ThisAsT;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var state = VisualState;
            var texture = GetTexture(state);
            var opacity = NestedOpacity;

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
                    DrawLocalRectangle(spriteBatch, LocalBounds, background.Value * opacity);
            }

            if (BorderColor.HasValue)
                DrawLocalBorder(spriteBatch, LocalBounds, BorderColor.Value * opacity, BorderThickness);

            base.Draw(spriteBatch);
        }

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

        private static void OnButtonPressed(ControlMouseEventArgs args) { }

        private static void OnButtonReleased(ControlMouseEventArgs args) { }
    }
}
