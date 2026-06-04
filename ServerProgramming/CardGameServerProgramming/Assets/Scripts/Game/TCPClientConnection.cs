using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;
using UnityEngine;
using TCP;
using Packets;
using GameLogic;
using Newtonsoft.Json;
using System.Collections.Concurrent;

public class TCPClientConnection : MonoBehaviour
{

    public static TCPClientConnection Instance;

    #region Events
    public Action Event_ConnectedToServer;

    public Action<int> Event_ReceivedClientID;
    public Action<String> Event_ReceivedClientName;

    public Action Event_GameStarted;
    public Action<GameState> Event_GameStateUpdated;
    public Action Event_Disconnected;

    #endregion

    #region ConcurrentQueues

    ConcurrentQueue<int> pendingClientIDs = new ConcurrentQueue<int>();
    ConcurrentQueue<string> pendingClientNames = new ConcurrentQueue<string>();

    ConcurrentQueue<int> pendingGameStartedEvents = new ConcurrentQueue<int>();

    ConcurrentQueue<GameState> pendingStates = new ConcurrentQueue<GameState>();
    ConcurrentQueue<int> pendingGameDisconnectedEvents = new ConcurrentQueue<int>();

    #endregion

    private TCPClient m_TCPClient;

    private Thread receiveThread;

    private BinaryWriter writer;

    private readonly object writerLock = new object();

    void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);        
    }

    public void ConnectToServer()
    {
        m_TCPClient = new TCPClient();

        if (m_TCPClient.IsConnected())
        {
            NetworkStream networkStream = m_TCPClient.GetNetworkStream();

            if (networkStream != null)
            {
                receiveThread = new Thread(() => ReceiveMessages(networkStream));
                receiveThread.IsBackground = true;
                receiveThread.Start();

                writer = new BinaryWriter(networkStream);
            }

            Event_ConnectedToServer?.Invoke();
        }
    }

    public bool IsConnected()
    {
        return m_TCPClient != null &&
               m_TCPClient.IsConnected();
    }

    private void Update()
    {
        while (pendingClientIDs.TryDequeue(out int clientID))
        {
            Event_ReceivedClientID?.Invoke(clientID);
        }

        while (pendingClientNames.TryDequeue(out string clientName))
        {
            Event_ReceivedClientName?.Invoke(clientName);
        }

        while (pendingGameStartedEvents.TryDequeue(out int gameStarted))
        {
            Event_GameStarted?.Invoke();
        }

        while (pendingStates.TryDequeue(out GameState state))
        {
            Event_GameStateUpdated?.Invoke(state);
        }

        while (pendingGameDisconnectedEvents.TryDequeue(out int gameDisconnected))
        {
            Event_Disconnected?.Invoke();
        }
    }

    void ReceiveMessages(NetworkStream stream)
    {
        BinaryReader reader = new BinaryReader(stream);

        try
        {
            while (true)
            {
                int packetTypeValue = reader.ReadInt32();

                PacketType packetType =
                    (PacketType)packetTypeValue;

                switch (packetType)
                {
                    case PacketType.SystemPacket:
                        HandleSystemPacket(reader);
                        break;

                    case PacketType.MatchMakingLobbyPacket:
                        break;

                    case PacketType.GamePacket:
                        HandleGamePacket(reader);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            pendingGameDisconnectedEvents.Enqueue(0);
        }
    }

    void HandleSystemPacket(BinaryReader reader)
    {
        int packetTypeValue = reader.ReadInt32();

        SystemPacketTypes systemPacketType =
            (SystemPacketTypes)packetTypeValue;

        switch (systemPacketType)
        {
            case SystemPacketTypes.ClientUUID:
                int clientId = reader.ReadInt32();
                pendingClientIDs.Enqueue(clientId);
                break;

            case SystemPacketTypes.ClientName:
                int messageLength = reader.ReadInt32();
                byte[] data = reader.ReadBytes(messageLength);
                string clientName = Encoding.UTF8.GetString(data);

                pendingClientNames.Enqueue(clientName);
                break;
        }
    }

    void HandleGamePacket(BinaryReader reader)
    {
        int packetTypeValue = reader.ReadInt32();

        GamePacketTypes packetType = (GamePacketTypes)packetTypeValue;

        switch (packetType) 
        {
            case GamePacketTypes.GameStarted:
                pendingGameStartedEvents.Enqueue(0);
                break;

            case GamePacketTypes.GameStateUpdatePacket:
                int messageLength = reader.ReadInt32();
                byte[] data = reader.ReadBytes(messageLength);
                string gameStateJsonString = Encoding.UTF8.GetString(data);

                GameState gameState = JsonConvert.DeserializeObject<GameState>(gameStateJsonString);
                pendingStates.Enqueue(gameState);

                break;
        }
    }

    public void SendSystemPacket(SystemPacketTypes systemPacketType)
    {
        lock(writerLock)
        {
            writer.Write((int)PacketType.SystemPacket);
            writer.Write((int)systemPacketType);
        }
    }

    public void SendGameActionPacket(GameActionTypes actionType)
    {
        lock (writerLock)
        {
            writer.Write((int)PacketType.GamePacket);
            writer.Write((int)GamePacketTypes.GameActionPacket);
            writer.Write((int)actionType);
        }
    }

    private void OnDestroy()
    {
        m_TCPClient.CloseConnection();
    }

}
