using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Services;

namespace MonoGame.GameManager.Controls
{
    public class Panel : ContainerAbstract<Panel>
    {

        public Panel() : this(Vector2.Zero, ServiceProvider.ScreenManager.ScreenSize.ToVector2()) { }

        public Panel(Rectangle destinationRectangle) : this(destinationRectangle.Location.ToVector2(), destinationRectangle.Size.ToVector2()) { }

        public Panel(Vector2 position, Vector2 size)
            : base(position, size) { }
    }
}
