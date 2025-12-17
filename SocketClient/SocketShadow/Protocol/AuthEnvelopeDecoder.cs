using System;
using System.IO;
using System.IO.Compression;

namespace SocketClient.Protocol
{
    public static class AuthEnvelopeDecoder
    {
        // =========================================================
        // Public state (used by ShadowClient)
        // =========================================================

        public static byte[] LastInflatedPayload { get; private set; }

        // =========================================================
        // Decode
        // =========================================================

        public static void Decode(byte[] envelope)
        {
            if (envelope.Length < 10)
                throw new InvalidDataException("Envelope too short");

            ushort totalLen = ReadBE(envelope, 0);
            ushort msgType  = ReadBE(envelope, 2);
            ushort flags    = ReadBE(envelope, 4);
            ushort phase    = ReadBE(envelope, 6);
            ushort reserved = ReadBE(envelope, 8);

            Console.WriteLine($"[decode] server envelope length = {envelope.Length}");
            Console.WriteLine($"[decode] totalLen = {totalLen}");
            Console.WriteLine($"[decode] msgType  = 0x{msgType:X4}");
            Console.WriteLine($"[decode] flags    = 0x{flags:X4}");
            Console.WriteLine($"[decode] phase    = 0x{phase:X4}");
            Console.WriteLine($"[decode] reserved = 0x{reserved:X4}");

            int payloadOffset = 10;
            int payloadLen = envelope.Length - payloadOffset;

            byte[] compressed = new byte[payloadLen];
            Buffer.BlockCopy(envelope, payloadOffset, compressed, 0, payloadLen);

            byte[] inflated = InflateZlib(compressed);

            LastInflatedPayload = inflated;

            Console.WriteLine($"[decode] inflated payload length = {inflated.Length}");
            Console.WriteLine("[INFLATED]");
            HexDump.Dump(inflated, inflated.Length, null);
        }

        // =========================================================
        // Helpers
        // =========================================================

        private static ushort ReadBE(byte[] buf, int offset)
        {
            return (ushort)((buf[offset] << 8) | buf[offset + 1]);
        }

        private static byte[] InflateZlib(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var zlib  = new ZLibStream(input, CompressionMode.Decompress);
            using var outMs = new MemoryStream();
            zlib.CopyTo(outMs);
            return outMs.ToArray();
        }
    }
}
