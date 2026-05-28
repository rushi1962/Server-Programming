using System.Net;
using System.Net.Sockets;
using System.Text;
using CardGameTCPServer.TCP;

class Program
{
    static List<ClientConnection> clients =
    new List<ClientConnection>();

    static int nextPlayerId = 1;

    static void Main(string[] args)
    {
        TcpListener server = TCPServer.GetServer();

        while (true)
        {
            Console.WriteLine("Waiting for client...");
            TcpClient tcpClient = server.AcceptTcpClient();

            ClientConnection client = new ClientConnection(tcpClient, nextPlayerId++);
        }
    }
}
