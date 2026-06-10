using System.Collections.Concurrent;
using CardGameTCPServer.GameLogic;
using CardGameTCPServer.Packets;

namespace CardGameTCPServer.TCP
{
    public class Match
    {
        public int MatchId;

        public List<ClientConnection> Clients =
            new List<ClientConnection>();

        public Action<GamePacketTypes, Match, ClientConnection> BroadcastGameUpdate;

        private Game game;
        public Worker OwnerWorker { get; set; }

        private ConcurrentQueue<IGameCommand> gameCommands = new ConcurrentQueue<IGameCommand>();

        private bool running = true;
        private bool stateChanged = false;

        public Match(int matchId, List<ClientConnection> clients)
        {
            MatchId = matchId;            
            Clients = clients;

            foreach (ClientConnection client in Clients) 
            {
                client.SetCurrentMatch(this);
            }

            //Create game
            List<int> playerIds = new List<int>();
            foreach (ClientConnection client in clients)
            {
                playerIds.Add(client.ClientID);
            }
            game = new Game(playerIds);

            running = true;
        }

        public Game GetGame()
        {
            return game;
        }

        public void EnqueueCommand(IGameCommand command)
        {
            if (!running) return;

            gameCommands.Enqueue(command);
        }

        public void ProcessCommands()
        {
            while (gameCommands.TryDequeue(out IGameCommand command))
            {
                command.Execute();
                stateChanged = true;
            }

            if(stateChanged)
            {
                BroadcastGameUpdate?.Invoke(GamePacketTypes.GameStateUpdatePacket, this, null);
                stateChanged = false;
            }
        }

        public void MatchCleanup()
        {
            running = false;
        }
    }
}
