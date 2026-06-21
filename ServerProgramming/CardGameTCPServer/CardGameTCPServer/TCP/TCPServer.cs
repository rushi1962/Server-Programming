using System.Net;
using System.Net.Sockets;

namespace CardGameTCPServer.TCP
{
    class TCPServer
    {
        static TcpListener server;
        static IPAddress iPAddress = IPAddress.Any;

        static public TcpListener GetServer()
        {
            if(server == null)
            {
                server = new TcpListener(iPAddress, ConfigManager.Config.Port);
                server.Start();
            }

            return server;
        }
    }
}
