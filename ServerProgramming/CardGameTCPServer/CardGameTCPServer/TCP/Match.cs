namespace CardGameTCPServer.TCP
{
    class Match
    {
        public int MatchId;

        public List<ClientConnection> Clients =
            new List<ClientConnection>();

        public Match(int matchId, List<ClientConnection> clients)
        {
            MatchId = matchId;            
            Clients = clients;

            foreach (ClientConnection client in Clients) 
            {
                client.SetCurrentMatch(this);
            }
        }
    }
}
