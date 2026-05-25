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
                            BroadcastToMatch(client.CurrentMatch, PacketType.ChatMessage, $"{client.PlayerName}: {message}", client);
                        }
                        else
                        {
                            BroadcastSystemMessage($"{client.PlayerName}: {message}", PacketType.ChatMessage, client);
                        }        
                        break;

                    case PacketType.PlayerJoined:
                        BroadcastSystemMessage($"{client.PlayerName} joined the server", PacketType.PlayerJoined);
                        break;
                }
            }
        }
        catch
        {
            BroadcastSystemMessage($"{client.PlayerName} left the server", PacketType.PlayerLeft);
        }

        clients.Remove(client);

        client.TcpClient.Close();
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
    }

    static void BroadcastToMatch(Match match, PacketType packetType, string message, ClientConnection sender = null)
    {
        foreach (ClientConnection client in match.Players)
        {
            if (client == sender)
                continue;

            try
            {
                SendPacket(client, packetType, message);
            }
            catch
            {
            }
        }
    }
}