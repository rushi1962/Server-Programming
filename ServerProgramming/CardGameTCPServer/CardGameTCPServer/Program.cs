using System.Net.Sockets;
using System.Text;
using CardGameTCPServer;
using CardGameTCPServer.GameLogic;
using CardGameTCPServer.Packets;
using CardGameTCPServer.Services;
using CardGameTCPServer.TCP;
using CardGameTCPServer.Utilities;

public enum ServerState
{
    Running = 1,
    ShuttingDown = 2,
    Shutdown = 3
}

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

    static ServerState currentServerState = ServerState.Running;

    static DateTime shutdownTime;

    static async Task Main(string[] args)
    {
        ConfigManager.Load();

        CreateWorkers();

        _ = Task.Run(ConsoleCommandService.Run);

        TcpListener server = TCPServer.GetServer();

        while (true)
        {
            Logger.Info("Waiting for client...");
            TcpClient tcpClient = await server.AcceptTcpClientAsync();

            ClientConnection client = new ClientConnection(tcpClient, nextPlayerId++);

            if (currentServerState != ServerState.Running)
            {
                client.EnqueueReliableOutgoingPacket(new ServerShutdownCountdownPackage
                    ((int)(shutdownTime - DateTime.UtcNow).TotalSeconds));
                client.TcpClient.Close();
                continue;
            }

            lock (clientsListLock)
            {
                clients.Add(client);
            }
            Logger.Info($"Client joined the server | ID : {client.ClientID}");

            //Send client ID to client
            client.EnqueueReliableOutgoingPacket(new ClientProfileDataPacket(client.ClientID, client.ReconnectToken));

            //Create a separate thread for client
            _ = HandleClient(client);
        }
    }

    static void CreateWorkers()
    {
        int workerCount = ConfigManager.Config.WorkerCount > 0 ? ConfigManager.Config.WorkerCount : Environment.ProcessorCount;

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
            while (!client.ConnectionTransferred)
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
            client.ConnectionState = ConnectionState.Disconnected;

            Logger.Warning($"Client left the server | ID: {client.ClientID}");
        }

        if (client.ConnectionTransferred) return;

        //Complete the disconnection process if client disconnection is detected
        if(client.CurrentMatch == null)
        {
            lock (clientsListLock)
            {
                clients.Remove(client);
            }
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
                if (currentServerState != ServerState.Running)
                {
                    return;
                }

                lock (matchMakingQueueLock)
                {
                    matchMakingQueue.Enqueue(client);
                }
                TryCreateMatch();
                break;

            case SystemPacketTypes.LeaveMatch:
                
                break;

            case SystemPacketTypes.HeartBeat:
                break;

            case SystemPacketTypes.ReconnectionToken:
                await HandleReconnectionPacket(client);
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

    static async Task HandleReconnectionPacket(ClientConnection client)
    {
        int messageLength = await PacketReader.ReadInt32Async(client.Stream);
        byte[] tokenData = await PacketReader.ReadBytesAsync(client.Stream, messageLength);
        string reconnectionToken = Encoding.UTF8.GetString(tokenData);
        ClientConnection matchedClient = null;

        lock (clientsListLock)
        {
            foreach (ClientConnection oldClient in clients)
            {
                if (oldClient != null &&
                    oldClient.ConnectionState == ConnectionState.Disconnected && oldClient.ReconnectToken == reconnectionToken)
                {

                    matchedClient = oldClient;
                    break;
                }
            }
        }

        if (matchedClient != null) 
        {

            client.ConnectionTransferred = true;

            lock (clientsListLock)
            {
                clients.Remove(client);
            }

            matchedClient.Reconnect(client.TcpClient);
            _ = HandleClient(matchedClient);

            client.EnqueueReliableOutgoingPacket(new ReconnectionSuccessPacket());
            if (matchedClient.CurrentMatch != null)
            {
                BroadcastGamePacket(GamePacketTypes.GameStateUpdatePacket, matchedClient.CurrentMatch);
            }
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
        newMatch.MatchCleanup += CleanupMatch;
        newMatch.DeclareGame += DeclareGame;

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

    static void DeclareGame(Match match, ClientConnection requestingClient)
    {
        match.GetGame().DeclareGame(requestingClient);
        BroadcastGamePacket(GamePacketTypes.GameStateUpdatePacket, match);
        match.DeclareGame -= DeclareGame;
    }

    static void CleanupMatch(Match match)
    {
        foreach (ClientConnection client in match.Clients)
        {
            if (client != null && client.ConnectionState == ConnectionState.Disconnected) 
            {
                lock (clientsListLock)
                {
                    clients.Remove(client);
                }                    
            }
        }

        lock (matchListLock)
        {
            matchList.Remove(match);
        }

        match.BroadcastGameUpdate -= BroadcastGamePacket;
        match.MatchCleanup -= CleanupMatch;
    }

    static async Task ServerConsoleLoop()
    {
        while (true)
        {
            string command = Console.ReadLine();

            if (command == null)
                continue;

            if (command.StartsWith("shutdown"))
            {
                BeginShutdown(60); // 60 second countdown
            }
        }
    }

    internal static void ShowHelp()
    {
        throw new NotImplementedException();
    }

    internal static void ShowStatus()
    {
        lock (clientsListLock)
        {
            //Write clients count
            Logger.Info($"Total number of clients on server: {clients.Count}");
        }

        lock (matchMakingQueueLock)
        {
            //Write matchmaking queue count
            Logger.Info($"Total number of clients in matchmaking queue: {matchMakingQueue.Count}");
        }

        lock (matchListLock)
        {
            //Write matches count
            Logger.Info($"Total number of ongoing matches: {matchList.Count}");
        }

        //Write worker thread count
        Logger.Info($"Total number of worker threads: {workers.Count}");
    }

    public static void BeginShutdown(int seconds)
    {
        if (currentServerState != ServerState.Running)
            return;

        currentServerState = ServerState.ShuttingDown;

        shutdownTime = DateTime.UtcNow.AddSeconds(seconds);

        Logger.Warning($"Server shutdown in {seconds} seconds");

        BroadcastShutdownPacket(seconds);

        _ = Task.Run(() => ShutdownCountdown(seconds));
    }

    static async Task ShutdownCountdown(int seconds)
    {
        while (seconds > 0)
        {
            await Task.Delay(1000);

            seconds--;

            if (seconds == 30 ||
               seconds == 10 ||
               seconds <= 5)
            {
                BroadcastShutdownPacket(seconds);
            }
        }

        PerformShutdown();
    }

    private static void PerformShutdown()
    {
        lock (matchListLock)
        {
            foreach (Match match in matchList)
            {
                match.GetGame().DrawGame();
                BroadcastGamePacket(GamePacketTypes.GameStateUpdatePacket, match);
            }

            matchList.Clear();
        }

        lock (clients)
        {
            foreach(ClientConnection client in clients)
            {
                client.EnqueueReliableOutgoingPacket(new ServerShutdownPackage());
                client.TcpClient.Close();
            }

            clients.Clear();
        }

        TCPServer.GetServer().Stop();
        Environment.Exit(0);
    }

    private static void BroadcastShutdownPacket(int seconds)
    {
        lock (clientsListLock)
        {
            foreach (var client in clients) 
            {
                client.EnqueueReliableOutgoingPacket(new ServerShutdownCountdownPackage(seconds));
            }
        }
    }
}
