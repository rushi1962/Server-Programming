using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer.Data
{
    public class PlayerStatsData
    {
        public int PlayerID;
        public int MatchesWon;
        public int MatchesLost;
        public int MatchesTied;

        public PlayerStatsData(int playerID, int matchesWon, int matchesLost, int matchesTied)
        {
            PlayerID = playerID;
            MatchesWon = matchesWon;
            MatchesLost = matchesLost;
            MatchesTied = matchesTied;
        }
    }
}
