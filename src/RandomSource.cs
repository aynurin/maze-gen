using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PlayersWorlds.Maps {
    public class RandomSource {
        private static int _instanceCount = 0;
        private Log _log = Log.ToConsole<RandomSource>();
        private readonly Random _random;
        private readonly int _instanceId;

        public int Seed { get; private set; }
        public static int? EnvRandomSeed { get; set; }

        private RandomSource(int seed) {
            _instanceId = Interlocked.Increment(ref _instanceCount);
            Seed = seed;
            _random = new Random(Seed);
        }

        public static RandomSource CreateFromEnv() {
            if (EnvRandomSeed.HasValue)
                return new RandomSource(EnvRandomSeed.Value);
            else return new RandomSource(DateTime.Now.Millisecond);
        }

        private Random D(string refName) {
            _log.D(4, $"[{_instanceId}] RandomSource.{refName}");
            return _random;
        }

        public int Next() {
            return D("Next()").Next();
        }

        public int Next(int min, int max) {
            return D("Next(int,int)").Next(min, max);
        }

        public double Next(double min, double max,
                                  double precision = 100) {
            return (double)(
                Next((int)(min * precision), (int)(max * precision))
                    / precision
            );
        }

        public float RandomSingle() {
            return D("RandomSingle()").Next(99999) / 100000f;
        }

        public byte[] NextBytes(int count) {
            var rndBytes = new byte[count];
            D("NextBytes(int)").NextBytes(rndBytes);
            return rndBytes;
        }

        public T RandomOf<T>(IList<T> items) {
            return items.Count == 0 ?
                throw new InvalidOperationException(
                    "Cannot get a random item from an empty list") :
                items[D("RandomOf<T>(IList<T>)").Next(items.Count)];
        }

        public T RandomOrDefaultOf<T>(IList<T> items) {
            if (items.Count == 0) {
                return default;
            }
            return RandomOf(items);
        }

        public T RandomOf<T>(IEnumerable<T> items, int count) {
            return count == 0 ?
                throw new InvalidOperationException(
                    "Cannot get a random item from an empty list") :
                items.ElementAt(D("RandomOf<T>(IEnumerable<T>,int)").Next(count));
        }

        public T RandomOrDefaultOf<T>(IEnumerable<T> items, int count) {
            return count == 0 ? default : items.ElementAt(D("RandomOrDefaultOf(IEnumerable<T>,int)").Next(count));
        }
    }
}