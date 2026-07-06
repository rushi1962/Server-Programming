using CardGameTCPServer.Data;
using CardGameTCPServer.GameLogic;
using CardGameTCPServer.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer.Services
{
    public class MatchResultService
    {
        private static MatchResultService instance = new MatchResultService();

        public static MatchResultService Instance { get { return instance; } }

        public Dictionary<int, MatchData> matchesResults;
        private readonly object matchesResultsLock;

        private MatchResultService()
        {

        }

        public void Initialize()
        {
            matchesResults = DatabaseService.Instance.LoadMatchesData();
        }

        public void SaveMatchResult(int matchID, GameState gameState)
        {
            if(gameState != null && gameState.IsGameOver)
            {
                lock(matchesResultsLock)
                {
                    MatchData data = new MatchData(matchID, gameState.PlayerState_1.PlayerID, gameState.PlayerState_2.PlayerID,
                    gameState.GameWinnerID, DateTime.UtcNow);

                    DatabaseService.Instance.InsertMatchData(data);
                }                
            }
        }

        public bool GetAccount(int matchID, out MatchData data)
        {
            data = null;

            lock (matchesResultsLock)
            {
                if (matchesResults.ContainsKey(matchID))
                {
                    data = matchesResults[matchID];
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
