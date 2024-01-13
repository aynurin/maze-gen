using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Areas {
    /// <summary>
    /// Generate areas randomly for a map of the provided size.
    /// </summary>
    public class RandomAreaGenerator : AreaGenerator {
        private readonly RandomAreaGeneratorSettings _settings;

        /// <summary>
        /// Creates a new random area generator with the specified settings.
        /// </summary>
        /// <param name="settings">Settings to control the generator behavior.
        /// </param>
        public RandomAreaGenerator(RandomAreaGeneratorSettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Randomly generates areas for a map of the specified size.
        /// </summary>
        /// <param name="size">The size of the map.</param>
        /// <returns>Areas to be added to the map.</returns>
        public override IEnumerable<MapArea> Generate(Vector size) {
            var areas = new List<MapArea>();
            var addedArea = 0;
            if (_settings.DimensionProbabilities.Keys.All(areaSize =>
                !areaSize.Fits(size - new Vector(2, 2)))) {
                // none of the areas fit the map
                return areas;
            }
            do {
                var area = GetRandomZone();
                if (!area.Size.Fits(size - new Vector(2, 2)))
                    continue;
                if (addedArea + area.Size.Area >
                    size.Area * Math.Min(1, _settings.MinFillFactor * 2))
                    continue;
                areas.Add(area);
                addedArea += area.Size.Area;
            } while (addedArea < size.Area * _settings.MinFillFactor);
            return areas;
        }

        // for testing
        internal IEnumerable<MapArea> Generate(int count) {
            for (var i = 0; i < count; i++) {
                yield return GetRandomZone();
            }
        }

        private MapArea GetRandomZone() => RandomRotate(new MapArea(
                PickRandom(_settings.AreaTypeProbabilities),
                PickRandom(_settings.DimensionProbabilities),
                new Vector(0, 0),
                PickRandom(_settings.TagProbabilities)
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

        private static MapArea RandomRotate(MapArea area) {
            if (GlobalRandom.Next() % 2 == 0)
                return area;
            return new MapArea(area.Type, area.Size, area.Position, area.Tags);
        }

        /// <summary />
        public class RandomAreaGeneratorSettings {
            internal float MinFillFactor { get; }

            internal Dictionary<Vector, float> DimensionProbabilities { get; }

            internal Dictionary<AreaType, float> AreaTypeProbabilities { get; }

            internal Dictionary<string, float> TagProbabilities { get; }

            /// <summary>
            /// Creates an instance of RandomAreaGeneratorSettings. Default
            /// values provide some basic settings.
            /// </summary>
            /// <param name="minFillFactor"></param>
            /// <param name="dimensionProbabilities"></param>
            /// <param name="areaTypeProbabilities"></param>
            /// <param name="tagProbabilities"></param>
            public RandomAreaGeneratorSettings(
                float minFillFactor = 0.3f,
                Dictionary<Vector, float> dimensionProbabilities = null,
                Dictionary<AreaType, float> areaTypeProbabilities = null,
                Dictionary<string, float> tagProbabilities = null) {
                MinFillFactor = minFillFactor;
                DimensionProbabilities = dimensionProbabilities ?? s_default_dimensions;
                AreaTypeProbabilities = areaTypeProbabilities ?? s_default_area_types;
                TagProbabilities = tagProbabilities ?? s_default_tags;
            }

            /// <summary />
            public static RandomAreaGeneratorSettings Default =>
                new RandomAreaGeneratorSettings();

            private static readonly Dictionary<Vector, float> s_default_dimensions = new Dictionary<Vector, float>() {
                { new Vector(2, 2), 0.25f },
                { new Vector(2, 3), 0.25f },
                { new Vector(3, 4), 0.15f },
                { new Vector(3, 6), 0.15f },
                { new Vector(4, 8), 0.075f },
                { new Vector(5, 6), 0.075f },
                { new Vector(7, 7), 0.005f }
            };

            private static readonly Dictionary<AreaType, float> s_default_area_types = new Dictionary<AreaType, float>() {
                { AreaType.Fill, 0.7f },
                { AreaType.Hall, 0.3f }
            };

            private static readonly Dictionary<string, float> s_default_tags = new Dictionary<string, float>() {
                { "room", 0.5f },
                { "lake", 0.15f },
                { "dirt", 0.15f },
                { "swamp", 0.15f },
                { "void", 0.05f },
            };
        }
    }
}