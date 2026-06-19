using System.Net.Sockets;

namespace CardGameTCPServer.Packets
{
    public class ReconnectionSuccessPacket : IOutgoingPacket
    {
        public async Task WriteAsync(NetworkStream stream)
        {
            await stream.WriteAsync(BitConverter.GetBytes((int)PacketType.SystemPacket));
            await stream.WriteAsync(BitConverter.GetBytes((int)SystemPacketTypes.ReconnectionSuccess));
        }
    }
}
