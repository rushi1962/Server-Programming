using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer
{
    public static class Logger
    {
        private static readonly object lockObj = new();

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Warning(string message)
        {
            Write("WARN", message);
        }

        public static void Error(string message)
        {
            Write("ERROR", message);
        }

        private static void Write(string level, string message)
        {
            string logLine =
                $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

            lock (lockObj)
            {
                Console.WriteLine(logLine);

                File.AppendAllText(
                    "server.log",
                    logLine + Environment.NewLine);
            }
        }
    }
}
