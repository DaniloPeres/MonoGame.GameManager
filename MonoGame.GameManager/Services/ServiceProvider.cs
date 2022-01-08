using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Controls.MouseEvent;
using MonoGame.GameManager.Managers;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Services.Inputs;
using System;

namespace MonoGame.GameManager.Services
{
    public class ServiceProvider
    {
        private static readonly Lazy<ServiceProvider> lazyInstance = new Lazy<ServiceProvider>(() => new ServiceProvider());
        private static ServiceProvider instance => lazyInstance.Value;

        private static ScreenManager screenManager;

        private readonly IHost host;
        private ServiceProvider()
        {
            host = Host.CreateDefaultBuilder()
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.ClearProviders();
                })
                .ConfigureServices((_, services) =>
                {
                    services
                        .AddTransient<MouseInputListener>()
                        .AddTransient<TouchInputListener>()
                        .AddTransient<ControlMouseEventHandler>()
                        .AddTransient<ContentLoaderManager>()
                        .AddSingleton<ControlManager>()
                        .AddSingleton<MemoryManager>()
                        .AddSingleton<ControlCounterService>()
                        .AddSingleton<GameWindowManager>();
                })
                .Build();
        }

        public static ControlManager ControlManager => instance.GetService<ControlManager>();
        public static Panel RootPanel => ControlManager.RootPanel;
        public static ScreenManager ScreenManager => screenManager;
        public static GraphicsDeviceManager GraphicsDeviceManager => ScreenManager.Graphics;
        public static GraphicsDevice GraphicsDevice => ScreenManager.GraphicsDevice;
        public static MemoryManager MemoryManager => instance.GetService<MemoryManager>();
        public static ContentLoaderManager ContentLoaderManager => instance.GetService<ContentLoaderManager>();
        public static ControlCounterService ControlCounterService => instance.GetService<ControlCounterService>();
        public static GameWindowManager GameWindowManager => instance.GetService<GameWindowManager>();
        public static Game Game => ScreenManager;

        public static void SetScreenManager(ScreenManager screenManager)
        {
            ServiceProvider.screenManager = screenManager;
        }

        private T GetService<T>() => host.Services.GetService<T>();
    }
}
