using System;
using System.Diagnostics;
using System.Threading;

namespace ConsoleApp.Instrumentation
{
    internal static class StateTrace
    {
        private static readonly Stopwatch MonoClock = Stopwatch.StartNew();
        private static long Sequence = 0;
        private static readonly long StartEpochTicks = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static void Log(string eventName, string details = null, int? rc = null)
        {
            long seq = Interlocked.Increment(ref Sequence);

            long monoUs =
                (long)(MonoClock.ElapsedTicks * (1_000_000.0 / Stopwatch.Frequency));

            long epochMs = StartEpochTicks + MonoClock.ElapsedMilliseconds;

            int tid = Environment.CurrentManagedThreadId;

            if (details == null && rc == null)
            {
                Console.WriteLine(
                    $"[{seq:D6}] [EPOCH {epochMs}] [+{monoUs}us] [T{tid}] {eventName}()");
            }
            else if (rc == null)
            {
                Console.WriteLine(
                    $"[{seq:D6}] [EPOCH {epochMs}] [+{monoUs}us] [T{tid}] {eventName}({details})");
            }
            else
            {
                Console.WriteLine(
                    $"[{seq:D6}] [EPOCH {epochMs}] [+{monoUs}us] [T{tid}] {eventName}({details}) -> rc={rc}");
            }
        }
    }
}
