using CardGameTCPServer.Data;
using CardGameTCPServer.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer.Services
{
    public class PlayerStatsService
    {
        private static PlayerStatsService instance = new PlayerStatsService();

        public static PlayerStatsService Instance { get { return instance; } }

        public Dictionary<int, PlayerStatsData> playerStats;
        private readonly object playerStatsLock;

        private PlayerStatsService()
        {

        }

        public void Initialize()
        {
            playerStats = DatabaseService.Instance.LoadPlayerStatsData();
        }

        public void SavePlayerWin(int playerID)
        {
            lock (playerStatsLock)
            {
                PlayerStatsData playerStatsData;
                if (playerStats.ContainsKey(playerID))
                {
                    playerStatsData = playerStats[playerID];
                }
                else 
                {
                    playerStatsData = new PlayerStatsData(playerID, 0, 0, 0);
                }

                playerStatsData.MatchesWon++;
                DatabaseService.Instance.UpdatePlayerStatsData(playerStatsData);
            }
        }

        public void SavePlayerLose(int playerID)
        {
            lock (playerStatsLock)
            {
                PlayerStatsData playerStatsData;
                if (playerStats.ContainsKey(playerID))
                {
                    playerStatsData = playerStats[playerID];
                }
                else
                {
                    playerStatsData = new PlayerStatsData(playerID, 0, 0, 0);
                }

                playerStatsData.MatchesLost++;
                DatabaseService.Instance.UpdatePlayerStatsData(playerStatsData);
            }
        }

        public void SavePlayerTie(int playerID)
        {
            lock (playerStatsLock)
            {
                PlayerStatsData playerStatsData;
                if (playerStats.ContainsKey(playerID))
                {
                    playerStatsData = playerStats[playerID];
                }
                else
                {
                    playerStatsData = new PlayerStatsData(playerID, 0, 0, 0);
                }

                playerStatsData.MatchesTied++;
                DatabaseService.Instance.UpdatePlayerStatsData(playerStatsData);
            }
        }

        public bool GetPlayerStats(int playerID, out PlayerStatsData data)
        {
            data = null;

            lock (playerStatsLock)
            {
                if (playerStats.ContainsKey(playerID))
                {
                    data = playerStats[playerID];
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
