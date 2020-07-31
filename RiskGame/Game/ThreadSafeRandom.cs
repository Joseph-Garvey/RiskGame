using System;
using System.Threading;

namespace RiskGame
{
    /// <summary>
    /// C# Thread-Safe randomisation.
    /// Generates random numbers from a seed.
    /// </summary>
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }
}
