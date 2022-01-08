using Microsoft.Xna.Framework;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Timers;

namespace Tic_Tac_Toe.Screens
{
    public class StartupScreen : ScreenManager
    {
        private static Point screenSize = new Point(400, 600);
        public StartupScreen() : base(screenSize, new TicTacToeScreen())
        {
            Window.AllowUserResizing = true;

            ScreenBackgroundColor = Color.BlueViolet;
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
