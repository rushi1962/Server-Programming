using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using CardGameTCPServer.GameLogic;
using CardGameTCPServer.Packets;
using CardGameTCPServer.TCP;

class Program
{
    #region collections
    static List<ClientConnection> clients = new List<ClientConnection>();

    static Queue<ClientConnection> matchMakingQueue = new Queue<ClientConnection>();

    static List<Match> matchList = new List<Match>();

    static Dictionary<int, Game> matchToGameDictinary = new Dictionary<int, Game>();
    #endregion

    #region locks
    static readonly object clientsListLock = new object();
    static readonly object matchMakingQueueLock = new object();
    static readonly object matchListLock = new object();
    static readonly object matchToGameDictinaryLock = new object();
    #endregion

    static int nextPlayerId = 1;
    static int nextMatchID = 1;

    static void Main(string[] args)
    {
        TcpListener server = TCPServer.GetServer();

        while (true)
        {
            Console.WriteLine("Waiting for client...");
            TcpClient tcpClient = server.AcceptTcpClient();

            ClientConnection client = new ClientConnection(tcpClient, nextPlayerId++);
            lock (clientsListLock)
            {
                clients.Add(client);
            }
            Console.WriteLine($"Client joined the server | ID : {client.ClientID}");

            //Send client ID to client
            SendClientProfileData(client);

            //Create a separate thread for client
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private static void HandleClient(ClientConnection client)
    {
        try
        {
            while (true)
            {
                int packetTypeValue = client.Reader.ReadInt32();
                PacketType packetType = (PacketType)packetTypeValue;

                switch (packetType) 
                {
                    case PacketType.SystemPacket:
                        ProcessSystemPackets(client);
                        break;

                    case PacketType.MatchMakingLobbyPacket:
                        break;

                    case PacketType.GamePacket:
                        ProcessGamePackets(client);
                        break;
                }

            }
        }
        catch
        {
            if(client.CurrentMatch != null)
            {
                lock(matchToGameDictinaryLock)
                {
                    matchToGameDictinary[client.CurrentMatch.MatchId].DeclareGame(client);
                }
                CleanupMatch(client.CurrentMatch);
            }

            Console.WriteLine($"Client left the server | ID: {client.ClientID}");
        }

        //Complete the disconnection process if client disconnection is detected
        lock (clientsListLock)
        {
            clients.Remove(client);
        }
        client.TcpClient.Close();
    }

    private static void ProcessSystemPackets(ClientConnection client)
    {
        int systemPacketTypeValue = client.Reader.ReadInt32();
        SystemPacketTypes systemPacketType = (SystemPacketTypes)systemPacketTypeValue;

        switch (systemPacketType) 
        {
            case SystemPacketTypes.MatchMakingRequested:
                lock(matchMakingQueueLock)
                {
                    matchMakingQueue.Enqueue(client);
                }
                TryCreateMatch();
                break;

            case SystemPacketTypes.LeaveGame:
                if (client.CurrentMatch != null)
                {
                    lock (matchToGameDictinaryLock)
                    {
                        matchToGameDictinary[client.CurrentMatch.MatchId].DeclareGame(client);
                        BroadcastGamePacket(GamePacketTypes.GameStateUpdatePacket, client.CurrentMatch);
                    }
                    CleanupMatch(client.CurrentMatch);
                }
                break;
        }
    }

    private static void ProcessMatchMakingPackets(ClientConnection client)
    {
        //ToDo: Needs to be implemented
    }

    private static void ProcessGamePackets(ClientConnection client)
    {
        int gamePacketTypeValue = client.Reader.ReadInt32();
        GamePacketTypes gamePacketType = (GamePacketTypes)gamePacketTypeValue;

        switch (gamePacketType)
        {
            case GamePacketTypes.GameActionPacket:
                ProcessGameActionPackets(client);
                break;
        }
    }

    private static void ProcessGameActionPackets(ClientConnection client)
    {
        int gameActionTypeValue = client.Reader.ReadInt32();
        GameActionTypes gameActionType = (GameActionTypes)gameActionTypeValue;
        Game game = FindGame(client);
        Match clientCurrentMatch = client.CurrentMatch;

        switch (gameActionType)
        {
            case GameActionTypes.GameAction_Attack:
                clientCurrentMatch.EnqueueCommand(new AttackCommand(client, game));
                break;

            case GameActionTypes.GameAction_Heal:
                clientCurrentMatch.EnqueueCommand(new HealCommand(client, game));
                break;

            case GameActionTypes.GameAction_ManaBoost:
                clientCurrentMatch.EnqueueCommand(new ManaBoostCommand(client, game));
                break;
        }
    }

    static void TryCreateMatch()
    {
        List<ClientConnection> clientsToGoInMatch = new List<ClientConnection>();

        lock (matchMakingQueueLock)
        {
            if (matchMakingQueue.Count < GameConfig.NUMBER_OF_PLAYERS_IN_A_MATCH)
            {
                return;
            }

            for (int i = 0; i < GameConfig.NUMBER_OF_PLAYERS_IN_A_MATCH; i++)
            {
                clientsToGoInMatch.Add(matchMakingQueue.Dequeue());
            }
        }

        //Create match
        Match newMatch = new Match(nextMatchID, clientsToGoInMatch);

        newMatch.BroadcastGameUpdate += BroadcastGamePacket;

        lock (matchListLock)
        {
            matchList.Add(newMatch);
        }

        //Create game
        List<int> playerIds = new List<int>();
        foreach (ClientConnection client in clientsToGoInMatch)
        {
            playerIds.Add(client.ClientID);
        }

        lock (matchToGameDictinaryLock)
        {
            matchToGameDictinary.Add(newMatch.MatchId, new Game(playerIds));
        }

        BroadcastGamePacket(GamePacketTypes.GameStarted, newMatch);
        BroadcastGamePacket(GamePacketTypes.GameStateUpdatePacket, newMatch);
    }

    static Game FindGame(ClientConnection client)
    {
        Game game = null;
        lock(matchToGameDictinaryLock)
        {
            game = matchToGameDictinary[client.CurrentMatch.MatchId];
        }
        return game;
    }

    static void SendClientProfileData(ClientConnection client)
    {
        if (!client.GetIsClientConnected()) return;

        //Send client ID
        client.Writer.Write((int)PacketType.SystemPacket);
        client.Writer.Write((int)SystemPacketTypes.ClientUUID);
        client.Writer.Write(client.ClientID);

        //Send client profile name
        string clientName = $"Player{client.ClientID}";

        byte[] data = Encoding.UTF8.GetBytes(clientName);

        client.Writer.Write((int)PacketType.SystemPacket);
        client.Writer.Write((int)SystemPacketTypes.ClientName);
        client.Writer.Write(data.Length);
        client.Writer.Write(data);
    }

    static void BroadcastGamePacket(GamePacketTypes gamePacketType, Match match, ClientConnection sender = null)
    {
        foreach(ClientConnection client in match.Clients)
        {
            switch (gamePacketType)
            {
                case GamePacketTypes.GameStateUpdatePacket:
                    SendGameStateUpdate(client);
                    break;

                case GamePacketTypes.GameStarted:
                    SendGameStartedData(client);
                    break;
            }
        }
    }

    static void SendGameStateUpdate(ClientConnection client)
    {
        if (!client.GetIsClientConnected()) return;

        Game game = null;
        lock (matchToGameDictinaryLock)
        {
            game = matchToGameDictinary[client.CurrentMatch.MatchId];
        }
        var options = new JsonSerializerOptions
        {
            IncludeFields = true
        };

        string gameStateJsonString = JsonSerializer.Serialize(game.GetGameState(), options);

        byte[] data = Encoding.UTF8.GetBytes(gameStateJsonString);

        client.Writer.Write((int)PacketType.GamePacket);
        client.Writer.Write((int)GamePacketTypes.GameStateUpdatePacket);
        client.Writer.Write(data.Length);
        client.Writer.Write(data);
    }

    static void SendGameStartedData(ClientConnection client) 
    {
        if (!client.GetIsClientConnected()) return;

        client.Writer.Write((int)PacketType.GamePacket);
        client.Writer.Write((int)GamePacketTypes.GameStarted);
    }

    static void CleanupMatch(Match match)
    {
        foreach (ClientConnection client in match.Clients)
        {
            client.CurrentMatch = null;
        }

        match.MatchCleanup();

        lock (matchListLock)
        {
            matchList.Remove(match);
        }

        lock (matchToGameDictinaryLock)
        {
            matchToGameDictinary.Remove(match.MatchId);
        }
    }
}
