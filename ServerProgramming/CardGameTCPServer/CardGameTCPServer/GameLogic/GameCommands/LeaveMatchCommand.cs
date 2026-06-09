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
            //Broadcast game state and clean up match
        }
    }
}
