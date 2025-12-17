using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace SocketClient
{
    internal class ShadowClient
    {
        private const string HOST = "auth.deltaworlds.com";
        private const int PORT = 6671;

        static void Main(string[] args)
        {
            Console.WriteLine("[shadow] starting");

            var sw = Stopwatch.StartNew();

            using var client = new TcpClient();
            client.NoDelay = true;

            Console.WriteLine("[shadow] connecting...");
            client.Connect(HOST, PORT);

            Console.WriteLine($"[shadow] connected @ {sw.ElapsedMilliseconds}ms");

            using var stream = client.GetStream();

            // small delay to mirror harness behavior
            Thread.Sleep(TimingProfile.AfterConnectDelay);

            Console.WriteLine("[shadow] entering passive observe mode");

            // -------------------------------------------------
            // PHASE 2: minimal client probe
            // -------------------------------------------------

            Thread.Sleep(TimingProfile.BeforeLoginDelay);

            // Minimal non-zero probe frame
            byte[] probe = new byte[] { 0x00, 0x00, 0x00, 0x01 };

            try
            {
                stream.Write(probe, 0, probe.Length);
                stream.Flush();
                Console.WriteLine("[shadow] sent probe frame (4 bytes)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[shadow] write exception: {ex.Message}");
                return;
            }

            // -------------------------------------------------
            // PHASE 3: safe receive loop
            // -------------------------------------------------

            var buffer = new byte[8192];
            stream.ReadTimeout = 2000;

            while (true)
            {
                if (!client.Connected)
                {
                    Console.WriteLine("[shadow] socket disconnected");
                    break;
                }

                if (!stream.DataAvailable)
                {
                    Thread.Sleep(10);
                    continue;
                }

                int read;
                try
                {
                    read = stream.Read(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[shadow] read exception: {ex.GetType().Name} {ex.Message}");
                    break;
                }

                if (read == 0)
                {
                    Console.WriteLine("[shadow] server closed connection");
                    break;
                }

                HexDump.Dump(buffer, read, "[RX]");
            }

            Console.WriteLine("[shadow] exiting");
        }
    }
}
