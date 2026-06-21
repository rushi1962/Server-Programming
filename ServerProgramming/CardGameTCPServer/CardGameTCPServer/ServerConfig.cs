using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer
{
    public class ServerConfig
    {
        public int Port { get; set; }
        public int DisconnectTimeoutSeconds { get; set; }
        public int LaggingTimeoutSeconds { get; set; }
        public int ShutdownCountdownSeconds { get; set; }
        public int WorkerCount { get; set; }
        public string LogDirectory { get; set; }
    }
}
