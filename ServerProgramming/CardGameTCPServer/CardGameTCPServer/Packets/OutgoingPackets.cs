using System.Net.Sockets;

namespace CardGameTCPServer.Packets
{
    public interface IOutgoingPacket
    {
        Task WriteAsync(NetworkStream stream);
    }
}
