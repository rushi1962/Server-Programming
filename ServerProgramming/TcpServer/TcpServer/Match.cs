using System.Collections.Generic;

namespace Server
{
    class Match
    {
        public int MatchId;

        public List<ClientConnection> Players =
            new List<ClientConnection>();

        public Match(int matchId)
        {
            MatchId = matchId;
        }
    }
}
