using System;
using System.Collections;
using System.Collections.Generic;

namespace Nour.Play.Areas {
    internal class RandomAreaGenerator : IEnumerable<MapArea>, IEnumerator<MapArea> {
        private MapArea _current;

        public GeneratorSettings Settings { get; set; }
        public MapArea Current => _current;
        object IEnumerator.Current => _current;

        public RandomAreaGenerator(GeneratorSettings settings) {
            Settings = settings;
        }
        public IEnumerator<MapArea> GetEnumerator() {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Dispose() { } // noop

        public bool MoveNext() {
            _current = GetRandomZone();
            return true;
        }

        public void Reset() { } // noop

        private MapArea GetRandomZone() => RandomRotate(new MapArea(
                PickRandom(Settings.AreaTypeProbabilities),
                PickRandom(Settings.DimensionProbabilities),
                PickRandom(Settings.TagProbabilities)
            ));

        private static T PickRandom<T>(IDictionary<T, float> distribution) {
            var random = GlobalRandom.RandomSingle();
            var cumulativeProb = 0f;
            T lastItem = default;
            foreach (var couple in distribution) {
                cumulativeProb += couple.Value;
                if (random < cumulativeProb) {
                    return couple.Key;
                }
                lastItem = couple.Key;
            }
            return lastItem;
        }

        public static MapArea RandomRotate(MapArea area) {
            if (GlobalRandom.Next() % 2 == 0)
                return area;
            return new MapArea(area.Type, area.Size, area.Tags);
        }

        public class GeneratorSettings {
            public Dictionary<Vector, float> DimensionProbabilities { get; set; } = DEFAULT_DIMENSIONS;
            public Dictionary<AreaType, float> AreaTypeProbabilities { get; set; } = DEFAULT_AREA_TYPES;
            public Dictionary<String, float> TagProbabilities { get; set; } = DEFAULT_TAGS;

            public GeneratorSettings(
                Dictionary<Vector, float> dimensionProbabilities,
                Dictionary<AreaType, float> areaTypeProbabilities,
                Dictionary<String, float> tagProbabilities) {
                if (dimensionProbabilities != null) {
                    DimensionProbabilities = dimensionProbabilities;
                }
                if (areaTypeProbabilities != null) {
                    AreaTypeProbabilities = areaTypeProbabilities;
                }
                if (tagProbabilities != null) {
                    TagProbabilities = tagProbabilities;
                }
            }

            public static GeneratorSettings Default => new GeneratorSettings(null, null, null);
            // areas will be added based on probability, e.g.:
            // 2x2 : 60% (means 60% of mazes will have a area of 2 rows x 2 columns)
            // 2x3 : 60%
            // 3x4 : 40%
            // 3x6 : 40%
            // 4x8 : 20%
            // 5x6 : 20%
            // 7x7 : 5%
            private static readonly Dictionary<Vector, float> DEFAULT_DIMENSIONS = new Dictionary<Vector, float>() {
                { new Vector(2, 2), 0.25f },
                { new Vector(2, 3), 0.25f },
                { new Vector(3, 4), 0.15f },
                { new Vector(3, 6), 0.15f },
                { new Vector(4, 8), 0.075f },
                { new Vector(5, 6), 0.075f },
                { new Vector(7, 7), 0.005f }
            };

            private static readonly Dictionary<AreaType, float> DEFAULT_AREA_TYPES = new Dictionary<AreaType, float>() {
                { AreaType.Fill, 0.7f },
                { AreaType.Hall, 0.3f }
            };


            private static readonly Dictionary<String, float> DEFAULT_TAGS = new Dictionary<String, float>() {
                { "room", 0.5f },
                { "lake", 0.15f },
                { "dirt", 0.15f },
                { "swamp", 0.15f },
                { "void", 0.05f },
            };
        }
    }
}
// MazeZone.GetRandomZone()