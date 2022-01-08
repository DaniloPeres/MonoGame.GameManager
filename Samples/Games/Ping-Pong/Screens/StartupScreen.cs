using Microsoft.Xna.Framework;
using MonoGame.GameManager.Screens;

namespace Ping_Pong.Screens
{
    public class StartupScreen : ScreenManager
    {
        private static Point screenSize = new Point(600, 400);
        public StartupScreen() : base(screenSize, new PingPongScreen())
        {
            Window.AllowUserResizing = true;

            ScreenBackgroundColor = Color.DarkSlateGray;
        }

        protected override void Initialize()
        {
            Graphics.PreferredBackBufferWidth = (int)(ScreenSize.X * 1.5f);
            Graphics.PreferredBackBufferHeight = (int)(ScreenSize.Y * 1.5f);
            Graphics.ApplyChanges();

            base.Initialize();
        }
    }
}
