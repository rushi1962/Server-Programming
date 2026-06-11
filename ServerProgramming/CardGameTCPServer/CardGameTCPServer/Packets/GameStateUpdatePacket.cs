using CardGameTCPServer.GameLogic;
using System.Net.Sockets;
using System.Text.Json;
using System.Text;

namespace CardGameTCPServer.Packets
{
    public class GameStateUpdatePacket : IOutgoingPacket
    {
        public GameState State { get; }

        public GameStateUpdatePacket(GameState state)
        {
            State = state;
        }

        public async Task WriteAsync(NetworkStream stream)
        {
            var options = new JsonSerializerOptions
            {
                IncludeFields = true
            };

            string gameStateJsonString = JsonSerializer.Serialize(State, options);

            byte[] data = Encoding.UTF8.GetBytes(gameStateJsonString);

            await stream.WriteAsync(BitConverter.GetBytes((int)PacketType.GamePacket));
            await stream.WriteAsync(BitConverter.GetBytes((int)GamePacketTypes.GameStateUpdatePacket));
            await stream.WriteAsync(BitConverter.GetBytes(data.Length));
            await stream.WriteAsync(data);
        }
    }
}
