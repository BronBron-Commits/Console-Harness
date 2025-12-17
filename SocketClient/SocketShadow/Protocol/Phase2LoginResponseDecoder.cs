using System;
using System.IO;
using System.Text;

namespace SocketClient.Protocol
{
    public static class Phase2LoginResponseDecoder
    {
        public static void Decode(byte[] packet)
        {
            Console.WriteLine("[decode] PHASE2 login response");
            Console.WriteLine($"[decode] total length = {packet.Length}");

            int offset = 0;

            ushort totalLen = ReadU16(packet, ref offset);
            ushort msgType  = ReadU16(packet, ref offset);
            ushort flags    = ReadU16(packet, ref offset);
            ushort phase    = ReadU16(packet, ref offset);
            ushort reserved = ReadU16(packet, ref offset);

            Console.WriteLine($"[decode] totalLen = {totalLen}");
            Console.WriteLine($"[decode] msgType  = 0x{msgType:X4}");
            Console.WriteLine($"[decode] flags    = 0x{flags:X4}");
            Console.WriteLine($"[decode] phase    = 0x{phase:X4}");
            Console.WriteLine($"[decode] reserved = 0x{reserved:X4}");

            int payloadLen = packet.Length - offset;
            Console.WriteLine($"[decode] payload length = {payloadLen}");

            DumpLayout(packet, offset);
        }

        private static ushort ReadU16(byte[] buf, ref int off)
        {
            ushort v = (ushort)((buf[off] << 8) | buf[off + 1]);
            off += 2;
            return v;
        }

        private static void DumpLayout(byte[] buf, int start)
        {
            Console.WriteLine("[layout] offset  size  description");

            int off = start;
            int index = 0;

            while (off < buf.Length)
            {
                int remaining = buf.Length - off;
                int size = Math.Min(16, remaining);

                Console.WriteLine(
                    $"[layout] 0x{off:X4}  {size,4}  {BitConverter.ToString(buf, off, size)}"
                );

                off += size;
                index++;
            }
        }
    }
}
