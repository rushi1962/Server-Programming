using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer.GameLogic
{
    public class GameState
    {
        public PlayerState PlayerState_1;
        public PlayerState PlayerState_2;

        public int GameTurnPlayerID = 0;
        public bool IsGameOver;
        public int GameWinnerID = 0;

        public GameState()
        {
            GameTurnPlayerID = 0;
            IsGameOver = false;
            GameWinnerID = 0;
        }
    }

    public class PlayerState
    {
        public int PlayerID;
        public string PlayerName = "";
        public int PlayerHealth;
        public int PlayerMana;
        public int PlayerAttackAmount;
        public int PlayerAttackCost;
        public int PlayerHealAmount;
        public int PlayerHealCost;
        public int PlayerManaBoostAmount;
        public int PlayerManaBoostCost;
    }
}
