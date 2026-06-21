using System.Text.Json;

namespace CardGameTCPServer
{
    public static class ConfigManager
    {
        public static ServerConfig Config { get; private set; }

        public static void Load()
        {
            string json = File.ReadAllText("config.json");

            Config = JsonSerializer.Deserialize<ServerConfig>(json);

            if (Config == null)
            {
                throw new Exception("Failed to load config.json");
            }

            if (Config.Port <= 0)
            {
                throw new Exception("Invalid Port");
            }

            if (Config.DisconnectTimeoutSeconds <= 0)
            {
                throw new Exception("Invalid Disconnect Timeout");
            }
        }
    }
}
