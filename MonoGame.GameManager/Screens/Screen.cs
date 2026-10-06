using Microsoft.Xna.Framework;
using MonoGame.GameManager.Audio;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Managers;
using MonoGame.GameManager.Screens.Transitions;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Services.Inputs;
using MonoGame.GameManager.Timers;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Screens
{
    /// <summary>
    /// A screen of the game (menu, level, pause dialog...). Override <see cref="LoadContent"/> to load assets,
    /// <see cref="OnInit"/> to create the controls and <see cref="Update"/> for the game logic.
    /// </summary>
    /// <remarks>
    /// Everything that belongs to a screen is released when it is closed: its controls (<see cref="Root"/>), the
    /// animations and timers of its <see cref="Scheduler"/>, the assets of its <see cref="Content"/> loader and
    /// the resources registered with <see cref="RegisterDisposable{T}"/>.
    /// </remarks>
    public abstract class Screen : IDisposable
    {
        private readonly List<IDisposable> disposables = new List<IDisposable>();
        private ScreenManager screenManager;
        private bool isCovered;

        /// <summary>The root panel of the screen. Controls added with <c>AddToScreen()</c> go here.</summary>
        public Panel Root { get; private set; }

        /// <summary>
        /// Updates the animations, timers and scheduled actions of the screen. It is cleared when the screen is
        /// closed and paused while the screen is covered (unless <see cref="UpdateWhenCovered"/> is true).
        /// </summary>
        public IScheduler Scheduler { get; } = new Scheduler();

        /// <summary>Loads assets that are released when the screen is closed.</summary>
        public IContentLoader Content { get; private set; }

        [Obsolete("Use Content (assets released with the screen) or ServiceProvider.ContentLoader (assets kept for the whole game).")]
        public IContentLoader ContentLoader => ServiceProvider.ContentLoader;

        public ScreenManager ScreenManager => screenManager ?? ServiceProvider.ScreenManager;

        public GameTime GameTime => ServiceProvider.GameTime;

        public IInputManager Input => ServiceProvider.Input;

        public IAudioManager Audio => ServiceProvider.Audio;

        /// <summary>True while the screen is open and on top of the stack.</summary>
        public bool IsActive => screenManager != null && !IsDisposed && !isCovered;

        /// <summary>True while another screen is open on top of this one.</summary>
        public bool IsCovered => isCovered;

        /// <summary>True after the screen was closed.</summary>
        public bool IsDisposed { get; private set; }

        /// <summary>When true, the screen keeps updating while another screen covers it.</summary>
        public virtual bool UpdateWhenCovered => false;

        /// <summary>When true (default), the screen is still drawn while another screen covers it.</summary>
        public virtual bool DrawWhenCovered => true;

        /// <summary>
        /// When true (default), the screens below this one do not receive pointer input while it is open.
        /// </summary>
        public virtual bool IsModal => true;

        /// <summary>
        /// Override this to load graphical resources required by the screen.
        /// </summary>
        public virtual void LoadContent() { }

        /// <summary>
        /// Override this to create the controls of the screen, after the content was loaded.
        /// </summary>
        public virtual void OnInit() { }

        /// <summary>
        /// Override this for the logic of the screen. It is called every frame while the screen is active
        /// (or covered, when <see cref="UpdateWhenCovered"/> is true).
        /// </summary>
        public virtual void Update(GameTime gameTime) { }

        /// <summary>Called when another screen is opened on top of this one.</summary>
        public virtual void OnCovered() { }

        /// <summary>Called when the screen on top of this one is closed.</summary>
        public virtual void OnUncovered() { }

        /// <summary>
        /// Override this to dispose resources when the screen is closed. The controls, scheduler and content of the
        /// screen are released automatically after this method.
        /// </summary>
        public virtual void Dispose() { }

        public void ChangeScreen(Screen screen) => ScreenManager.ChangeScreen(screen);

        public void ChangeScreenWithNoTransition(Screen screen) => ScreenManager.ChangeScreenWithNoTransition(screen);

        public void ChangeScreen(Screen screen, ITransition transition = null) => ScreenManager.ChangeScreen(screen, transition);

        /// <summary>Opens a screen on top of this one (eg: a pause menu).</summary>
        public void PushScreen(Screen screen, ITransition transition = null) => ScreenManager.PushScreen(screen, transition);

        /// <summary>Closes the screen on top of the stack.</summary>
        public void PopScreen(ITransition transition = null) => ScreenManager.PopScreen(transition);

        /// <summary>
        /// Registers a resource (generated texture, render target...) that is disposed when the screen is closed.
        /// </summary>
        public T RegisterDisposable<T>(T disposable) where T : IDisposable
        {
            if (disposable != null)
                disposables.Add(disposable);
            return disposable;
        }

        internal void Attach(ScreenManager manager, Panel root, IContentLoader content)
        {
            screenManager = manager;
            Root = root;
            Content = content;
        }

        internal void SetCovered(bool covered)
        {
            if (isCovered == covered)
                return;

            isCovered = covered;
            if (covered)
                OnCovered();
            else
                OnUncovered();
        }

        /// <summary>Releases everything that belongs to the screen (after the user <see cref="Dispose"/>).</summary>
        internal void Release()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;

            Scheduler.Clear();
            Root?.Dispose();

            for (var i = disposables.Count - 1; i >= 0; i--)
                disposables[i].Dispose();
            disposables.Clear();

            Content?.Dispose();
        }
    }
}
