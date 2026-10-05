using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Services;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A container that groups controls. Children are positioned relative to the panel. It can draw a background
    /// color and a border.
    /// </summary>
    public class Panel : ContainerAbstract<Panel>
    {
        /// <summary>Creates a panel with the size of the virtual screen.</summary>
        public Panel() : this(Vector2.Zero, (ServiceProvider.ScreenManager?.ScreenSize ?? Point.Zero).ToVector2()) { }

        public Panel(Rectangle destinationRectangle) : this(destinationRectangle.Location.ToVector2(), destinationRectangle.Size.ToVector2()) { }

        public Panel(Vector2 position, Vector2 size)
            : base(position, size) { }

        /// <summary>The color drawn behind the children (null = none).</summary>
        public Color? BackgroundColor { get; set; }

        /// <summary>The color of the border drawn over the children (null = none).</summary>
        public Color? BorderColor { get; set; }

        /// <summary>The thickness of the border, in local units.</summary>
        public float BorderThickness { get; set; } = 1f;

        public Panel SetBackgroundColor(Color? backgroundColor)
        {
            BackgroundColor = backgroundColor;
            return this;
        }

        public Panel SetBorder(Color color, float thickness = 1f)
        {
            BorderColor = color;
            BorderThickness = thickness;
            return this;
        }

        public Panel RemoveBorder()
        {
            BorderColor = null;
            return this;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var opacity = NestedOpacity;
            if (BackgroundColor.HasValue)
                DrawLocalRectangle(spriteBatch, LocalBounds, BackgroundColor.Value * opacity);

            base.Draw(spriteBatch);

            if (BorderColor.HasValue)
                DrawLocalBorder(spriteBatch, LocalBounds, BorderColor.Value * opacity, BorderThickness);
        }
    }
}
