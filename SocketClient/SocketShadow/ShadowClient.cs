using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using SocketClient.Protocol;

namespace SocketClient
{
    internal static class ShadowClient
    {
        private const string HOST = "auth.deltaworlds.com";
        private const int PORT = 6671;

        static void Main()
        {
            Log("starting");
            Directory.CreateDirectory("captures");

            var sw = Stopwatch.StartNew();

            using var client = new TcpClient { NoDelay = true };
            Log("connecting...");
            client.Connect(HOST, PORT);
            Log($"connected @ {sw.ElapsedMilliseconds}ms");

            using var stream = client.GetStream();
            stream.ReadTimeout = 8000;
            stream.WriteTimeout = 8000;

            Thread.Sleep(TimingProfile.AfterConnectDelay);

            // -------------------------------------------------
            // CLIENT HELLO (verified)
            // -------------------------------------------------
            byte[] clientHello =
            {
                0x00, 0x0A,
                0x00, 0x02,
                0x00, 0x24,
                0x00, 0x03,
                0x00, 0x00
            };

            Send(stream, clientHello, "client-hello");
            Thread.Sleep(TimingProfile.BeforeLoginDelay);

            // -------------------------------------------------
            // PHASE 1 RESPONSE
            // -------------------------------------------------
            byte[] phase1 = ReadFrame(stream, "phase1");
            AuthEnvelopeDecoder.Decode(phase1);

            // -------------------------------------------------
            // LOGIN FRAME (STRUCTURAL ONLY — already verified)
            // -------------------------------------------------
            Log("sending login-frame candidate");
            byte[] loginFrame = AuthFrameBuilder.BuildPhase2Probe();
            Send(stream, loginFrame, "login-frame");

            // -------------------------------------------------
            // PHASE 2 SERVER CHALLENGE (CAPTURE ONLY)
            // -------------------------------------------------
            Log("waiting for phase-2 challenge");
            byte[] phase2 = ReadFrame(stream, "phase2");

            Phase2ChallengeDecoder.DecodeHeaderOnly(phase2);

            Log("phase-2 captured successfully — exiting cleanly");
        }

        private static byte[] ReadFrame(NetworkStream stream, string label)
        {
            var buffer = new byte[8192];
            int read = stream.Read(buffer, 0, buffer.Length);

            if (read <= 0)
                throw new IOException("server closed connection");

            byte[] frame = buffer.Take(read).ToArray();
            string path = $"captures/{label}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.bin";
            File.WriteAllBytes(path, frame);

            Log($"received {label} ({read} bytes)");
            HexDump.Dump(frame, frame.Length, "[RX]");

            return frame;
        }

        private static void Send(NetworkStream s, byte[] data, string label)
        {
            s.Write(data, 0, data.Length);
            s.Flush();
            Log($"sent {label} ({data.Length} bytes)");
        }

        private static void Log(string msg)
        {
            Console.WriteLine($"[shadow] {msg}");
        }
    }
}
