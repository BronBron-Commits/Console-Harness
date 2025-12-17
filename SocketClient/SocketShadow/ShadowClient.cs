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

        static void Main(string[] args)
        {
            Log("starting");

            Directory.CreateDirectory("captures");

            var stopwatch = Stopwatch.StartNew();

            using var client = new TcpClient
            {
                NoDelay = true
            };

            // -----------------------------------------------------
            // PHASE 1: TCP CONNECT
            // -----------------------------------------------------

            Log("connecting...");
            client.Connect(HOST, PORT);
            Log($"connected @ {stopwatch.ElapsedMilliseconds}ms");

            using var stream = client.GetStream();
            stream.ReadTimeout = 5000;
            stream.WriteTimeout = 5000;

            Thread.Sleep(TimingProfile.AfterConnectDelay);

            // -----------------------------------------------------
            // PHASE 2: REAL CLIENT HELLO (VERIFIED)
            // -----------------------------------------------------

            byte[] clientHello =
            {
                0x00, 0x0A,
                0x00, 0x02,
                0x00, 0x24,
                0x00, 0x03,
                0x00, 0x00
            };

            Log("sending client hello");
            SendFrame(stream, clientHello, "client hello");

            // Small observed delay before server response
            Thread.Sleep(TimingProfile.BeforeLoginDelay);

            // -----------------------------------------------------
            // PHASE 3: RECEIVE SERVER ENVELOPE
            // -----------------------------------------------------

            var buffer = new byte[8192];
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

            Log($"received server envelope ({read} bytes)");

            byte[] envelope = buffer.Take(read).ToArray();

            // Save raw envelope
            string path = Path.Combine("captures", "server-envelope.bin");
            File.WriteAllBytes(path, envelope);
            Log($"saved raw envelope to {path}");

            // Hex dump (sanity)
            HexDump.Dump(envelope, envelope.Length, "[RX]");

            // -----------------------------------------------------
            // PHASE 4: DECODE SERVER ENVELOPE
            // -----------------------------------------------------

            AuthEnvelopeDecoder.Decode(envelope);

            Log("decode complete, exiting");
        }

        // =========================================================
        // Helpers
        // =========================================================

        private static void SendFrame(NetworkStream stream, byte[] frame, string label)
        {
            try
            {
                stream.Write(frame, 0, frame.Length);
                stream.Flush();
                Log($"sent {label} ({frame.Length} bytes)");
            }
            catch (Exception ex)
            {
                Log($"WRITE ERROR [{label}]: {ex.GetType().Name} {ex.Message}");
                throw;
            }
        }

        private static void Log(string message)
        {
            Console.WriteLine($"[shadow] {message}");
        }
    }
}
