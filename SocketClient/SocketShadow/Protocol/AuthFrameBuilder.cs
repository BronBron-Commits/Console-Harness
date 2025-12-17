namespace SocketClient.Protocol
{
    internal static class AuthFrameBuilder
    {
        public static byte[] BuildPhase2Probe()
        {
            // STRUCTURAL replay only — known-good envelope
            return new byte[]
            {
                0x00, 0x0A,
                0x00, 0x02,
                0x00, 0x24,
                0x00, 0x03,
                0x00, 0x00
            };
        }
    }
}
