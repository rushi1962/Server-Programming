namespace CardGameTCPServer.Packets
{
    public enum SystemPacketTypes
    {
        MatchMakingRequested = 1,
        ClientName = 2,
        ClientUUID = 3,
        LeaveMatch = 4,
        HeartBeat = 5,
        ReconnectionToken = 6,
        ReconnectionSuccess = 7,
        ServerShutdownCountdownStarted = 8,
        ServerShutdown = 9,
        LoginGuest = 10,
        LoginWithAccountID = 11,
        LoginSuccess = 12,
        LoginFailed = 13,
    }
}
