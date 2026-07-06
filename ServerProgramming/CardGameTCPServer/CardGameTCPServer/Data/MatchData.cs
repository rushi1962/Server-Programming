using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer.Data
{
    public class MatchData
    {
        public int MatchID;
        public int Player_1_ID;
        public int Player_2_ID;
        public int WinnerID;
        public DateTime FinishedAt;

        public MatchData(int matchID, int player_1_ID, int player_2_ID, int winnerID, DateTime finishedAt) 
        {
            MatchID = matchID;
            Player_1_ID = player_1_ID;
            Player_2_ID = player_2_ID;
            WinnerID = winnerID;
            FinishedAt = finishedAt;
        }
    }
}
