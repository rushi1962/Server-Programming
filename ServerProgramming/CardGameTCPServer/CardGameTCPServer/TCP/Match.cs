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

        private ConcurrentQueue<IGameCommand> gameCommands = new ConcurrentQueue<IGameCommand>();

        private bool running = true;
        private Thread tickThread;
        private bool stateChanged = false;

        public Match(int matchId, List<ClientConnection> clients)
        {
            MatchId = matchId;            
            Clients = clients;

            foreach (ClientConnection client in Clients) 
            {
                client.SetCurrentMatch(this);
            }

            running = true;
            tickThread = new Thread(() => Tick());
            tickThread.Start();
        }

        void Tick() 
        {
            while (running)
            {
                ProcessCommands();
                Thread.Sleep(50);
            }
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
