using System;

namespace CapitalGainsCalculator
{
    public static class Debug
    {
        public static void Log(string text, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;

            Console.WriteLine(text);

            Console.ResetColor();
        }
    }
}