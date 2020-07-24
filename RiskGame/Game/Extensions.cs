using System.Collections.Generic;

namespace RiskGame
{
    // Class Extensions for threaded randomisation of territory order //
    public static class Extensions
    {
        // Fisher Yates Shuffle //
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int x = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[x];
                list[x] = list[n];
                list[n] = value;
            }
        }

    }
}
