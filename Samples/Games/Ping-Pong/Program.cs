using Ping_Pong.Screens;
using System;

namespace Ping_Pong
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
