using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps {
    internal class RandomSource {
        private Random _random;

        public int Seed { get; private set; }

        public RandomSource() {
            Seed = DateTime.Now.Millisecond;
            _random = new Random(Seed);
        }

        public RandomSource(int seed) {
            Seed = seed;
            _random = new Random(Seed);
        }

        public int Next() => _random.Next();

        public int Next(int min, int max) => _random.Next(min, max);

        public double Next(double min, double max,
                                  double precision = 100) =>
            (double)(
                Next((int)(min * precision), (int)(max * precision))
                    / precision
            );

        public float RandomSingle() => _random.Next(99999) / 100000f;

        public byte[] NextBytes(int count) {
            var rndBytes = new byte[count];
            var rnd = System.Security.Cryptography.RNGCryptoServiceProvider.Create();
            rnd.GetBytes(rndBytes);
            return rndBytes;
        }

        public T RandomOf<T>(IList<T> items) =>
            items.Count == 0 ?
                throw new InvalidOperationException(
                    "Cannot get a random item from an empty list") :
                items[_random.Next(items.Count)];

        public T RandomOf<T>(IEnumerable<T> items, int count) =>
            count == 0 ?
                throw new InvalidOperationException(
                    "Cannot get a random item from an empty list") :
                items.ElementAt(_random.Next(count));

        public T RandomOrDefaultOf<T>(IEnumerable<T> items) {
            var list = new List<T>(items);
            if (list.Count == 0) {
                throw new InvalidOperationException(
                    "Cannot get a random item from an empty list");
            }
            return list.Random();
        }

    }
}