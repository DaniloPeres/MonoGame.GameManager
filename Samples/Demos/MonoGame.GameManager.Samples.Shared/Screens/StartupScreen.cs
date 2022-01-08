using Microsoft.Xna.Framework;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Services;

namespace MonoGame.GameManager.Samples.Screens
{
    public class StartupScreen : ScreenManager
    {
        public StartupScreen() : base(new Point(1200, 800), new MainScreen())
        { }

        protected override void LoadContent()
        {
            ServiceProvider.MemoryManager.CleanMemoryType = Enums.CleanMemoryType.Manually;

#if WINDOWS_UAP
            _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            () =>
            {
                float DPI = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi;
                Windows.UI.ViewManagement.ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.PreferredLaunchViewSize;
                var desiredSize = new Windows.Foundation.Size(((float)1200 * 96.0f / DPI), ((float)800 * 96.0f / DPI));
                Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize = desiredSize;
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryResizeView(desiredSize);
            });
#elif WINDOWS
            Graphics.PreferredBackBufferWidth = ScreenSize.X;
            Graphics.PreferredBackBufferHeight = ScreenSize.Y;
            Graphics.ApplyChanges();
#endif

            ContentHandler.Instance.LoadAllContents();

            base.LoadContent();
        }
    }
}
