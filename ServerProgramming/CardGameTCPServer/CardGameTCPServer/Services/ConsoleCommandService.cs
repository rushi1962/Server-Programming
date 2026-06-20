using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer.Services
{
    public class ConsoleCommandService
    {
        public static void Run()
        {
            while (true)
            {
                string command = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(command))
                    continue;

                ProcessCommand(command.Trim());
            }
        }

        private static void ProcessCommand(string command)
        {
            switch (command.ToLower())
            {
                case "help":
                    Program.ShowHelp();
                    break;

                case "status":
                    Program.ShowStatus();
                    break;

                case "shutdown":
                    string[] parts = command.Split(' ');

                    if (parts.Length == 2 &&
                       int.TryParse(parts[1], out int seconds))
                    {
                        Program.BeginShutdown(seconds);
                    }
                    break;
            }
        }
    }
}
