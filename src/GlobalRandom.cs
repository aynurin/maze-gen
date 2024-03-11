

using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps {
    public static class GlobalRandom {
        private static Random s_random;
        private static int s_seed;

        public static int Seed { get => s_seed; }

        static GlobalRandom() {
            s_seed = DateTime.Now.Millisecond;
            s_random = new Random(s_seed);
        }

        public static void Reseed(int seed) {
            s_seed = seed;
            s_random = new Random(s_seed);
        }

        public static int Next() => s_random.Next();

        public static int Next(int min, int max) => s_random.Next(min, max);

        public static double Next(double min, double max,
                                  double precision = 100) =>
            (double)(
                Next((int)(min * precision), (int)(max * precision))
                    / precision
            );

        public static byte[] NextBytes(int count) {
            var rndBytes = new byte[count];
            var rnd = System.Security.Cryptography.RNGCryptoServiceProvider.Create();
            rnd.GetBytes(rndBytes);
            return rndBytes;
        }

        public static T Random<T>(this IList<T> items) =>
            items.Count == 0 ?
                throw new InvalidOperationException(
                    "Cannot get a random item from an empty list") :
                items[s_random.Next(items.Count)];

        public static T Random<T>(this IEnumerable<T> items, int count) =>
            count == 0 ?
                throw new InvalidOperationException(
                    "Cannot get a random item from an empty list") :
                items.ElementAt(s_random.Next(count));

        [Obsolete("See if you can use the Random<T>(this IEnumerable<T> " +
                  "items, int count) overload.")]
        public static T Random<T>(this IEnumerable<T> items) {
            throw new InvalidOperationException(
                "See if you can use the Random<T>(this IEnumerable<T> items, " +
                "int count) overload.");
        }

        public static T RandomOrDefault<T>(this IEnumerable<T> items) {
            var list = new List<T>(items);
            if (list.Count == 0) {
                throw new InvalidOperationException(
                    "Cannot get a random item from an empty list");
            }
            return list.Random();
        }

        public static float RandomSingle() => s_random.Next(99999) / 100000f;
    }
}