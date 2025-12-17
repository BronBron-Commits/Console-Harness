using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SocketClient.Protocol
{
    public static class AuthEnvelopeDecoder
    {
        public static void Decode(byte[] envelope)
        {
            Console.WriteLine("[decode] server envelope length = " + envelope.Length);

            // ---- Header (pre-zlib) ----
            // Expected layout:
            // 0x00–0x01 : total length
            // 0x02–0x03 : message type
            // 0x04–0x05 : flags / wildcard
            // 0x06–0x07 : protocol phase
            // 0x08–0x09 : reserved
            // 0x0A..    : zlib payload

            ushort totalLen = ReadU16BE(envelope, 0);
            ushort msgType  = ReadU16BE(envelope, 2);
            ushort flags    = ReadU16BE(envelope, 4);
            ushort phase    = ReadU16BE(envelope, 6);
            ushort reserved = ReadU16BE(envelope, 8);

            Console.WriteLine($"[decode] totalLen = {totalLen}");
            Console.WriteLine($"[decode] msgType  = 0x{msgType:X4}");
            Console.WriteLine($"[decode] flags    = 0x{flags:X4}");
            Console.WriteLine($"[decode] phase    = 0x{phase:X4}");
            Console.WriteLine($"[decode] reserved = 0x{reserved:X4}");

            // ---- Zlib payload ----
            int zlibOffset = 10;

            if (envelope[zlibOffset] != 0x78)
            {
                Console.WriteLine("[decode] ERROR: expected zlib header at offset 0x0A");
                return;
            }

            byte[] compressed = envelope.Skip(zlibOffset).ToArray();
            byte[] inflated   = InflateZlib(compressed);

            Console.WriteLine($"[decode] inflated payload length = {inflated.Length}");
            HexDump("INFLATED", inflated);
        }

        private static byte[] InflateZlib(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var zlib  = new ZLibStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            zlib.CopyTo(output);
            return output.ToArray();
        }

        private static ushort ReadU16BE(byte[] buf, int offset)
        {
            return (ushort)((buf[offset] << 8) | buf[offset + 1]);
        }

        private static void HexDump(string label, byte[] data)
        {
            Console.WriteLine($"[{label}]");
            for (int i = 0; i < data.Length; i += 16)
            {
                var slice = data.Skip(i).Take(16).ToArray();
                Console.Write($"{i:X4}: ");
                foreach (var b in slice)
                    Console.Write($"{b:X2} ");
                Console.WriteLine();
            }
        }
    }
}
