using CardGameTCPServer.TCP;

namespace CardGameTCPServer.GameLogic
{
    public class HealCommand : IGameCommand
    {
        private ClientConnection client;
        private Game game;

        public HealCommand(ClientConnection client, Game game)
        {
            this.client = client;
            this.game = game;
        }

        void IGameCommand.Execute()
        {
            game.HealAction(client);
        }
    }
}
