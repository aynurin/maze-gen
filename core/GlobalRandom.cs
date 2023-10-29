

using System;
using System.Collections.Generic;

namespace Nour.Play {
    internal static class GlobalRandom {
        private static Random _random = new Random();
        public static int Next() => _random.Next();
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
                items[_random.Next(items.Count)];

        public static float RandomSingle() => _random.Next(99999) / 100000f;
    }
}