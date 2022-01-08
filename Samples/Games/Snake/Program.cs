using Snake.Screens;
using System;

namespace Snake
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
