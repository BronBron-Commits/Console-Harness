using System;

namespace SocketClient
{
    public static class TimingProfile
    {
        // Measured from harness logs (microseconds → milliseconds)
        public static readonly TimeSpan AfterConnectDelay   = TimeSpan.FromMilliseconds(5);
        public static readonly TimeSpan BeforeLoginDelay    = TimeSpan.FromMilliseconds(1);
        public static readonly TimeSpan AfterLoginDelay     = TimeSpan.FromMilliseconds(120);
        public static readonly TimeSpan AfterEnterDelay     = TimeSpan.FromMilliseconds(750);
        public static readonly TimeSpan HeartbeatInterval   = TimeSpan.FromMilliseconds(100);
    }
}
