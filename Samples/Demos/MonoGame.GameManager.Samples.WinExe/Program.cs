using MonoGame.GameManager.Samples.Screens;
using System;

namespace MonoGame.GameManager.Samples.WinExe
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new StartupScreen())
                game.Run();
        }
    }
}
