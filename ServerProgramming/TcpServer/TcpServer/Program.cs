using System.Net;
using System.Net.Sockets;
using System.Text;
using Server;
using Shared;

class Program
{
    static List<ClientConnection> clients =
    new List<ClientConnection>();

    static int nextPlayerId = 1;

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
                        BroadcastSystemMessage($"{client.PlayerName}: {message}", PacketType.ChatMessage);

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

    static void BroadcastSystemMessage(string message, PacketType packetType)
    {
        if(packetType != PacketType.ChatMessage)
        {
            Console.WriteLine(message);
        }

        foreach (ClientConnection client in clients)
        {
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