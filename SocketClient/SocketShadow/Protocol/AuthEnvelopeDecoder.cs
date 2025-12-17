using System;
using System.IO;
using System.IO.Compression;

namespace SocketClient.Protocol
{
    internal static class AuthEnvelopeDecoder
    {
        public static byte[] LastPayloadRaw;
        public static byte[] LastInflatedPayload;

        public static void Decode(byte[] frame)
        {
            using var ms = new MemoryStream(frame);
            using var br = new BinaryReader(ms);

            ushort totalLen = ReadBE16(br);
            ushort msgType  = ReadBE16(br);
            ushort flags    = ReadBE16(br);
            ushort phase    = ReadBE16(br);
            ushort reserved = ReadBE16(br);

            Console.WriteLine($"[decode] totalLen = {totalLen}");
            Console.WriteLine($"[decode] msgType  = 0x{msgType:X4}");
            Console.WriteLine($"[decode] flags    = 0x{flags:X4}");
            Console.WriteLine($"[decode] phase    = 0x{phase:X4}");
            Console.WriteLine($"[decode] reserved = 0x{reserved:X4}");

            int payloadLen = frame.Length - 10;
            if (payloadLen <= 0)
            {
                Console.WriteLine("[decode] no payload");
                return;
            }

            byte[] payload = br.ReadBytes(payloadLen);
            LastPayloadRaw = payload;

            bool looksZlib = payload.Length >= 2 && payload[0] == 0x78;

            Console.WriteLine($"[decode] compressed payload length = {payload.Length}");
            Console.WriteLine($"[decode] zlib header detected = {looksZlib}");

            if (!looksZlib)
            {
                Console.WriteLine("[decode] payload left untouched");
                return;
            }

            // ---- SAFE INFLATE (BEST-EFFORT ONLY) ----
            try
            {
                using var comp = new MemoryStream(payload);
                using var zlib = new DeflateStream(comp, CompressionMode.Decompress, leaveOpen: true);
                using var outMs = new MemoryStream();

                zlib.CopyTo(outMs);
                LastInflatedPayload = outMs.ToArray();

                Console.WriteLine($"[decode] inflated payload length = {LastInflatedPayload.Length}");
                HexDump.Dump(LastInflatedPayload, LastInflatedPayload.Length, "[INFLATED]");
            }
            catch (InvalidDataException)
            {
                // EXPECTED for Phase-1 on some servers
                Console.WriteLine("[decode] inflate failed (nonstandard compression)");
                Console.WriteLine("[decode] payload preserved raw (expected behavior)");
                LastInflatedPayload = null;
            }
        }

        private static ushort ReadBE16(BinaryReader br)
        {
            var b = br.ReadBytes(2);
            return (ushort)((b[0] << 8) | b[1]);
        }
    }
}
