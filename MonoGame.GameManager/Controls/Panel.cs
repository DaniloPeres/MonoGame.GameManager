using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Services;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A container that groups controls. Children are positioned relative to the panel.
    /// </summary>
    public class Panel : ContainerAbstract<Panel>
    {
        /// <summary>Creates a panel with the size of the virtual screen.</summary>
        public Panel() : this(Vector2.Zero, (ServiceProvider.ScreenManager?.ScreenSize ?? Point.Zero).ToVector2()) { }

        public Panel(Rectangle destinationRectangle) : this(destinationRectangle.Location.ToVector2(), destinationRectangle.Size.ToVector2()) { }

        public Panel(Vector2 position, Vector2 size)
            : base(position, size) { }
    }
}
