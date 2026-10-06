using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Xna.Framework;
using MonoGame.GameManager.Samples.Screens;
using System;

namespace MonoGame.GameManager.Samples.Android
{
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        Icon = "@drawable/icon",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )]
    public class Activity1 : AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Window.Attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;

            var game = new StartupScreen();
            var view = (View)game.Services.GetService(typeof(View));
            var root = new LinearLayout(this)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.MatchParent)
            };
            root.AddView(view);

            SetContentView(root);
            HideSystemBars();
            game.Run();
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);

            if (hasFocus)
                HideSystemBars();
        }

        /// <summary>Full-screen immersive mode: hides the status and navigation bars (they come back with a swipe).</summary>
        private void HideSystemBars()
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(30))
            {
                // Android 15 (API 35) and later draw edge-to-edge by default.
                if (!OperatingSystem.IsAndroidVersionAtLeast(35))
                    Window.SetDecorFitsSystemWindows(false);

                var controller = Window.InsetsController;
                if (controller == null)
                    return;

                controller.Hide(WindowInsets.Type.SystemBars());
                controller.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                return;
            }

            // Android 10 (API 29) has no WindowInsetsController.
#pragma warning disable CA1422
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            Window.DecorView.SystemUiFlags =
                SystemUiFlags.LayoutStable
                | SystemUiFlags.LayoutHideNavigation
                | SystemUiFlags.LayoutFullscreen
                | SystemUiFlags.HideNavigation
                | SystemUiFlags.Fullscreen
                | SystemUiFlags.ImmersiveSticky;
#pragma warning restore CA1422
        }
    }
}
