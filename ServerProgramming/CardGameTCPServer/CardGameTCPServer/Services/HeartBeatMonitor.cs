using CardGameTCPServer.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer.Services
{
    public class HeartbeatMonitor
    {
        private bool running;
        private Thread monitorThread;
        private List<ClientConnection> clients;
        private readonly Object clientsListLock;

        public HeartbeatMonitor(List<ClientConnection> clients, Object clientsListLock)
        {
            running = true;
            this.clients = clients;
            this.clientsListLock = clientsListLock;

            monitorThread = new Thread(Run);
            monitorThread.Start();
        }

        private void Run()
        {
            while (running)
            {
                CheckClients();

                Thread.Sleep(5000);
            }
        }

        private void CheckClients()
        {
            lock (clientsListLock)
            {
                foreach (ClientConnection client in clients)
                {
                    int elapsedSeconds = (int)(DateTime.UtcNow - client.LastRecievedPacketTime).TotalSeconds;

                    if (elapsedSeconds >= NetworkConfig.DISCONNECT_TIMEOUT_SECONDS)
                    {
                        client.ConnectionState = ConnectionState.Disconnected;
                    }
                    else if (elapsedSeconds >= NetworkConfig.LAGGING_TIMEOUT_SECONDS)
                    {
                        client.ConnectionState = ConnectionState.Lagging;
                    }
                    else
                    {
                        client.ConnectionState = ConnectionState.Connected;
                    }
                }
            }
        }
    }
}
