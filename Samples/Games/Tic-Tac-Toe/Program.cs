using System;
using Tic_Tac_Toe.Screens;

namespace Tic_Tac_Toe
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
