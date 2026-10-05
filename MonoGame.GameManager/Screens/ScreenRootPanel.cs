using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;

namespace MonoGame.GameManager.Screens
{
    /// <summary>
    /// The root panel of a screen. Its update events stop while the screen is covered and paused.
    /// </summary>
    internal sealed class ScreenRootPanel : Panel
    {
        public ScreenRootPanel(Vector2 size) : base(Vector2.Zero, size) { }

        public bool UpdateEnabled { get; set; } = true;

        public override void FireOnUpdateEvent(GameTime gameTime)
        {
            if (UpdateEnabled)
                base.FireOnUpdateEvent(gameTime);
        }
    }
}
