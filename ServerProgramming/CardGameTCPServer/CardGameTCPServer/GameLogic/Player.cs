using System;
using CardGameTCPServer.Packets;

namespace CardGameTCPServer.GameLogic
{
    public class Player
    {
        private int PlayerID;

        private int Health;
        private int Mana;

        private List<Card> cards = new List<Card>();
        public Player(int playerID)
        {
            PlayerID = playerID;

            Health = 20;
            Mana = 20;
        }

        public void AddCard(Card card)
        {
            cards.Add(card);
        }

        public int GetPlayerID() { return PlayerID; }

        public int GetHealth() { return Health; }
        public int GetMana() { return Mana; }
        public bool IsDead() { return (Health <= 0); }
        public List<Card> GetCards() { return cards; }

        public void ReceiveAttack(int attackAmount)
        {
            Health -= attackAmount;
            Health = (int)MathF.Max(Health, 0);
        }

        public void ReceiveHealing(int healAmount) 
        { 
            Health += healAmount;
            Health = (int)MathF.Min(Health, 20);
        }

        public void ReceiveManaBoost(int manaBostAmount)
        {
            Mana += manaBostAmount;
            Mana = (int)MathF.Min(Mana, 20);
        }

        public void PayManaCost(int manaAmount)
        {
            Mana -= manaAmount;
            Mana = (int)MathF.Max(Mana, 0);
        }

        public bool HasEnoughMana(GameActionTypes actionType)
        {
            switch (actionType) 
            {
                case GameActionTypes.GameAction_Attack:
                    return Mana >= cards.Find(x => x.GetActionType() == GameActionTypes.GameAction_Attack).GetCost();

                case GameActionTypes.GameAction_Heal:
                    return Mana >= cards.Find(x => x.GetActionType() == GameActionTypes.GameAction_Heal).GetCost();

                case GameActionTypes.GameAction_ManaBoost:
                    return Mana >= cards.Find(x => x.GetActionType() == GameActionTypes.GameAction_ManaBoost).GetCost();

                default:
                    return true;
            }
        }
    }
}
