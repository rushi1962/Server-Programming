using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer.Packets
{
    public class ServerShutdownCountdownPackage : IOutgoingPacket
    {
        int RemainingSeconds;

        public ServerShutdownCountdownPackage(int remainingSeconds) 
        {
            RemainingSeconds = remainingSeconds;
        }

        public async Task WriteAsync(NetworkStream stream)
        {
            await stream.WriteAsync(BitConverter.GetBytes((int)PacketType.SystemPacket));
            await stream.WriteAsync(BitConverter.GetBytes((int)SystemPacketTypes.ServerShutdownCountdownStarted));
            await stream.WriteAsync(BitConverter.GetBytes(RemainingSeconds));
        }
    }
}
