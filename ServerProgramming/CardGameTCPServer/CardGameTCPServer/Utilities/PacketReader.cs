using System.Net.Sockets;

namespace CardGameTCPServer.Utilities
{
    public static class PacketReader
    {
        public static async Task<int> ReadInt32Async(NetworkStream stream)
        {
            byte[] buffer = new byte[4];

            int bytesRead = 0;

            while (bytesRead < 4)
            {
                int result = await stream.ReadAsync(
                    buffer,
                    bytesRead,
                    4 - bytesRead);

                if (result == 0)
                    throw new Exception("Disconnected");

                bytesRead += result;
            }

            return BitConverter.ToInt32(buffer, 0);
        }

        public static async Task<byte[]> ReadBytesAsync(NetworkStream stream, int length)
        {
            byte[] buffer = new byte[length];

            int bytesRead = 0;

            while (bytesRead < length)
            {
                int result = await stream.ReadAsync(
                    buffer,
                    bytesRead,
                    length - bytesRead);

                if (result == 0)
                    throw new Exception("Disconnected");

                bytesRead += result;
            }

            return buffer;
        }
    }
}
