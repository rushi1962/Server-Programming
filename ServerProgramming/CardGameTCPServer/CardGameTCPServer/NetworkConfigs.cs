using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer
{
    public static class NetworkConfig
    {
        public const int HEARTBEAT_INTERVAL_SECONDS = 5;

        public const int LAGGING_TIMEOUT_SECONDS = 20;

        public const int DISCONNECT_TIMEOUT_SECONDS = 40;
    }
}
