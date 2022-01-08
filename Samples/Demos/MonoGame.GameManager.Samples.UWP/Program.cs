using MonoGame.GameManager.Samples.Screens;
using MonoGame.GameManager.Screens;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace MonoGame.GameManager.Samples.UWP
{
    public static class Program
    {
        static void Main()
        {
            var factory = new Framework.GameFrameworkViewSource<StartupScreen>();
            Windows.ApplicationModel.Core.CoreApplication.Run(factory);
        }
    }
}
