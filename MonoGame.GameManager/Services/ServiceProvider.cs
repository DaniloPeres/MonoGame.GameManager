using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Audio;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Core;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Managers;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Services.Inputs;
using MonoGame.GameManager.Storage;
using MonoGame.GameManager.Timers;
using System;
using System.Reflection;

namespace MonoGame.GameManager.Services
{
    /// <summary>
    /// Global access point to the services of the game (Facade over a <see cref="ServiceRegistry"/>).
    /// </summary>
    /// <remarks>
    /// The default services are registered when the <see cref="Screens.ScreenManager"/> is created. A game can
    /// register its own implementation of a service before that (or replace it later) with
    /// <see cref="Register{TService}(TService)"/>, and it can register its own services too.
    /// </remarks>
    public static class ServiceProvider
    {
        private static readonly ServiceRegistry registry = new ServiceRegistry();
        private static readonly Scheduler fallbackGlobalScheduler = new Scheduler();
        private static readonly GameClock fallbackClock = new GameClock();
        private static ScreenManager screenManager;

        /// <summary>The registry of the services.</summary>
        public static ServiceRegistry Registry => registry;

        public static TService Get<TService>() where TService : class => registry.Resolve<TService>();

        public static bool TryGet<TService>(out TService service) where TService : class => registry.TryResolve(out service);

        public static void Register<TService>(TService instance) where TService : class => registry.Register(instance);

        public static void Register<TService>(Func<TService> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton) where TService : class
            => registry.Register(factory, lifetime);

        public static void Replace<TService>(TService instance) where TService : class => registry.Replace(instance);

        public static ScreenManager ScreenManager => screenManager;

        public static IScreenNavigator ScreenNavigator => screenManager;

        public static Game Game => screenManager;

        public static GraphicsDeviceManager GraphicsDeviceManager => screenManager?.Graphics;

        public static GraphicsDevice GraphicsDevice => screenManager?.GraphicsDevice;

        /// <summary>The control manager, available after the game was initialized (null before).</summary>
        public static ControlManager ControlManager => registry.TryResolve(out ControlManager controlManager) ? controlManager : null;

        /// <summary>The root panel of the current screen (or of the stage when no screen is open), or null.</summary>
        public static Panel RootPanel => screenManager?.CurrentScreen?.Root ?? ControlManager?.RootPanel;

        /// <summary>Loads assets that are kept for the whole game.</summary>
        public static IContentLoader ContentLoader => Get<IContentLoader>();

        /// <summary>The cache of the global content loader.</summary>
        public static IAssetCache AssetCache => ContentLoader.Cache;

        public static IGameWindowManager GameWindowManager => Get<IGameWindowManager>();

        public static IInputManager Input => Get<IInputManager>();

        public static IAudioManager Audio => Get<IAudioManager>();

        /// <summary>Saves and loads game data.</summary>
        public static ISaveGameService Storage => Get<ISaveGameService>();

        public static IClock Clock => registry.TryResolve(out IClock clock) ? clock : fallbackClock;

        public static IRandom Random => registry.TryResolve(out IRandom random) ? random : RandomGenerator.Default;

        /// <summary>A scheduler that lives for the whole game (it is not cleared when the screen changes).</summary>
        public static IScheduler GlobalScheduler => registry.TryResolve(out IScheduler scheduler) ? scheduler : fallbackGlobalScheduler;

        /// <summary>The scheduler of the current screen, or the <see cref="GlobalScheduler"/> when no screen is open.</summary>
        public static IScheduler Scheduler => screenManager?.CurrentScreen?.Scheduler ?? GlobalScheduler;

        /// <summary>The time of the current frame (zero before the first update).</summary>
        public static GameTime GameTime => screenManager?.GameTime ?? new GameTime();

        [Obsolete("Use ServiceProvider.ContentLoader instead.")]
        public static IContentLoader ContentLoaderManager => ContentLoader;

        [Obsolete("Use ServiceProvider.AssetCache instead. Assets are released with the ContentManager that loaded them.")]
        public static AssetCache MemoryManager => AssetCache as AssetCache;

        /// <summary>Registers the default services of a new game (the services registered before are kept).</summary>
        internal static void Initialize(ScreenManager manager)
        {
            screenManager = manager ?? throw new ArgumentNullException(nameof(manager));
            registry.Remove<ControlManager>();

            registry.TryRegister<IClock>(() => new GameClock());
            registry.TryRegister<IRandom>(() => new RandomGenerator());
            registry.TryRegister<IScheduler>(() => new Scheduler());
            registry.TryRegister<IGameWindowManager>(() => new GameWindowManager(manager));
            registry.TryRegister<IContentLoader>(() => new ContentLoader(manager.Content));
            registry.TryRegister<IInputManager>(() => new InputManager(manager.Window, GameWindowManager.GetPositionOnScreen));
            registry.TryRegister<IAudioManager>(() => new AudioManager(ContentLoader));
            registry.TryRegister<ISaveGameService>(() => new JsonSaveGameService(
                JsonSaveGameService.DefaultDirectory(manager.Settings.ApplicationName ?? GetDefaultApplicationName())));
        }

        /// <summary>Creates the control manager once the graphics device exists.</summary>
        internal static ControlManager CreateControlManager(GraphicsDevice graphicsDevice, Point screenSize)
        {
            var controlManager = new ControlManager(graphicsDevice, screenSize);
            registry.Register(controlManager);
            var input = Input;
            controlManager.MouseEventHandler.Attach(input.Mouse, input.Touch);
            return controlManager;
        }

        /// <summary>Removes every service of a game that was disposed.</summary>
        internal static void Reset(ScreenManager manager)
        {
            if (!ReferenceEquals(screenManager, manager))
                return;

            screenManager = null;
            registry.Clear();
            fallbackGlobalScheduler.Clear();
        }

        private static string GetDefaultApplicationName()
        {
            var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            return string.IsNullOrEmpty(assemblyName) ? "MonoGameGame" : assemblyName;
        }
    }
}
