namespace CardGameTCPServer.TCP
{
    public class Worker
    {
        private List<Match> matches = new();

        private bool running;
        private Thread workerThread;

        private readonly object matchesLock = new object();

        public Worker()
        {
            running = true;

            workerThread = new Thread(Run);
            workerThread.Start();
        }

        public void AddMatch(Match match)
        {
            lock (matchesLock) 
            {
                matches.Add(match);
            }
        }

        public void RemoveMatch(Match match)
        {
            lock (matchesLock) 
            {
                matches.Remove(match);
            }
        }

        private void Run()
        {
            while (running)
            {
                List<Match> snapshot;

                lock (matchesLock)
                {
                    snapshot = matches.ToList();
                }

                foreach (Match match in snapshot)
                {
                    switch (match.State)
                    {
                        case MatchState.Running:
                            match.Update();
                            break;

                        case MatchState.WaitingForReconnect:
                            match.ProcessReconnectLogic();
                            break;

                        case MatchState.Finished:
                            match.Cleanup();
                            break;
                    }
                }
                Thread.Sleep(50);
            }
        }
    }
}
