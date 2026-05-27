namespace Shared
{
    public enum PacketType
    {
        ChatMessage = 1,
        PlayerJoined = 2,
        PlayerLeft = 3,
        TurnChanged = 4,
        PlayAttackCard = 5,
        PlayHealCard = 6,
        PlayManaCard = 7,
        GameStateUpdate = 8,
        GameOver = 9
    }
}
