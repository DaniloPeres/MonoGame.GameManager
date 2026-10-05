using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.Extensions;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A box that is checked and unchecked by clicking it (or its text), drawn with colors or with two textures.
    /// </summary>
    /// <example>
    /// <code>
    /// new Checkbox(new Vector2(20, 20))
    ///     .SetLabel(font, "Full screen")
    ///     .SetIsChecked(settings.IsFullScreen, notify: false)
    ///     .AddOnCheckedChanged(isChecked => windowManager.SetFullScreen(isChecked))
    ///     .AddToScreen();
    /// </code>
    /// </example>
    public class Checkbox : ContainerAbstract<Checkbox>
    {
        private readonly Texture2D uncheckedTexture;
        private readonly Texture2D checkedTexture;
        private Action<bool> onCheckedChanged;
        private float boxSize;
        private float labelSpacing = 8f;

        /// <summary>Creates a checkbox drawn with colors.</summary>
        public Checkbox(Vector2 position, float boxSize = 24f)
            : base(position, new Vector2(boxSize))
        {
            this.boxSize = boxSize;
            AddOnClick(args => Toggle());
        }

        /// <summary>Creates a checkbox drawn with a texture for each state.</summary>
        public Checkbox(Texture2D uncheckedTexture, Texture2D checkedTexture, Vector2 position)
            : base(position, Vector2.Zero)
        {
            this.uncheckedTexture = uncheckedTexture ?? throw new ArgumentNullException(nameof(uncheckedTexture));
            this.checkedTexture = checkedTexture ?? throw new ArgumentNullException(nameof(checkedTexture));
            AddOnClick(args => Toggle());
        }

        public bool IsChecked { get; private set; }

        /// <summary>The color of the box (when drawn with colors).</summary>
        public Color BoxColor { get; set; } = new Color(40, 40, 40);

        /// <summary>The color of the check mark (when drawn with colors).</summary>
        public Color CheckColor { get; set; } = Color.White;

        /// <summary>The color of the border of the box (when drawn with colors).</summary>
        public Color BorderColor { get; set; } = Color.White;

        public float BorderThickness { get; set; } = 2f;

        /// <summary>The text next to the box created by <see cref="SetLabel"/>, or null.</summary>
        public Label Label { get; private set; }

        /// <summary>The space between the box and the text.</summary>
        public float LabelSpacing
        {
            get => labelSpacing;
            set
            {
                labelSpacing = value;
                MarkSizeAsDirty();
            }
        }

        /// <summary>Changes the state. The callbacks are invoked when it changes and <paramref name="notify"/> is true.</summary>
        public Checkbox SetIsChecked(bool isChecked, bool notify = true)
        {
            if (IsChecked == isChecked)
                return this;
            IsChecked = isChecked;
            if (notify)
                onCheckedChanged?.Invoke(isChecked);
            return this;
        }

        public Checkbox Toggle() => SetIsChecked(!IsChecked);

        public Checkbox SetColors(Color box, Color check, Color? border = null)
        {
            BoxColor = box;
            CheckColor = check;
            if (border.HasValue)
                BorderColor = border.Value;
            return this;
        }

        /// <summary>Shows a text next to the box (created once, updated afterwards).</summary>
        public Checkbox SetLabel(SpriteFont spriteFont, string text, Color? color = null)
        {
            if (Label == null || Label.IsDisposed)
            {
                Label = new Label(spriteFont, text, Vector2.Zero, color ?? Color.White).SetAnchor(Anchor.CenterLeft);
                AddChild(Label);
            }
            else
            {
                Label.SetSpriteFont(spriteFont).SetText(text);
                if (color.HasValue)
                    Label.SetColor(color.Value);
            }
            MarkSizeAsDirty();
            return this;
        }

        /// <summary>Adds a callback invoked with the new state when it changes.</summary>
        public Checkbox AddOnCheckedChanged(Action<bool> onCheckedChanged)
        {
            this.onCheckedChanged += onCheckedChanged;
            return this;
        }

        public Checkbox RemoveOnCheckedChanged(Action<bool> onCheckedChanged)
        {
            this.onCheckedChanged -= onCheckedChanged;
            return this;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var box = GetBoxArea();
            var opacity = NestedOpacity * (IsEnabled ? 1f : 0.5f);

            if (uncheckedTexture != null)
            {
                DrawLocalTexture(spriteBatch, IsChecked ? checkedTexture : uncheckedTexture, null, box, Color * opacity);
            }
            else
            {
                DrawLocalRectangle(spriteBatch, box, BoxColor * opacity);
                DrawLocalBorder(spriteBatch, box, BorderColor * opacity, BorderThickness);
                if (IsChecked)
                {
                    var inset = box.Width * 0.25f;
                    DrawLocalRectangle(spriteBatch, box.Inflate(-inset, -inset), CheckColor * opacity);
                }
            }

            base.Draw(spriteBatch);
        }

        protected override void ArrangeChildren()
        {
            var size = Measure();
            if (SizeWithoutScale != size)
                Size = size;

            if (Label != null && !Label.IsDisposed)
            {
                var position = new Vector2(GetBoxSize().X + labelSpacing, 0f);
                if (Label.PositionAnchor != position)
                    Label.PositionAnchor = position;
            }
        }

        protected override Vector2 CalculateSize() => Measure();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                onCheckedChanged = null;
            base.Dispose(disposing);
        }

        private Vector2 GetBoxSize()
        {
            if (uncheckedTexture == null)
                return new Vector2(boxSize);
            return Vector2.Max(uncheckedTexture.Size().ToVector2(), checkedTexture.Size().ToVector2());
        }

        private RectangleF GetBoxArea()
        {
            var box = GetBoxSize();
            return new RectangleF(0f, (SizeWithoutScale.Y - box.Y) / 2f, box.X, box.Y);
        }

        private Vector2 Measure()
        {
            var box = GetBoxSize();
            if (Label == null || Label.IsDisposed || !Label.IsVisible)
                return box;

            var scale = NestedScale;
            var labelSize = new Vector2(scale.X != 0f ? Label.Size.X / scale.X : 0f, scale.Y != 0f ? Label.Size.Y / scale.Y : 0f);
            return new Vector2(box.X + labelSpacing + labelSize.X, Math.Max(box.Y, labelSize.Y));
        }
    }
}
