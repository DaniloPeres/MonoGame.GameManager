using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Managers;
using MonoGame.GameManager.Screens.Transitions;
using MonoGame.GameManager.Services;
using System;

namespace MonoGame.GameManager.Screens
{
    public abstract class Screen : IDisposable
    {
        public Panel Root => ServiceProvider.RootPanel;
        public ScreenManager ScreenManager => ServiceProvider.ScreenManager;
        public ContentLoaderManager ContentLoader => ServiceProvider.ContentLoaderManager;
        public GameTime GameTime => ScreenManager.GameTime;

        /// <summary>
        /// Override this to load graphical resources required by the screen.
        /// </summary>
        public virtual void LoadContent() { }

        /// <summary>
        /// Override this to inform when the screen has been initialized.
        /// </summary>
        public virtual void OnInit() { }

        /// <summary>
        /// Override this to inform when the screen has to be dispose something on the screen
        /// </summary>
        public virtual void Dispose() { }

        public void ChangeScreen(Screen screen) => ScreenManager.ChangeScreen(screen);
        public void ChangeScreenWithNoTransition(Screen screen) => ScreenManager.ChangeScreenWithNoTransition(screen);
        public void ChangeScreen(Screen screen, ITransition transition = null) => ScreenManager.ChangeScreen(screen, transition);
    }
}
