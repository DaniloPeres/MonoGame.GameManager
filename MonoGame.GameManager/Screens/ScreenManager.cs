using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Managers;
using MonoGame.GameManager.Screens.Transitions;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Screens
{
    /// <summary>
    /// The game class: it owns the graphics device, the virtual resolution and the stack of open screens, and runs
    /// the update and draw loop of the library.
    /// </summary>
    /// <remarks>
    /// Every frame: the input devices are polled and the pointer events are delivered to the controls, the update
    /// events of the controls are fired, then each active screen updates its <see cref="Screen.Scheduler"/> and
    /// calls <see cref="Screen.Update"/>, then the audio and the global scheduler are updated and, at last, the
    /// pending screen navigations are applied. Everything is drawn into a render target of the virtual resolution
    /// that is scaled to fit the window.
    /// </remarks>
    public class ScreenManager : Game, IScreenNavigator
    {
        public readonly GraphicsDeviceManager Graphics;

        private readonly List<Screen> screens = new List<Screen>();
        private readonly Queue<NavigationRequest> pendingNavigations = new Queue<NavigationRequest>();
        private Screen initialScreen;
        private ControlManager controlManager;
        private SpriteBatch spriteBatch;
        private RenderTarget2D renderTarget;
        private bool isNavigating;
        private bool isCleanedUp;

        /// <param name="screenSize">The virtual resolution of the game.</param>
        /// <param name="initialScreen">The first screen.</param>
        public ScreenManager(Point screenSize, Screen initialScreen)
            : this(new ScreenManagerSettings { VirtualResolution = screenSize }, initialScreen) { }

        /// <param name="settings">The configuration of the game.</param>
        /// <param name="initialScreen">The first screen.</param>
        public ScreenManager(ScreenManagerSettings settings, Screen initialScreen)
        {
            Settings = settings ?? new ScreenManagerSettings();
            this.initialScreen = initialScreen ?? throw new ArgumentNullException(nameof(initialScreen));

            Graphics = new GraphicsDeviceManager(this)
            {
                SupportedOrientations = Settings.SupportedOrientations,
                IsFullScreen = Settings.IsFullScreen
            };
            if (Settings.WindowSize.HasValue)
            {
                Graphics.PreferredBackBufferWidth = Settings.WindowSize.Value.X;
                Graphics.PreferredBackBufferHeight = Settings.WindowSize.Value.Y;
            }
            if (Settings.SynchronizeWithVerticalRetrace.HasValue)
                Graphics.SynchronizeWithVerticalRetrace = Settings.SynchronizeWithVerticalRetrace.Value;
            if (Settings.IsFixedTimeStep.HasValue)
                IsFixedTimeStep = Settings.IsFixedTimeStep.Value;

            Content.RootDirectory = Settings.ContentRootDirectory;
            IsMouseVisible = Settings.IsMouseVisible;
            if (Settings.AllowUserResizing)
                Window.AllowUserResizing = true;
            if (!string.IsNullOrEmpty(Settings.Title))
                Window.Title = Settings.Title;

            ServiceProvider.Initialize(this);

            if (WindowManager is GameWindowManager gameWindowManager)
                gameWindowManager.Init(Settings.VirtualResolution);
            else
                WindowManager.ScreenSize = Settings.VirtualResolution;
        }

        /// <summary>The configuration of the game.</summary>
        public ScreenManagerSettings Settings { get; }

        public IGameWindowManager WindowManager => ServiceProvider.GameWindowManager;

        /// <summary>The virtual resolution.</summary>
        public Point ScreenSize => WindowManager.ScreenSize;

        public Rectangle ScreenRectangle => WindowManager.GetScreenRectangle();

        /// <summary>The transition used by <see cref="ChangeScreen(Screen)"/>.</summary>
        public ITransition DefaultTransition
        {
            get => Settings.DefaultTransition;
            set => Settings.DefaultTransition = value;
        }

        public Color ScreenBackgroundColor
        {
            get => Settings.ScreenBackgroundColor;
            set => Settings.ScreenBackgroundColor = value;
        }

        public Color WindowBackgroundColor
        {
            get => Settings.WindowBackgroundColor;
            set => Settings.WindowBackgroundColor = value;
        }

        /// <summary>The time of the current frame (null before the first update).</summary>
        public GameTime GameTime { get; private set; }

        /// <summary>The control manager (available after the game was initialized).</summary>
        public ControlManager ControlManager => controlManager;

        public Screen CurrentScreen => screens.Count > 0 ? screens[screens.Count - 1] : null;

        public IReadOnlyList<Screen> Screens => screens;

        public bool IsTransitioning => isNavigating;

        public void ChangeScreen(Screen screen) => ChangeScreen(screen, DefaultTransition);

        public void ChangeScreenWithNoTransition(Screen screen) => ChangeScreen(screen, null);

        public void ChangeScreen(Screen screen, ITransition transition = null)
            => Enqueue(new NavigationRequest(NavigationKind.Change, screen ?? throw new ArgumentNullException(nameof(screen)), transition));

        public void PushScreen(Screen screen, ITransition transition = null)
            => Enqueue(new NavigationRequest(NavigationKind.Push, screen ?? throw new ArgumentNullException(nameof(screen)), transition));

        public void PopScreen(ITransition transition = null)
            => Enqueue(new NavigationRequest(NavigationKind.Pop, null, transition));

        protected override void Initialize()
        {
            controlManager = ServiceProvider.CreateControlManager(GraphicsDevice, ScreenSize);
            controlManager.BaseState = SpriteBatchState.Default
                .WithSamplerState(Settings.PixelArt ? SamplerState.PointClamp : SamplerState.LinearClamp)
                .WithBlendState(Settings.BlendState ?? BlendState.AlphaBlend);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            CreateRenderTarget();
            WindowManager.ScreenSizeChanged += OnScreenSizeChanged;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            WindowManager.UpdateLayout();
            OpenInitialScreen();
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            GameTime = gameTime;
            var clock = ServiceProvider.Clock;
            clock.Update(gameTime);

            if (IsActive || Settings.ProcessInputWhenInactive)
                ServiceProvider.Input.Update(gameTime);

            controlManager?.Update(gameTime);

            var deltaSeconds = clock.DeltaSeconds;
            foreach (var screen in screens.ToArray())
            {
                if (screen.IsDisposed || (screen.IsCovered && !screen.UpdateWhenCovered))
                    continue;
                screen.Scheduler.Update(deltaSeconds);
                screen.Update(gameTime);
            }

            ServiceProvider.Audio.Update(clock.UnscaledDeltaSeconds);
            ServiceProvider.GlobalScheduler.Update(clock.UnscaledDeltaSeconds);
            ProcessPendingNavigations();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (controlManager != null && renderTarget != null)
            {
                controlManager.OnBeforeDraw();

                // Draw the virtual screen in the render target, then draw it scaled in the window.
                GraphicsDevice.SetRenderTarget(renderTarget);
                controlManager.Draw(ScreenBackgroundColor);

                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(WindowBackgroundColor);
                spriteBatch.Begin(samplerState: Settings.PixelArt ? SamplerState.PointClamp : SamplerState.LinearClamp);
                spriteBatch.Draw(renderTarget, WindowManager.GetScreenDrawRectangle(), Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            CleanUp();
            base.UnloadContent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                CleanUp();

            base.Dispose(disposing);

            if (disposing)
                ServiceProvider.Reset(this);
        }

        private void Enqueue(NavigationRequest request)
        {
            pendingNavigations.Enqueue(request);
        }

        private void OpenInitialScreen()
        {
            if (initialScreen == null)
                return;

            var screen = initialScreen;
            initialScreen = null;
            AttachScreen(screen);

            var transition = DefaultTransition;
            if (transition == null)
                return;

            isNavigating = true;
            transition.TransitionIn(() => isNavigating = false);
        }

        private void ProcessPendingNavigations()
        {
            while (!isNavigating && pendingNavigations.Count > 0)
                Execute(pendingNavigations.Dequeue());
        }

        private void Execute(NavigationRequest request)
        {
            if (request.Kind == NavigationKind.Pop && screens.Count <= 1)
                return; // the last screen can only be replaced

            var transition = request.Transition;
            isNavigating = true;

            void Apply()
            {
                try
                {
                    switch (request.Kind)
                    {
                        case NavigationKind.Change:
                            for (var i = screens.Count - 1; i >= 0; i--)
                                CloseScreen(screens[i]);
                            AttachScreen(request.Screen);
                            break;
                        case NavigationKind.Push:
                            AttachScreen(request.Screen);
                            break;
                        case NavigationKind.Pop:
                            CloseScreen(screens[screens.Count - 1]);
                            RefreshScreenStates();
                            break;
                    }
                }
                catch
                {
                    isNavigating = false;
                    throw;
                }

                if (transition != null)
                    transition.TransitionIn(() => isNavigating = false);
                else
                    isNavigating = false;
            }

            if (transition != null)
                transition.TransitionOut(Apply);
            else
                Apply();
        }

        private void AttachScreen(Screen screen)
        {
            if (screen.IsDisposed)
                throw new InvalidOperationException("A closed screen cannot be opened again. Create a new instance.");
            if (screens.Contains(screen))
                throw new InvalidOperationException("The screen is already open.");

            var root = new ScreenRootPanel(ScreenSize.ToVector2());
            var content = new ContentLoader(new ContentManager(Services, Content.RootDirectory), ownsContentManager: true);
            screen.Attach(this, root, content);

            screens.Add(screen);
            controlManager.RootPanel.AddChild(root);
            RefreshScreenStates();

            screen.LoadContent();
            screen.OnInit();
        }

        private void CloseScreen(Screen screen)
        {
            screens.Remove(screen);
            try
            {
                screen.Dispose();
            }
            finally
            {
                screen.Release();
            }
        }

        private void RefreshScreenStates()
        {
            for (var i = 0; i < screens.Count; i++)
            {
                var screen = screens[i];
                var isTop = i == screens.Count - 1;

                if (screen.Root is ScreenRootPanel root)
                {
                    root.UpdateEnabled = isTop || screen.UpdateWhenCovered;
                    root.IsVisible = isTop || screen.DrawWhenCovered;
                    root.BlocksMouseEvents = screen.IsModal;
                    root.SetZIndex(i);
                }

                screen.SetCovered(!isTop);
            }
        }

        private void CreateRenderTarget()
        {
            renderTarget?.Dispose();
            var size = ScreenSize;
            renderTarget = new RenderTarget2D(GraphicsDevice, Math.Max(1, size.X), Math.Max(1, size.Y), false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        private void OnScreenSizeChanged()
        {
            if (GraphicsDevice == null)
                return;
            CreateRenderTarget();
            (WindowManager as GameWindowManager)?.UpdateRootSize();
        }

        private void CleanUp()
        {
            if (isCleanedUp)
                return;
            isCleanedUp = true;

            pendingNavigations.Clear();
            for (var i = screens.Count - 1; i >= 0; i--)
                CloseScreen(screens[i]);

            if (ServiceProvider.TryGet(out IGameWindowManager windowManager))
                windowManager.ScreenSizeChanged -= OnScreenSizeChanged;
            if (ServiceProvider.TryGet(out Audio.IAudioManager audioManager))
                (audioManager as IDisposable)?.Dispose();
            if (ServiceProvider.TryGet(out IContentLoader contentLoader))
                contentLoader.Cache.Clear();

            controlManager?.Dispose();
            controlManager = null;
            renderTarget?.Dispose();
            renderTarget = null;
            spriteBatch?.Dispose();
            spriteBatch = null;

            ShapeExtension.Reset();
            Primitives.Reset();
        }

        private enum NavigationKind
        {
            Change,
            Push,
            Pop
        }

        private readonly struct NavigationRequest
        {
            public NavigationRequest(NavigationKind kind, Screen screen, ITransition transition)
            {
                Kind = kind;
                Screen = screen;
                Transition = transition;
            }

            public NavigationKind Kind { get; }

            public Screen Screen { get; }

            public ITransition Transition { get; }
        }
    }
}
