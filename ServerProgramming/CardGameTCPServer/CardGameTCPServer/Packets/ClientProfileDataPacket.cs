using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer.Packets
{
    public class ClientProfileDataPacket : IOutgoingPacket
    {
        public int ClientID { get; }

        public ClientProfileDataPacket(int clientID)
        {
            ClientID = clientID;
        }

        public async Task WriteAsync(NetworkStream stream)
        {
            //Send client ID
            await stream.WriteAsync(BitConverter.GetBytes((int)PacketType.SystemPacket));
            await stream.WriteAsync(BitConverter.GetBytes((int)SystemPacketTypes.ClientUUID));
            await stream.WriteAsync(BitConverter.GetBytes(ClientID));

            //Send client profile name
            string clientName = $"Player{ClientID}";

            byte[] data = Encoding.UTF8.GetBytes(clientName);

            await stream.WriteAsync(BitConverter.GetBytes((int)PacketType.SystemPacket));
            await stream.WriteAsync(BitConverter.GetBytes((int)SystemPacketTypes.ClientName));
            await stream.WriteAsync(BitConverter.GetBytes(data.Length));
            await stream.WriteAsync(data);
        }
    }
}
