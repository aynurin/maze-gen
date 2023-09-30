using System;
using System.Collections;
using System.Collections.Generic;

namespace Nour.Play.Maze {
    internal class RandomZoneGenerator : IEnumerable<MazeZone> {
        public IEnumerator<MazeZone> GetEnumerator() {
            return new ZoneEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        internal class ZoneEnumerator : IEnumerator<MazeZone> {
            private MazeZone _current;
            public MazeZone Current => _current;

            object IEnumerator.Current => _current;

            public void Dispose() { } // noop

            public bool MoveNext() {
                _current = GetRandomZone();
                return true;
            }

            public void Reset() { } // noop

            // zones will be added based on probability, e.g.:
            // 2x2 : 60% (means 60% of mazes will have a zone of 2 rows x 2 columns)
            // 2x3 : 60%
            // 3x4 : 40%
            // 3x6 : 40%
            // 4x8 : 20%
            // 5x6 : 20%
            // 7x7 : 5%
            private Dictionary<Size, float> _dimensionsProbabilities = new Dictionary<Size, float>() {
                { new Size(2, 2), 0.25f },
                { new Size(2, 3), 0.25f },
                { new Size(3, 4), 0.15f },
                { new Size(3, 6), 0.15f },
                { new Size(4, 8), 0.075f },
                { new Size(5, 6), 0.075f },
                { new Size(7, 7), 0.005f }
            };

            private Dictionary<MazeZone.ZoneType, float> _zoneTypeProbabilities = new Dictionary<MazeZone.ZoneType, float>() {
                { MazeZone.ZoneType.Fill, 0.7f },
                { MazeZone.ZoneType.Hall, 0.3f }
            };


            private Dictionary<String, float> _tagsProbabilities = new Dictionary<String, float>() {
                { "room", 0.5f },
                { "lake", 0.15f },
                { "dirt", 0.15f },
                { "swamp", 0.15f },
                { "void", 0.05f }
            };

            // 2.45 = 1
            // 0.6 = x

            private MazeZone GetRandomZone() => new MazeZone(
                    PickRandom(_zoneTypeProbabilities),
                    PickRandom(_dimensionsProbabilities),
                    PickRandom(_tagsProbabilities)
                ).RandomRotate();

            private static T PickRandom<T>(IDictionary<T, float> distribution) {
                var random = RandomExtensions.RandomSingle();
                var cumulativeProb = 0f;
                T lastItem = default(T);
                foreach (var couple in distribution) {
                    cumulativeProb += couple.Value;
                    if (random < cumulativeProb) {
                        return couple.Key;
                    }
                    lastItem = couple.Key;
                }
                return lastItem;
            }
        }
    }
}
// MazeZone.GetRandomZone()