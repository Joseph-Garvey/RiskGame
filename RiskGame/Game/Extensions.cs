using System.Collections.Generic;

namespace RiskGame
{
    // Class Extensions for threaded randomization of territory order //
    public static class Extensions
    {
        // Fisher Yates Shuffle //
        public static void Shuffle<T>(this IList<T> list) // Shuffle the list at random.
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
