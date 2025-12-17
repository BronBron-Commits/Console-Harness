using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SocketClient.Protocol
{
    public static class AuthEnvelopeDecoder
    {
        public static byte[] LastInflatedPayload { get; private set; } = Array.Empty<byte>();

        public static void Decode(byte[] frame)
        {
            ushort totalLen = ReadU16(frame, 0);
            ushort msgType  = ReadU16(frame, 2);
            ushort flags    = ReadU16(frame, 4);
            ushort phase    = ReadU16(frame, 6);
            ushort reserved = ReadU16(frame, 8);

            Console.WriteLine($"[decode] totalLen = {totalLen}");
            Console.WriteLine($"[decode] msgType  = 0x{msgType:X4}");
            Console.WriteLine($"[decode] flags    = 0x{flags:X4}");
            Console.WriteLine($"[decode] phase    = 0x{phase:X4}");
            Console.WriteLine($"[decode] reserved = 0x{reserved:X4}");

            byte[] payload = frame.Skip(10).ToArray();

            // ---- ZLIB HEADER CHECK ----
            bool looksLikeZlib =
                payload.Length >= 2 &&
                payload[0] == 0x78 &&
                (payload[1] == 0x9C || payload[1] == 0xDA || payload[1] == 0x01);

            Console.WriteLine($"[decode] compressed payload length = {payload.Length}");
            Console.WriteLine($"[decode] zlib header detected = {looksLikeZlib}");

            // Attempt safe inflate
            if (!TryInflate(payload, looksLikeZlib, out var inflated))
            {
                Console.WriteLine("[decode] inflation failed (expected for some phases)");
                LastInflatedPayload = payload;
                return;
            }

            LastInflatedPayload = inflated;
            Console.WriteLine($"[decode] inflated payload length = {inflated.Length}");
            HexDump.Dump(inflated, inflated.Length, "[INFLATED]");
        }

        // -------------------------------------------------------------

        private static bool TryInflate(byte[] data, bool zlibWrapped, out byte[] result)
        {
            try
            {
                byte[] source = data;

                // Strip zlib header if present
                if (zlibWrapped)
                {
                    source = data.Skip(2).ToArray();
                }

                using var input = new MemoryStream(source);
                using var inflater = new DeflateStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();

                inflater.CopyTo(output);
                result = output.ToArray();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[decode] inflate error: {ex.GetType().Name} - {ex.Message}");
                result = Array.Empty<byte>();
                return false;
            }
        }

        private static ushort ReadU16(byte[] b, int o)
            => (ushort)((b[o] << 8) | b[o + 1]);
    }
}
