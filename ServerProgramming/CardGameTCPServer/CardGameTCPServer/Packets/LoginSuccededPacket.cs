using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer.Packets
{
    public class LoginSuccededPacket : IOutgoingPacket
    {
        public int AccountID { get; }

        public LoginSuccededPacket(int accountID)
        {
            AccountID = accountID;
        }

        public async Task WriteAsync(NetworkStream stream)
        {
            await stream.WriteAsync(BitConverter.GetBytes((int)PacketType.SystemPacket));
            await stream.WriteAsync(BitConverter.GetBytes((int)SystemPacketTypes.LoginSuccess));
            await stream.WriteAsync(BitConverter.GetBytes(AccountID));
        }
    }
}
