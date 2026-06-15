using CardGameTCPServer.Packets;
using CardGameTCPServer.TCP;

namespace CardGameTCPServer.GameLogic
{
    public class LeaveMatchCommand : IGameCommand
    {
        private ClientConnection client;
        private Game game;

        public LeaveMatchCommand(ClientConnection client, Game game)
        {
            this.client = client;
            this.game = game;
        }

        void IGameCommand.Execute()
        {
            game.DeclareGame(client);
            client.EnqueueReliableOutgoingPacket(new GameStateUpdatePacket(game.GetGameState()));
            foreach (var currentDelegate in client.CurrentMatch.BroadcastGameUpdate.GetInvocationList())
            {
                //client.CurrentMatch.BroadcastGameUpdate = Action.RemoveAll(client.CurrentMatch.BroadcastGameUpdate, currentDelegate);
            }
        }
    }
}
