using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using System;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A button that switches between two states (on/off) when clicked, with its own textures or colors for the
    /// "on" state.
    /// </summary>
    /// <example>
    /// <code>
    /// new ToggleButton(new Vector2(20, 20), new Vector2(120, 40), Color.DimGray)
    ///     .SetToggledBackgroundColor(Color.SeaGreen)
    ///     .SetText(font, "Sound")
    ///     .AddOnToggled(isOn => audio.IsMuted = !isOn)
    ///     .AddToScreen();
    /// </code>
    /// </example>
    public class ToggleButton : ButtonAbstract<ToggleButton>
    {
        private Action<bool> onToggled;
        private Texture2D toggledTexture;
        private Texture2D toggledHoverTexture;

        public ToggleButton(Texture2D defaultTexture, Vector2 position)
            : base(defaultTexture, position)
        {
            AddOnClick(args => Toggle());
        }

        public ToggleButton(Vector2 position, Vector2 size, Color backgroundColor)
            : base(position, size, backgroundColor)
        {
            AddOnClick(args => Toggle());
        }

        public bool IsToggled { get; private set; }

        /// <summary>The background color of the "on" state (used when there is no texture).</summary>
        public Color? ToggledBackgroundColor { get; set; }

        /// <summary>Changes the state. <see cref="AddOnToggled"/> callbacks are invoked when it changes and <paramref name="notify"/> is true.</summary>
        public ToggleButton SetIsToggled(bool isToggled, bool notify = true)
        {
            if (IsToggled == isToggled)
                return this;
            IsToggled = isToggled;
            if (toggledTexture != null)
                MarkAsDirty(); // the "on" texture can have another size
            if (notify)
                onToggled?.Invoke(isToggled);
            return this;
        }

        public ToggleButton Toggle() => SetIsToggled(!IsToggled);

        /// <summary>Sets the textures of the "on" state (null = the normal textures).</summary>
        public ToggleButton SetToggledTextures(Texture2D toggled, Texture2D toggledHover = null)
        {
            toggledTexture = toggled;
            toggledHoverTexture = toggledHover;
            return this;
        }

        public ToggleButton SetToggledBackgroundColor(Color color)
        {
            ToggledBackgroundColor = color;
            return this;
        }

        /// <summary>Adds a callback invoked with the new state when it changes.</summary>
        public ToggleButton AddOnToggled(Action<bool> onToggled)
        {
            this.onToggled += onToggled;
            return this;
        }

        public ToggleButton RemoveOnToggled(Action<bool> onToggled)
        {
            this.onToggled -= onToggled;
            return this;
        }

        protected override Texture2D GetNormalTexture() => IsToggled && toggledTexture != null ? toggledTexture : base.GetNormalTexture();

        protected override Texture2D GetHoverTexture() => IsToggled && toggledTexture != null ? toggledHoverTexture : base.GetHoverTexture();

        protected override Color? GetNormalBackgroundColor() => IsToggled && ToggledBackgroundColor.HasValue ? ToggledBackgroundColor : base.GetNormalBackgroundColor();

        protected override Color? GetBackgroundColor(ButtonVisualState state)
        {
            // Explicit hover/pressed colors belong to the "off" state: derive them from the "on" color instead.
            if (IsToggled && ToggledBackgroundColor.HasValue)
            {
                switch (state)
                {
                    case ButtonVisualState.Pressed:
                        return Shade(ToggledBackgroundColor.Value, 0.75f);
                    case ButtonVisualState.Hover:
                        return Lighten(ToggledBackgroundColor.Value, 0.2f);
                }
            }
            return base.GetBackgroundColor(state);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                onToggled = null;
            base.Dispose(disposing);
        }
    }
}
