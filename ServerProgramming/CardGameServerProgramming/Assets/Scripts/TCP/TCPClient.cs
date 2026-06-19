using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TCP 
{
    public class TCPClient
    {
        private string IPAdress = "127.0.0.1";
        private int port = 7777;
        TcpClient client;

        public TCPClient()
        {
            client = new TcpClient();

            Console.WriteLine("Connecting...");

            client.Connect(IPAdress, port);

            Console.WriteLine("Connected!");
        }

        public NetworkStream GetNetworkStream()
        {
            return client != null ? client.GetStream() : null;
        }

        public bool IsConnected()
        {
            return (client != null && client.Connected);
        }

        public void CloseConnection()
        {
            client.Close();
        }

        public async Task ReconnectToServer()
        {
            client?.Close();

            client = new TcpClient();

            await client.ConnectAsync(IPAdress, port);
        }
    }
}


