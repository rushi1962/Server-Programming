using System.Net;
using System.Net.Sockets;

namespace CardGameTCPServer.TCP
{
    class TCPServer
    {
        static TcpListener server;
        static IPAddress iPAddress = IPAddress.Any;
        static int port = 7777;

        static public TcpListener GetServer()
        {
            if(server == null)
            {
                server = new TcpListener(iPAddress, port);
                server.Start();
            }

            return server;
        }
    }
}
