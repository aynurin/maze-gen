

using System;
using System.Collections.Generic;

namespace Nour.Play {
    internal static class GlobalRandom {
        private static Random _random = new Random();
        public static int Next() => _random.Next();
        public static int Next(int maxValue) => _random.Next(maxValue);
        public static int Next(int minValue, int maxValue) =>
            _random.Next(minValue, maxValue);
        public static void NextBytes(byte[] buffer) =>
            _random.NextBytes(buffer);
        public static byte[] NextBytes(int count) {
            var rndBytes = new byte[count];
            var rnd = System.Security.Cryptography.RNGCryptoServiceProvider.Create();
            rnd.GetBytes(rndBytes);
            return rndBytes;
        }
        public static double NextDouble() => _random.NextDouble();
        public static T GetRandom<T>(this IList<T> items) =>
            items[_random.Next(items.Count)];
        public static float RandomSingle() => _random.Next(99999) / 100000f;
    }
}