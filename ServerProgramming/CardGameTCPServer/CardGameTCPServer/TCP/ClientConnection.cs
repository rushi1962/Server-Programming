using System.Net.Sockets;

namespace CardGameTCPServer.TCP
{
    public class ClientConnection
    {
        public int ClientID;

        public TcpClient TcpClient;

        public NetworkStream Stream;

        public BinaryReader Reader;

        public BinaryWriter Writer;

        public Match CurrentMatch;

        public ClientConnection(TcpClient tcpClient, int clientID)
        {
            TcpClient = tcpClient;

            ClientID = clientID;

            Stream = tcpClient.GetStream();

            Reader = new BinaryReader(Stream);

            Writer = new BinaryWriter(Stream);
        }

        public void SetCurrentMatch(Match match)
        {
            CurrentMatch = match;
        }

        public bool GetIsClientConnected()
        {
            return TcpClient.Connected;
        }
    }
}
