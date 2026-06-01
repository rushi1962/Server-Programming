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
        LeaveGame = 4
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
