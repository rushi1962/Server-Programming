using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using CardGameTCPServer.GameLogic;
using CardGameTCPServer.Packets;
using CardGameTCPServer.TCP;
using CardGameTCPServer.Utilities;

class Program
{
    #region collections
    static List<ClientConnection> clients = new List<ClientConnection>();

    static Queue<ClientConnection> matchMakingQueue = new Queue<ClientConnection>();

    static List<Match> matchList = new List<Match>();

    static List<Worker> workers = new List<Worker>();
    #endregion

    #region locks
    static readonly object clientsListLock = new object();
    static readonly object matchMakingQueueLock = new object();
    static readonly object matchListLock = new object();
    #endregion

    static int nextPlayerId = 1;
    static int nextMatchID = 1;
    static int nextWorkerIndex = 0;

    static async Task Main(string[] args)
    {
        CreateWorkers();

        TcpListener server = TCPServer.GetServer();

        while (true)
        {
            Console.WriteLine("Waiting for client...");
            TcpClient tcpClient = await server.AcceptTcpClientAsync();

            ClientConnection client = new ClientConnection(tcpClient, nextPlayerId++);
            lock (clientsListLock)
            {
                clients.Add(client);
            }
            Console.WriteLine($"Client joined the server | ID : {client.ClientID}");

            //Send client ID to client
            client.EnqueueReliableOutgoingPacket(new ClientProfileDataPacket(client.ClientID));

            //Create a separate thread for client
            _ = HandleClient(client);
        }
    }

    static void CreateWorkers()
    {
        int workerCount = Environment.ProcessorCount;

        for (int i = 0; i < workerCount; i++)
        {
            workers.Add(new Worker());
        }
    }

    static Worker FindWorkerForMatch()
    {
        Worker worker = workers[nextWorkerIndex];

        nextWorkerIndex++;
        nextWorkerIndex %= workers.Count;

        return worker;
    }

    static async Task HandleClient(ClientConnection client)
    {
        try
        {
            while (client.ConnectionState == ConnectionState.Connected)
            {
                int packetTypeValue = await PacketReader.ReadInt32Async(client.Stream);
                PacketType packetType = (PacketType)packetTypeValue;

                switch (packetType) 
                {
                    case PacketType.SystemPacket:
                        await ProcessSystemPackets(client);
                        break;

                    case PacketType.MatchMakingLobbyPacket:
                        break;

                    case PacketType.GamePacket:
                        await ProcessGamePackets(client);
                        break;
                }

                client.UpdateHeartbeat();
            }
        }
        catch
        {
            if(client.CurrentMatch != null)
            {
                client.CurrentMatch.GetGame().DeclareGame(client);
                BroadcastGamePacket(GamePacketTypes.GameStateUpdatePacket, client.CurrentMatch);
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

    static async Task ProcessSystemPackets(ClientConnection client)
    {
        int systemPacketTypeValue = await PacketReader.ReadInt32Async(client.Stream);
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
                    client.CurrentMatch.GetGame().DeclareGame(client);
                    BroadcastGamePacket(GamePacketTypes.GameStateUpdatePacket, client.CurrentMatch);
                    CleanupMatch(client.CurrentMatch);
                }
                break;

            case SystemPacketTypes.HeartBeat:
                
                break;
        }
    }

    private static void ProcessMatchMakingPackets(ClientConnection client)
    {
        //ToDo: Needs to be implemented
    }

    static async Task ProcessGamePackets(ClientConnection client)
    {
        int gamePacketTypeValue = await PacketReader.ReadInt32Async(client.Stream);
        GamePacketTypes gamePacketType = (GamePacketTypes)gamePacketTypeValue;

        switch (gamePacketType)
        {
            case GamePacketTypes.GameActionPacket:
                await ProcessGameActionPackets(client);
                break;
        }
    }

    static async Task ProcessGameActionPackets(ClientConnection client)
    {
        int gameActionTypeValue = await PacketReader.ReadInt32Async(client.Stream);
        GameActionTypes gameActionType = (GameActionTypes)gameActionTypeValue;
        Game game = client.CurrentMatch.GetGame();
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
            if (matchMakingQueue.Count < GameConfigs.NUMBER_OF_PLAYERS_IN_A_MATCH)
            {
                return;
            }

            for (int i = 0; i < GameConfigs.NUMBER_OF_PLAYERS_IN_A_MATCH; i++)
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

        Worker worker = FindWorkerForMatch();
        worker.AddMatch(newMatch);
        newMatch.OwnerWorker = worker;

        BroadcastGamePacket(GamePacketTypes.GameStarted, newMatch);
        BroadcastGamePacket(GamePacketTypes.GameStateUpdatePacket, newMatch);
    }

    static void BroadcastGamePacket(GamePacketTypes gamePacketType, Match match, ClientConnection sender = null)
    {
        foreach(ClientConnection client in match.Clients)
        {
            switch (gamePacketType)
            {
                case GamePacketTypes.GameStateUpdatePacket:
                    client.PushLatestGameState(new GameStateUpdatePacket(match.GetGame().GetGameState()));
                    break;

                case GamePacketTypes.GameStarted:
                    client.EnqueueReliableOutgoingPacket(new GameStartedPacket());
                    break;
            }
        }
    }

    static void CleanupMatch(Match match)
    {
        foreach (ClientConnection client in match.Clients)
        {
            client.CurrentMatch = null;
        }

        match.MatchCleanup();
        match.OwnerWorker.RemoveMatch(match);
        match.BroadcastGameUpdate -= BroadcastGamePacket;

        lock (matchListLock)
        {
            matchList.Remove(match);
        }
    }
}
