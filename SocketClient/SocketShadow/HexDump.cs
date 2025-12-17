using System;
using System.Text;

namespace SocketClient
{
    public static class HexDump
    {
        public static void Dump(byte[] buffer, int length, string prefix)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                sb.Append(buffer[i].ToString("X2")).Append(' ');
            }

            Console.WriteLine($"{prefix} [{length} bytes]");
            Console.WriteLine(sb.ToString());
        }
    }
}
