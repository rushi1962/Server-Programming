using System.Collections.Concurrent;
using System.Net.Sockets;
using CardGameTCPServer.Packets;

namespace CardGameTCPServer.TCP
{
    public enum ConnectionState
    {
        Connected = 1,
        Lagging = 2,
        Disconnected =3
    }

    public class ClientConnection
    {
        //TCP
        public int ClientID;
        public TcpClient TcpClient;
        public NetworkStream Stream;
        public BinaryReader Reader;
        public volatile ConnectionState ConnectionState;

        //Match
        public Match CurrentMatch;

        //Packets
        ConcurrentQueue<IOutgoingPacket> reliablePackets = new ConcurrentQueue<IOutgoingPacket>();
        GameStateUpdatePacket latestStatePacket = null;
        GameStateUpdatePacket StatePacketToSend = null;
        private readonly object statePacketLock = new();

        //Heartbeats
        public DateTime LastRecievedPacketTime { get; private set; }

        public ClientConnection(TcpClient tcpClient, int clientID)
        {
            TcpClient = tcpClient;

            ClientID = clientID;

            Stream = tcpClient.GetStream();

            Reader = new BinaryReader(Stream);

            ConnectionState = ConnectionState.Connected;

            LastRecievedPacketTime = DateTime.UtcNow;

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

        public void UpdateHeartbeat()
        {
            LastRecievedPacketTime = DateTime.UtcNow;
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
            while (ConnectionState == ConnectionState.Connected)
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
                        break;
                    }
                }

                await Task.Delay(1);
            }
        }
    }
}
