using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer.Packets
{
    public enum GamePacketTypes
    {
        GameActionPacket = 1,
        GameStateUpdatePacket = 2
    }
}
