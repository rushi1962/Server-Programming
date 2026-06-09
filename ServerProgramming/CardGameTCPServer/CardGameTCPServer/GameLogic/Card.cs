using CardGameTCPServer.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer.GameLogic
{
    public class Card
    {
        private GameActionTypes gameActionType;
        private int amount;
        private int cost;

        public Card(GameActionTypes actionType,int cardAmount, int cardCost)
        {
            gameActionType = actionType;
            amount = cardAmount;
            cost = cardCost;
        }

        public GameActionTypes GetActionType() { return gameActionType; }
        public int GetAmount() { return amount; }
        public int GetCost() { return cost; }
    }
}
