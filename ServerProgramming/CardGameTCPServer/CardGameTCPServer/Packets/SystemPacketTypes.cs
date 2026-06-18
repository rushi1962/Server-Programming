namespace CardGameTCPServer.Packets
{
    public enum SystemPacketTypes
    {
        MatchMakingRequested = 1,
        ClientName = 2,
        ClientUUID = 3,
        LeaveMatch = 4,
        HeartBeat = 5
    }
}
