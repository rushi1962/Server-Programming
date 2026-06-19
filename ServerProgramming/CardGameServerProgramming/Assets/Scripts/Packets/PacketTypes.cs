namespace Packets
{
    public enum PacketType
    {
        SystemPacket = 1,
        MatchMakingLobbyPacket = 2,
        GamePacket = 3
    }

    public enum SystemPacketTypes
    {
        MatchMakingRequested = 1,
        ClientName = 2,
        ClientUUID = 3,
        LeaveMatch = 4,
        HeartBeat = 5,
        ReconnectionToken = 6,
        ReconnectionSuccess = 7
    }

    public enum GamePacketTypes
    {
        GameActionPacket = 1,
        GameStateUpdatePacket = 2,
        GameStarted = 3
    }

    public enum GameActionTypes
    {
        GameAction_Attack = 1,
        GameAction_Heal = 2,
        GameAction_ManaBoost = 3,
    }
}
