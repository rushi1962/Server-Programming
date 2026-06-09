using CardGameTCPServer.TCP;

namespace CardGameTCPServer.GameLogic
{
    public class AttackCommand : IGameCommand
    {
        private ClientConnection client;
        private Game game;

        public AttackCommand(ClientConnection client, Game game)
        {
            this.client = client;
            this.game = game;
        }

        void IGameCommand.Execute()
        {
            game.AttackAction(client);
        }
    }
}
