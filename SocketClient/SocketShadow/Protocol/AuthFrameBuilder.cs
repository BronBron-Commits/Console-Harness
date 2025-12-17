using System;
using System.IO;
using System.IO.Compression;

namespace SocketClient.Protocol
{
    /// <summary>
    /// Builds outbound DeltaWorlds authentication frames
    /// matching the observed server envelope format.
    /// </summary>
    public static class AuthFrameBuilder
    {
        /// <summary>
        /// Builds a complete auth envelope frame.
        /// Header fields are written big-endian.
        /// Payload is zlib-compressed.
        /// </summary>
        public static byte[] Build(
            ushort messageType,
            ushort flags,
            ushort phase,
            byte[] inflatedPayload)
        {
            byte[] compressedPayload = CompressZlib(inflatedPayload);

            ushort totalLength = (ushort)(10 + compressedPayload.Length);

            using var ms = new MemoryStream(totalLength);

            WriteBE(ms, totalLength);
            WriteBE(ms, messageType);
            WriteBE(ms, flags);
            WriteBE(ms, phase);
            WriteBE(ms, 0x0000); // reserved

            ms.Write(compressedPayload, 0, compressedPayload.Length);

            return ms.ToArray();
        }

        private static byte[] CompressZlib(byte[] data)
        {
            using var output = new MemoryStream();
            using (var z = new ZLibStream(output, CompressionLevel.Optimal, leaveOpen: true))
            {
                z.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        private static void WriteBE(Stream s, ushort value)
        {
            s.WriteByte((byte)(value >> 8));
            s.WriteByte((byte)(value & 0xFF));
        }
    }
}
