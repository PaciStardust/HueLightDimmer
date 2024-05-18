using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueLightDimmer
{
    //Simple logging class to avoid using a logger since it adds unneeded complexity
    internal static class Logger
    {
        internal static void Log(LogLevel level, string message)
        {
            var curColor = Console.ForegroundColor;
            Console.ForegroundColor = GetLogColor(level);
            Console.WriteLine($"[{level.ToString()[0]}][{DateTime.Now}] {message}");
            Console.ForegroundColor = curColor;
        }

        internal static void Debug(string message)
            => Log(LogLevel.Debug, message);
        internal static void Info(string message)
            => Log(LogLevel.Info, message);
        internal static void Error(string message)
            => Log(LogLevel.Error, message);
        internal static void Error(Exception ex, string message)
            => Error($"({ex.GetType().Name}) {message}: {ex.Message}");

        private static ConsoleColor GetLogColor(LogLevel logLevel)
            => logLevel switch
            {
                LogLevel.Debug => ConsoleColor.Gray,
                LogLevel.Info => ConsoleColor.Blue,
                LogLevel.Error => ConsoleColor.Red,
                _ => ConsoleColor.White
            };
    }

    internal enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Error = 2
    }
}
