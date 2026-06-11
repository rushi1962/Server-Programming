using System.Collections.Concurrent;
using System.Net.Sockets;
using CardGameTCPServer.Packets;

namespace CardGameTCPServer.TCP
{
    public class ClientConnection
    {
        public int ClientID;

        public TcpClient TcpClient;

        public NetworkStream Stream;

        public BinaryReader Reader;

        public Match CurrentMatch;

        public bool IsConnected;

        ConcurrentQueue<IOutgoingPacket> outgoingPackets = new ConcurrentQueue<IOutgoingPacket>();

        public ClientConnection(TcpClient tcpClient, int clientID)
        {
            TcpClient = tcpClient;

            ClientID = clientID;

            Stream = tcpClient.GetStream();

            Reader = new BinaryReader(Stream);

            IsConnected = true;

            _ = SendLoop();
        }

        public void SetCurrentMatch(Match match)
        {
            CurrentMatch = match;
        }

        public bool GetIsClientConnected()
        {
            return TcpClient.Connected;
        }

        public void EnqueueOutgoingPacket(IOutgoingPacket packet)
        {
            outgoingPackets.Enqueue(packet);
        }

        public bool TryDequeueOutgoingPacket(out IOutgoingPacket packet)
        {
            return outgoingPackets.TryDequeue(out packet);
        }

        async Task SendLoop()
        {
            while (IsConnected)
            {
                if (outgoingPackets.TryDequeue(out IOutgoingPacket packet))
                {
                    try
                    {
                        await packet.WriteAsync(Stream);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        break;
                    }
                }

                await Task.Delay(1);
            }
        }
    }
}
