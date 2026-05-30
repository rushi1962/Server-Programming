using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameTCPServer.Packets
{
    public enum GameActionTypes
    {
        GameAction_Attack = 1,
        GameAction_Heal = 2,
        GameAction_ManaBoost = 3,
    }
}
