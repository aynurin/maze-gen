

using System;
using System.Collections.Generic;

namespace PlayersWorlds.Maps {
    internal static class GlobalRandom {
        private static readonly Random s_random = new Random();
        public static int Next() => s_random.Next();
        public static int Next(int min, int max) => s_random.Next(min, max);
        public static byte[] NextBytes(int count) {
            var rndBytes = new byte[count];
            var rnd = System.Security.Cryptography.RNGCryptoServiceProvider.Create();
            rnd.GetBytes(rndBytes);
            return rndBytes;
        }
        public static T GetRandom<T>(this IList<T> items) =>
            items.Count == 0 ?
                throw new InvalidOperationException(
                    "Cannot get a random item from an empty list") :
                items[s_random.Next(items.Count)];

        public static float RandomSingle() => s_random.Next(99999) / 100000f;
    }
}