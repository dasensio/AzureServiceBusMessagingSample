using AzureServiceBusMessagingPoc.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AzureServiceBusMessagingPoc
{
    internal static class ConsoleOutputWriter
    {
        static readonly ConcurrentQueue<ConsoleColor> _consoleColorsQueue = new ConcurrentQueue<ConsoleColor>();
        static readonly ConcurrentDictionary<string, ConsoleColor> _consoleColorsDic = new ConcurrentDictionary<string, ConsoleColor>();

        static ConsoleOutputWriter()
        {
            foreach (ConsoleColor value in Enum.GetValues(typeof(ConsoleColor)))
            {
                if (value != ConsoleColor.Black && value != ConsoleColor.White)
                {
                    _consoleColorsQueue.Enqueue(value);
                }
            }
        }

        internal static void WriteLine(string text = null)
        {
            Console.WriteLine(text);
        }

        internal static void WriteLines(IEnumerable<string> texts)
        {
            if (texts?.Any() != true) return;

            foreach (string text in texts)
            {
                WriteLine(text);
            }
        }

        internal static void WriteMessage(ConversationMessage message)
        {
            if (message == null) return;

            ConsoleColor userForegroundColor = GetUserForegroundColor(message);

            Console.ForegroundColor = userForegroundColor;
            Console.Write($"{message.User}");
            Console.ResetColor();
            Console.Write($" at {message.Date:HH:mm}: {message.Text}{Environment.NewLine}");
            Console.ResetColor();
        }

        internal static void WriteError(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine($"Error at {DateTime.Now:HH:mm:ss}");
            WriteLine($"{message}");
            Console.ResetColor();
        }

        private static ConsoleColor GetUserForegroundColor(ConversationMessage message)
        {
            if (message == null) return ConsoleColor.White;

            ConsoleColor userConsoleColor = ConsoleColor.White;

            if (!_consoleColorsDic.TryGetValue(message.User, out ConsoleColor consoleColor))
            {
                if (_consoleColorsQueue.TryDequeue(out consoleColor))
                {
                    _consoleColorsDic.TryAdd(message.User, consoleColor);
                    userConsoleColor = consoleColor;
                }
            }
            else
            {
                userConsoleColor = consoleColor;
            }

            return userConsoleColor;
        }
    }
}