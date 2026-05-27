using System.Net.Sockets;

namespace Server
{
    class ClientConnection
    {
        public int PlayerId;

        public TcpClient TcpClient;

        public NetworkStream Stream;

        public BinaryReader Reader;

        public BinaryWriter Writer;

        public string PlayerName;

        public Match CurrentMatch;

        public int Health = 20;

        public int Mana = 20;

        public ClientConnection(TcpClient tcpClient, int playerId)
        {
            TcpClient = tcpClient;

            PlayerId = playerId;

            Stream = tcpClient.GetStream();

            Reader = new BinaryReader(Stream);

            Writer = new BinaryWriter(Stream);

            PlayerName = $"Player{playerId}";
        }
    }
}
