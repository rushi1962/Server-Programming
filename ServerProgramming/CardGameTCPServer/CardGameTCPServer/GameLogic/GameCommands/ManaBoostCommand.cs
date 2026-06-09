using CardGameTCPServer.TCP;

namespace CardGameTCPServer.GameLogic
{
    public class ManaBoostCommand : IGameCommand
    {
        private ClientConnection client;
        private Game game;

        public ManaBoostCommand(ClientConnection client, Game game)
        {
            this.client = client;
            this.game = game;
        }

        void IGameCommand.Execute()
        {
            game.ManaBostAction(client);
        }
    }
}
