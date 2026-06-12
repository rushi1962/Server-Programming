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

        ConcurrentQueue<IOutgoingPacket> reliablePackets = new ConcurrentQueue<IOutgoingPacket>();

        GameStateUpdatePacket latestStatePacket = null;
        GameStateUpdatePacket StatePacketToSend = null;

        private readonly object statePacketLock = new();

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

        public void EnqueueReliableOutgoingPacket(IOutgoingPacket packet)
        {
            reliablePackets.Enqueue(packet);
        }

        public bool TryDequeueReliableOutgoingPacket(out IOutgoingPacket packet)
        {
            return reliablePackets.TryDequeue(out packet);
        }

        public void PushLatestGameState(GameStateUpdatePacket latestStatePacket)
        {
            lock(statePacketLock)
            {
                this.latestStatePacket = latestStatePacket;
            }
        }

        async Task SendLoop()
        {
            while (IsConnected)
            {
                if (reliablePackets.TryDequeue(out IOutgoingPacket packet))
                {
                    try
                    {
                        await packet.WriteAsync(Stream);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        IsConnected=false;
                        break;
                    }
                }

                lock (statePacketLock) 
                {
                    StatePacketToSend = latestStatePacket;
                    latestStatePacket = null;
                }                    

                if(StatePacketToSend != null)
                {
                    try
                    {
                        await StatePacketToSend.WriteAsync(Stream);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        IsConnected = false;
                        break;
                    }
                }

                await Task.Delay(1);
            }
        }
    }
}
