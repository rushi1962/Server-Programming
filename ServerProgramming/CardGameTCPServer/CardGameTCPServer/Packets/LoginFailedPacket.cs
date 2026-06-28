using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer.Packets
{
    public class LoginFailedPacket : IOutgoingPacket
    {
        public string Reason { get; }

        public LoginFailedPacket(string reason)
        {
            Reason = reason; 
        }

        public async Task WriteAsync(NetworkStream stream)
        {
            await stream.WriteAsync(BitConverter.GetBytes((int)PacketType.SystemPacket));
            await stream.WriteAsync(BitConverter.GetBytes((int)SystemPacketTypes.LoginFailed));

            byte[] data = Encoding.UTF8.GetBytes(Reason);
            await stream.WriteAsync(BitConverter.GetBytes(data.Length));
            await stream.WriteAsync(data);
        }
    }
}
