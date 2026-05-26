using System.Collections.Generic;

namespace Server
{
    class Match
    {
        public int MatchId;

        public List<ClientConnection> Players =
            new List<ClientConnection>();

        public int CurrentTurnPlayerIndex = 0;

        public Match(int matchId)
        {
            MatchId = matchId;
        }

        public ClientConnection GetCurrentPlayer()
        {
            return Players[CurrentTurnPlayerIndex];
        }
    }
}
