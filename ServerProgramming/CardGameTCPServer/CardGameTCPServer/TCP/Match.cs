using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using CardGameTCPServer.GameLogic;
using CardGameTCPServer.Packets;

namespace CardGameTCPServer.TCP
{
    public enum MatchState
    {
        Running,
        WaitingForReconnect,
        Finished
    }

    public class Match
    {
        public int MatchId;

        public List<ClientConnection> Clients =
            new List<ClientConnection>();

        public Action<GamePacketTypes, Match, ClientConnection> BroadcastGameUpdate;
        public Action<Match> MatchCleanup;
        public Action<Match, ClientConnection> DeclareGame;

        public MatchState State { get; private set; }

        private Game game;
        public Worker OwnerWorker { get; set; }

        private ConcurrentQueue<IGameCommand> gameCommands = new ConcurrentQueue<IGameCommand>();

        private bool stateChanged = false;

        private DateTime reconnectStartTime;

        private bool cleanedUp = false;

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

            State = MatchState.Running;
        }

        public Game GetGame()
        {
            return game;
        }

        public void EnterWaitingForReconnect()
        {
            if (State == MatchState.WaitingForReconnect)
                return;

            State = MatchState.WaitingForReconnect;

            reconnectStartTime = DateTime.UtcNow;
        }

        public void Finish()
        {
            if (State == MatchState.Finished)
                return;

            State = MatchState.Finished;            
        }

        public void Update()
        {
            foreach (var client in Clients)
            {
                if (client.ConnectionState == ConnectionState.Disconnected)
                {
                    EnterWaitingForReconnect();
                    return;
                }
            }

            ProcessCommands();
        }

        public void EnqueueCommand(IGameCommand command)
        {
            if (State != MatchState.Running) return;

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

        private bool AreAllPlayersConnected()
        {
            foreach (var client in Clients)
            {
                if (client.ConnectionState != ConnectionState.Connected)
                    return false;
            }

            return true;
        }

        private bool ReconnectTimeoutExpired()
        {
            return (DateTime.UtcNow - reconnectStartTime).TotalSeconds > GameConfigs.RECONNECT_TIMEOUT;
        }

        private void ResumeMatch()
        {
            State = MatchState.Running;
        }

        public void ProcessReconnectLogic()
        {
            if (AreAllPlayersConnected())
            {
                ResumeMatch();
                return;
            }

            if (ReconnectTimeoutExpired())
            {
                DeclareGame?.Invoke(this, Clients.Find(x => x.ConnectionState == ConnectionState.Disconnected));
                Finish();
            }
        }

        public void Cleanup()
        {
            if (cleanedUp) return;

            foreach (ClientConnection client in Clients)
            {
                client.CurrentMatch = null;
            }

            OwnerWorker.RemoveMatch(this);

            MatchCleanup?.Invoke(this);
            cleanedUp = true;
        }
    }
}
