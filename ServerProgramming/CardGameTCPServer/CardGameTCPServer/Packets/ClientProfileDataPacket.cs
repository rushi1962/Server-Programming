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
        public string ReconnectToken { get; }

        public ClientProfileDataPacket(int clientID, string reconnectToken)
        {
            ClientID = clientID;
            ReconnectToken = reconnectToken;
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

            //Send ReconnectionToken
            byte[] tokenData = Encoding.UTF8.GetBytes(ReconnectToken);

            await stream.WriteAsync(BitConverter.GetBytes((int)PacketType.SystemPacket));
            await stream.WriteAsync(BitConverter.GetBytes((int)SystemPacketTypes.ReconnectionToken));
            await stream.WriteAsync(BitConverter.GetBytes(tokenData.Length));
            await stream.WriteAsync(tokenData);
        }
    }
}
