using System.Net.Sockets;
using System.Text;
using Shared;

class Program
{
    static void Main(string[] args)
    {
        TcpClient client = new TcpClient();

        Console.WriteLine("Connecting...");

        client.Connect("127.0.0.1", 7777);

        Console.WriteLine("Connected!");

        NetworkStream stream = client.GetStream();

        Thread receiveThread =
            new Thread(() => ReceiveMessages(stream));

        receiveThread.Start();

        while (true)
        {
            string input = Console.ReadLine();

            PacketType packetType;
            string payload = "";

            switch (input.ToLower())
            {
                case "/attack":
                    packetType = PacketType.PlayAttackCard;
                    break;

                case "/heal":
                    packetType = PacketType.PlayHealCard;
                    break;

                case "/mana":
                    packetType = PacketType.PlayManaCard;
                    break;

                default:
                    packetType = PacketType.ChatMessage;
                    payload = input;
                    break;
            }

            byte[] data =
                Encoding.UTF8.GetBytes(payload);

            BinaryWriter writer =
                new BinaryWriter(stream);

            writer.Write((int)packetType);

            writer.Write(data.Length);

            if (data.Length > 0)
            {
                writer.Write(data);
            }
        }
    }

    static void ReceiveMessages(NetworkStream stream)
    {
        BinaryReader reader = new BinaryReader(stream);

        try
        {
            while (true)
            {
                int packetTypeValue = reader.ReadInt32();

                PacketType packetType =
                    (PacketType)packetTypeValue;

                int messageLength = reader.ReadInt32();

                byte[] data = reader.ReadBytes(messageLength);

                string message =
                    Encoding.UTF8.GetString(data);

                switch (packetType)
                {
                    case PacketType.ChatMessage:
                        Console.WriteLine($"Chat: {message}");
                        break;

                    case PacketType.PlayerJoined:
                        Console.WriteLine($"SYSTEM: {message}");
                        break;

                    case PacketType.PlayerLeft:
                        Console.WriteLine($"SYSTEM: {message}");
                        break;

                    case PacketType.TurnChanged:
                        Console.WriteLine($"TURN: {message}");
                        break;

                    case PacketType.GameStateUpdate:
                        Console.WriteLine($"STATE: {message}");
                        break;

                    case PacketType.GameOver:
                        Console.WriteLine($"GAME OVER: {message}");
                        break;
                }
            }
        }
        catch
        {
            Console.WriteLine("Disconnected from server");
        }
    }
}