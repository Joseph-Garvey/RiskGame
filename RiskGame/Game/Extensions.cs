using System.Collections.Generic;

namespace RiskGame
{
    /// <summary>
    /// Class Extensions for threaded randomization of territory order
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Fisher Yates Shuffle. Shuffles list at random.
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1) // shuffle each number in list
            {
                n--;
                int x = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1); // generate a random position
                T value = list[x];
                list[x] = list[n]; // swap values in list
                list[n] = value;
            }
        }

    }
}
