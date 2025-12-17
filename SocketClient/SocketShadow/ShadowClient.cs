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
        // =========================================================
        // Target
        // =========================================================

        private const string HOST = "auth.deltaworlds.com";
        private const int PORT = 6671;

        // =========================================================
        // Entry
        // =========================================================

        static void Main()
        {
            Log("starting");
            Directory.CreateDirectory("captures");

            var sw = Stopwatch.StartNew();

            using var client = new TcpClient { NoDelay = true };

            // -----------------------------------------------------
            // PHASE 1: TCP CONNECT
            // -----------------------------------------------------

            Log("connecting...");
            client.Connect(HOST, PORT);
            Log($"connected @ {sw.ElapsedMilliseconds}ms");

            using var stream = client.GetStream();
            stream.ReadTimeout = 5000;
            stream.WriteTimeout = 5000;

            Thread.Sleep(TimingProfile.AfterConnectDelay);

            // -----------------------------------------------------
            // PHASE 2: VERIFIED CLIENT HELLO
            // -----------------------------------------------------

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

            // -----------------------------------------------------
            // PHASE 3: RECEIVE SERVER ENVELOPE
            // -----------------------------------------------------

            byte[] buffer = new byte[8192];
            int read;

            try
            {
                read = stream.Read(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Log($"READ ERROR: {ex.GetType().Name} {ex.Message}");
                return;
            }

            if (read <= 0)
            {
                Log("server closed connection");
                return;
            }

            byte[] envelope = buffer.Take(read).ToArray();
            File.WriteAllBytes("captures/server-envelope.bin", envelope);

            Log($"received server envelope ({read} bytes)");
            HexDump.Dump(envelope, envelope.Length, "[RX]");

            // -----------------------------------------------------
            // PHASE 4: DECODE SERVER ENVELOPE
            // -----------------------------------------------------

            AuthEnvelopeDecoder.Decode(envelope);

            // -----------------------------------------------------
            // PHASE 5: PHASE-2 LOGIN FRAME (STRUCTURAL TEST)
            // -----------------------------------------------------

            Log("building login-frame candidate");

            byte[] inflatedClone = AuthEnvelopeDecoder.LastInflatedPayload;

            byte[] loginFrame = AuthFrameBuilder.Build(
                messageType: 0x0000,
                flags: 0xFFFF,
                phase: 0x0002, // phase-2 attempt
                inflatedPayload: inflatedClone
            );

            Send(stream, loginFrame, "login-frame");

            // -----------------------------------------------------
            // PHASE 6: OBSERVE SERVER BEHAVIOR
            // -----------------------------------------------------

            Log("waiting for phase-2 login response");

            try
            {
                while (client.Connected)
                {
                    if (!stream.DataAvailable)
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    int r = stream.Read(buffer, 0, buffer.Length);
                    if (r <= 0)
                    {
                        Log("server closed connection");
                        break;
                    }

                    byte[] pkt = buffer.Take(r).ToArray();
                    string fname = $"captures/server-response-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.bin";
                    File.WriteAllBytes(fname, pkt);

                    Log($"received {r} bytes");
                    HexDump.Dump(pkt, pkt.Length, "[RX]");
                }
            }
            catch (IOException)
            {
                // This timeout is EXPECTED if the server rejects or ignores the login frame
                Log("no phase-2 response (timeout reached)");
                Log("server is enforcing protocol semantics — boundary confirmed");
            }

            Log("exit");
        }

        // =========================================================
        // Helpers
        // =========================================================

        private static void Send(NetworkStream stream, byte[] data, string label)
        {
            stream.Write(data, 0, data.Length);
            stream.Flush();
            Log($"sent {label} ({data.Length} bytes)");
        }

        private static void Log(string msg)
        {
            Console.WriteLine($"[shadow] {msg}");
        }
    }
}
