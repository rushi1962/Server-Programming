using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using Server;
using Shared;

class Program
{
    static List<ClientConnection> clients =
    new List<ClientConnection>();

    static int nextPlayerId = 1;

    static List<Match> matches =
    new List<Match>();

    static int nextMatchId = 1;

    static Random random = new Random();

    static void Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, 7777);

        server.Start();

        Console.WriteLine("Server started on port 7777");

        while (true)
        {
            Console.WriteLine("Waiting for client...");

            TcpClient tcpClient = server.AcceptTcpClient();

            ClientConnection client =
                new ClientConnection(
                    tcpClient,
                    nextPlayerId++
                );

            BroadcastSystemMessage($"{client.PlayerName} joined the server", PacketType.PlayerJoined);

            clients.Add(client);

            Thread clientThread = new Thread(() => HandleClient(client));

            clientThread.Start();

            TryCreateMatch();
        }
    }

    static void HandleClient(ClientConnection client)
    {
        try
        {
            while (true)
            {
                int packetTypeValue = client.Reader.ReadInt32();

                PacketType packetType =
                    (PacketType)packetTypeValue;

                int messageLength = client.Reader.ReadInt32();

                byte[] data = client.Reader.ReadBytes(messageLength);

                string message = Encoding.UTF8.GetString(data);

                switch (packetType)
                {
                    case PacketType.ChatMessage:

                        if(client.CurrentMatch != null)
                        {
                            BroadcastToMatch(client.CurrentMatch, PacketType.ChatMessage, $"{client.PlayerName}: {message}", client, true);
                        }
                        else
                        {
                            BroadcastSystemMessage($"{client.PlayerName}: {message}", PacketType.ChatMessage, client);
                        }        
                        break;

                    case PacketType.PlayerJoined:
                        BroadcastSystemMessage($"{client.PlayerName} joined the server", PacketType.PlayerJoined);
                        break;

                    case PacketType.PlayAttackCard:
                        HandleAttackCard(client);
                        break;

                    case PacketType.PlayHealCard:
                        HandleHealCard(client);
                        break;

                    case PacketType.PlayManaCard:
                        HandleManaCard(client);
                        break;
                }
            }
        }
        catch
        {
            BroadcastSystemMessage($"{client.PlayerName} left the server", PacketType.PlayerLeft);
        }

        if (client.CurrentMatch != null && !client.CurrentMatch.IsGameOver)
        {
            Match match = client.CurrentMatch;

            match.IsGameOver = true;

            ClientConnection remainingPlayer = match.Players.FirstOrDefault(p => p != client);

            if (remainingPlayer != null)
            {
                SendPacket(remainingPlayer, PacketType.GameOver, "Opponent disconnected. You win!" );
            }

            Console.WriteLine($"Match {match.MatchId} ended due to disconnect");

            EndMatch(match);
        }

        clients.Remove(client);

        client.TcpClient.Close();
    }

    static bool IsPlayersTurn(ClientConnection client)
    {
        return client.CurrentMatch
                     .GetCurrentPlayer() == client;
    }

    private static void HandleManaCard(ClientConnection client)
    {
        Match match = client.CurrentMatch;

        if (match.IsGameOver)
        {
            return;
        }

        if (!IsPlayersTurn(client))
            return;

        int manaGain =
            random.Next(3, 6);

        client.Mana += manaGain;

        if (client.Mana > 20)
        {
            client.Mana = 20;
        }

        BroadcastToMatch(
            match,
            PacketType.ChatMessage,
            $"{client.PlayerName} gained {manaGain} mana!"
        );

        SendGameState(match);

        if (!match.IsGameOver)
        {
            AdvanceTurn(match);
        }
    }

    private static void HandleHealCard(ClientConnection client)
    {
        Match match = client.CurrentMatch;

        if (match.IsGameOver)
        {
            return;
        }

        if (!IsPlayersTurn(client))
            return;

        if (client.Mana < 4)
        {
            SendPacket(
                client,
                PacketType.ChatMessage,
                "Not enough mana!"
            );

            return;
        }

        int healAmount =
            random.Next(2, 7);

        client.Mana -= 4;

        client.Health += healAmount;

        if (client.Health > 20)
        {
            client.Health = 20;
        }

        BroadcastToMatch(
            match,
            PacketType.ChatMessage,
            $"{client.PlayerName} healed {healAmount} HP!"
        );

        SendGameState(match);

        if (!match.IsGameOver)
        {
            AdvanceTurn(match);
        }
    }

    private static void HandleAttackCard(ClientConnection client)
    {
        Match match = client.CurrentMatch;

        if (match.IsGameOver)
        {
            return;
        }

        if (!IsPlayersTurn(client))
            return;

        if (client.Mana < 5)
        {
            SendPacket(
                client,
                PacketType.ChatMessage,
                "Not enough mana!"
            );

            return;
        }

        ClientConnection opponent =
            match.Players
                 .First(p => p != client);

        int damage = random.Next(3, 9);

        client.Mana -= 5;

        opponent.Health -= damage;

        BroadcastToMatch(
            match,
            PacketType.ChatMessage,
            $"{client.PlayerName} dealt {damage} damage!"
        );

        SendGameState(match);

        CheckGameOver(match);

        if (!match.IsGameOver)
        {
            AdvanceTurn(match);
        }
    }

    static void SendGameState(Match match)
    {
        string gameState =
        "\n===== GAME STATE =====\n";

        foreach (ClientConnection player in match.Players)
        {
            gameState +=
                $"{player.PlayerName} | " +
                $"HP: {player.Health} | " +
                $"Mana: {player.Mana}\n";
        }

        gameState +=
            "======================";

        BroadcastToMatch(
            match,
            PacketType.GameStateUpdate,
            gameState
        );
    }

    static void AdvanceTurn(Match match)
    {
        match.CurrentTurnPlayerIndex =
            (match.CurrentTurnPlayerIndex + 1)
            % match.Players.Count;

        ClientConnection currentPlayer =
            match.GetCurrentPlayer();

        BroadcastToMatch(
            match,
            PacketType.TurnChanged,
            $"{currentPlayer.PlayerName}'s turn"
        );
    }

    static void CheckGameOver(Match match)
    {
        if (match.IsGameOver)
        {
            return;
        }

        foreach (ClientConnection player in match.Players)
        {
            if (player.Health <= 0)
            {
                ClientConnection winner =
                    match.Players
                         .First(p => p != player);

                BroadcastToMatch(
                    match,
                    PacketType.GameOver,
                    $"{winner.PlayerName} wins!"
                );

                Console.WriteLine(
                    $"Match {match.MatchId} ended"
                );

                EndMatch(match);

                return;
            }
        }
    }

    static void SendPacket(ClientConnection client, PacketType packetType, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        client.Writer.Write((int)packetType);

        client.Writer.Write(data.Length);

        client.Writer.Write(data);
    }

    static void BroadcastSystemMessage(string message, PacketType packetType, ClientConnection sender = null)
    {
        if(packetType != PacketType.ChatMessage)
        {
            Console.WriteLine(message);
        }

        foreach (ClientConnection client in clients)
        {
            if (client == sender)
            {
                continue;
            }
            try
            {
                if (client.CurrentMatch == null)
                {
                    SendPacket(client, packetType, message);
                }
            }
            catch
            {
            }
        }
    }

    static void TryCreateMatch()
    {
        List<ClientConnection> waitingPlayers =
            clients.Where(c => c.CurrentMatch == null)
                   .ToList();

        if (waitingPlayers.Count < 2)
            return;

        ClientConnection player1 = waitingPlayers[0];
        ClientConnection player2 = waitingPlayers[1];

        Match match = new Match(nextMatchId++);

        match.Players.Add(player1);
        match.Players.Add(player2);

        player1.CurrentMatch = match;
        player2.CurrentMatch = match;

        matches.Add(match);

        Console.WriteLine(
            $"Created Match {match.MatchId}"
        );

        BroadcastToMatch(match, PacketType.ChatMessage, $"Match {match.MatchId} started!");
        SendGameState(match);

        ClientConnection currentPlayer = match.GetCurrentPlayer();

        BroadcastToMatch(match, PacketType.TurnChanged, $"{currentPlayer.PlayerName}'s turn");
    }

    static void BroadcastToMatch(Match match, PacketType packetType, string message, ClientConnection sender = null, bool excludeSender = false)
    {
        foreach (ClientConnection client in match.Players)
        {
            if (excludeSender && client == sender)
            {
                continue;
            }

            try
            {
                SendPacket(client, packetType, message);
            }
            catch
            {
            }
        }
    }

    static void EndMatch(Match match)
    {
        match.IsGameOver = true;

        foreach (ClientConnection player in match.Players)
        {
            player.CurrentMatch = null;
        }

        matches.Remove(match);

        Console.WriteLine($"Cleaned up Match {match.MatchId}");
    }
}