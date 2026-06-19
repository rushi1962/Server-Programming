using CardGameTCPServer.TCP;
using CardGameTCPServer.Packets;

namespace CardGameTCPServer.GameLogic
{
    public class Game
    {
        private int CurrentTurnPlayerIndex = 0;
        private bool IsGameOver = false;
        private int GameWinnerID = 0;

        Random random = new Random();

        private List<Player> PlayerList = new List<Player>();

        public Game(List<int> playerIDs)
        {
            foreach(int playerID in playerIDs)
            {
                Player newPlayer = new Player(playerID);

                //Add cards
                newPlayer.AddCard(new Card(GameActionTypes.GameAction_Attack, random.Next(3,8), 5));
                newPlayer.AddCard(new Card(GameActionTypes.GameAction_Heal, random.Next(2, 6), 4));
                newPlayer.AddCard(new Card(GameActionTypes.GameAction_ManaBoost, random.Next(3, 5), 0));

                PlayerList.Add(newPlayer);
            }
        }

        public int GetCurrentPlayerID()
        {
            return PlayerList[CurrentTurnPlayerIndex].GetPlayerID();
        }

        public Player GetCurrentPlayer()
        {
            return PlayerList[CurrentTurnPlayerIndex];
        }

        public bool GetIsGameOver()
        {
            return IsGameOver;
        }

        public void AttackAction(ClientConnection attackingClient)
        {
            Player attackingPlayer = PlayerList.Find(x => x.GetPlayerID() == attackingClient.ClientID);

            if(attackingPlayer != null 
                && IsValidTurn(attackingClient.ClientID) && attackingPlayer.HasEnoughMana(GameActionTypes.GameAction_Attack))
            {
                foreach (Player player in PlayerList) 
                {
                    if (player != null && player != attackingPlayer) 
                    {
                        Card attackCard = attackingPlayer.GetCards().Find(x => x.GetActionType() == GameActionTypes.GameAction_Attack);
                        player.ReceiveAttack(attackCard.GetAmount());
                        attackingPlayer.PayManaCost(attackCard.GetCost());

                        //Check if game is over
                        if (player.IsDead())
                        {
                            IsGameOver = true;
                            GameWinnerID = attackingClient.ClientID;
                            return;
                        }
                    }                    
                }

                AdvanceTurn();
            }
        }

        public void HealAction(ClientConnection healingClient)
        {
            Player healingPlayer = PlayerList.Find(x => x.GetPlayerID() == healingClient.ClientID);

            if (healingPlayer != null && 
                IsValidTurn(healingClient.ClientID) && healingPlayer.HasEnoughMana(GameActionTypes.GameAction_Heal))
            {
                Card healingCard = healingPlayer.GetCards().Find(x => x.GetActionType() == GameActionTypes.GameAction_Heal);
                healingPlayer.ReceiveHealing(healingCard.GetAmount());
                healingPlayer.PayManaCost(healingCard.GetCost());
                AdvanceTurn();
            }
        }

        public void ManaBostAction(ClientConnection manaBoostingClient)
        {
            Player manaBoostPlayer = PlayerList.Find(x => x.GetPlayerID() == manaBoostingClient.ClientID);

            if (manaBoostPlayer != null && 
                IsValidTurn(manaBoostingClient.ClientID) && manaBoostPlayer.HasEnoughMana(GameActionTypes.GameAction_ManaBoost))
            {
                Card manaBoostCard = manaBoostPlayer.GetCards().Find(x => x.GetActionType() == GameActionTypes.GameAction_ManaBoost);
                manaBoostPlayer.ReceiveManaBoost(manaBoostCard.GetAmount());
                manaBoostPlayer.PayManaCost(manaBoostCard.GetCost());
                AdvanceTurn();
            }
        }

        public bool IsValidTurn(int playerID)
        {
            return !IsGameOver && GetCurrentPlayerID() == playerID && !GetCurrentPlayer().IsDead();
        }

        public void AdvanceTurn()
        {
            CurrentTurnPlayerIndex += 1;
            CurrentTurnPlayerIndex %= PlayerList.Count;
        }

        public void DeclareGame(ClientConnection requester)
        {
            if(!IsGameOver)
            {
                IsGameOver = true;
                GameWinnerID = requester.CurrentMatch.Clients.Find(x => x.ClientID != requester.ClientID).ClientID;
            }
        }

        public void DrawGame()
        {
            if (!IsGameOver)
            {
                IsGameOver = true;
            }
        }

        public GameState GetGameState()
        {
            GameState gameState = new GameState();

            gameState.PlayerState_1.PlayerID = PlayerList[0].GetPlayerID();
            gameState.PlayerState_1.PlayerName = $"Player{gameState.PlayerState_1.PlayerID}";
            gameState.PlayerState_1.PlayerHealth = PlayerList[0].GetHealth();
            gameState.PlayerState_1.PlayerMana = PlayerList[0].GetMana();
            gameState.PlayerState_1.PlayerAttackAmount = PlayerList[0].GetCards().
                Find(x => x.GetActionType() == Packets.GameActionTypes.GameAction_Attack).GetAmount();
            gameState.PlayerState_1.PlayerAttackCost = PlayerList[0].GetCards().
                Find(x => x.GetActionType() == Packets.GameActionTypes.GameAction_Attack).GetCost();
            gameState.PlayerState_1.PlayerHealAmount = PlayerList[0].GetCards().
                Find(x => x.GetActionType() == Packets.GameActionTypes.GameAction_Heal).GetAmount();
            gameState.PlayerState_1.PlayerHealCost = PlayerList[0].GetCards().
                Find(x => x.GetActionType() == Packets.GameActionTypes.GameAction_Heal).GetCost();
            gameState.PlayerState_1.PlayerManaBoostAmount = PlayerList[0].GetCards().
                Find(x => x.GetActionType() == Packets.GameActionTypes.GameAction_ManaBoost).GetAmount();
            gameState.PlayerState_1.PlayerManaBoostCost = PlayerList[0].GetCards().
                Find(x => x.GetActionType() == Packets.GameActionTypes.GameAction_ManaBoost).GetCost();

            gameState.PlayerState_2.PlayerID = PlayerList[1].GetPlayerID();
            gameState.PlayerState_2.PlayerName = $"Player{gameState.PlayerState_2.PlayerID}";
            gameState.PlayerState_2.PlayerHealth = PlayerList[1].GetHealth();
            gameState.PlayerState_2.PlayerMana = PlayerList[1].GetMana();
            gameState.PlayerState_2.PlayerAttackAmount = PlayerList[1].GetCards().
                Find(x => x.GetActionType() == Packets.GameActionTypes.GameAction_Attack).GetAmount();
            gameState.PlayerState_2.PlayerAttackCost = PlayerList[1].GetCards().
                Find(x => x.GetActionType() == Packets.GameActionTypes.GameAction_Attack).GetCost();
            gameState.PlayerState_2.PlayerHealAmount = PlayerList[1].GetCards().
                Find(x => x.GetActionType() == Packets.GameActionTypes.GameAction_Heal).GetAmount();
            gameState.PlayerState_2.PlayerHealCost = PlayerList[1].GetCards().
                Find(x => x.GetActionType() == Packets.GameActionTypes.GameAction_Heal).GetCost();
            gameState.PlayerState_2.PlayerManaBoostAmount = PlayerList[1].GetCards().
                Find(x => x.GetActionType() == Packets.GameActionTypes.GameAction_ManaBoost).GetAmount();
            gameState.PlayerState_2.PlayerManaBoostCost = PlayerList[1].GetCards().
                Find(x => x.GetActionType() == Packets.GameActionTypes.GameAction_ManaBoost).GetCost();

            gameState.GameTurnPlayerID = GetCurrentPlayerID();
            gameState.IsGameOver = IsGameOver;
            gameState.GameWinnerID = GameWinnerID;

            return gameState;
        }
    }
}
