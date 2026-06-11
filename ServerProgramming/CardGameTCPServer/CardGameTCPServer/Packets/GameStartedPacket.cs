

using System.Net.Sockets;

namespace CardGameTCPServer.Packets
{
    public class GameStartedPacket : IOutgoingPacket
    {
        public async Task WriteAsync(NetworkStream stream)
        {
            await stream.WriteAsync(BitConverter.GetBytes((int)PacketType.GamePacket));
            await stream.WriteAsync(BitConverter.GetBytes((int)GamePacketTypes.GameStarted));
        }
    }
}
