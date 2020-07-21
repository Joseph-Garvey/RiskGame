using System;
using System.Threading;

namespace RiskGame
{
    public static class ThreadSafeRandom
    { // C# randomisation threading library
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }
}
