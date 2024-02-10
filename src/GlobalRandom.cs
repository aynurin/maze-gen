

using System;
using System.Collections.Generic;
using System.Linq;

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

        [Obsolete("Use Random() instead.")]
        public static T GetRandom<T>(this IList<T> items) => items.Random();

        public static T Random<T>(this IList<T> items) =>
            items.Count == 0 ?
                throw new InvalidOperationException(
                    "Cannot get a random item from an empty list") :
                items[s_random.Next(items.Count)];

        [Obsolete("Use Random(int) instead.")]
        public static T GetRandom<T>(this IEnumerable<T> items, int count) =>
            items.Random(count);

        public static T Random<T>(this IEnumerable<T> items, int count) =>
            count == 0 ?
                throw new InvalidOperationException(
                    "Cannot get a random item from an empty list") :
                items.ElementAt(s_random.Next(count));

        [Obsolete("Use Random() instead.")]
        public static T GetRandom<T>(this IEnumerable<T> items) =>
            items.Random();

        public static T Random<T>(this IEnumerable<T> items) {
            var list = new List<T>(items);
            if (list.Count == 0) {
                throw new InvalidOperationException(
                    "Cannot get a random item from an empty list");
            }
            return list.Random();
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