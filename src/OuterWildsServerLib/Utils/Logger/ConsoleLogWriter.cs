using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Utils.Logger
{
    public class ConsoleLogWriter : ILogWriter
    {
        public bool ColorConsole { get; set; } = true;

        public void Write(string formattedMessage, LogLevel logLevel)
        {
            if (ColorConsole)
            {
                var colorToReset = Console.ForegroundColor;
                switch (logLevel)
                {
                    case LogLevel.TRACE:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case LogLevel.DEBUG:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogLevel.INFO:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case LogLevel.WARN:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.ERROR:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.FATAL:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                }
                Console.ForegroundColor = colorToReset;
            }
            Console.WriteLine(formattedMessage);
        }
    }
}
