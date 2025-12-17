using System;

namespace SocketClient.Protocol
{
    /// <summary>
    /// Represents a decoded DeltaWorlds authentication envelope.
    /// Header is fixed-width, payload is zlib-compressed.
    /// </summary>
    public sealed class AuthEnvelope
    {
        // ---- Raw header fields (big-endian on wire) ----
        public ushort TotalLength { get; }
        public ushort MessageType { get; }
        public ushort Flags { get; }
        public ushort Phase { get; }
        public ushort Reserved { get; }

        // ---- Payloads ----
        public byte[] CompressedPayload { get; }
        public byte[] InflatedPayload { get; }

        public AuthEnvelope(
            ushort totalLength,
            ushort messageType,
            ushort flags,
            ushort phase,
            ushort reserved,
            byte[] compressedPayload,
            byte[] inflatedPayload)
        {
            TotalLength = totalLength;
            MessageType = messageType;
            Flags = flags;
            Phase = phase;
            Reserved = reserved;
            CompressedPayload = compressedPayload;
            InflatedPayload = inflatedPayload;
        }

        public override string ToString()
        {
            return $"AuthEnvelope(len={TotalLength}, type=0x{MessageType:X4}, flags=0x{Flags:X4}, phase=0x{Phase:X4})";
        }
    }
}
