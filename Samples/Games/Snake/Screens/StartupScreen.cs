using Microsoft.Xna.Framework;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Screens.Transitions;

namespace Snake.Screens
{
    internal class StartupScreen : ScreenManager
    {
        private static Point screenSize = new Point(460, 320);
        public StartupScreen() : base(screenSize, new MainMenuScreen())
        {
            Window.AllowUserResizing = true;

            DefaultTransition = new FadeTransition();

            WindowBackgroundColor = UiHelper.DarkBackgroundColor;
            ScreenBackgroundColor = UiHelper.LightBackgroundColor;
        }

        protected override void Initialize()
        {
            Graphics.PreferredBackBufferWidth = (int)(ScreenSize.X * 1.5f);
            Graphics.PreferredBackBufferHeight = (int)(ScreenSize.Y * 1.5f);
            Graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            ContentHandler.Instance.LoadAllContents();
            base.LoadContent();
        }
    }
}
