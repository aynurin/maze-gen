using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Areas {
    /// <summary>
    /// Generate areas randomly for a map of the provided size.
    /// </summary>
    public class RandomAreaGenerator : AreaGenerator {
        private readonly Log _log = Log.ToConsole<RandomAreaGenerator>();
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
        /// <param name="existingAreas">Pre-existing areas.</param>
        /// <returns>Areas to be added to the map.</returns>
        public override IEnumerable<Area> Generate(
            Vector size,
            List<Area> existingAreas) {
            if (_settings.DimensionProbabilities.Keys.All(areaSize =>
                !areaSize.FitsInto(size - new Vector(2, 2)))) {
                // none of the areas fit the map
                return new List<Area>();
            }
            var addedArea = existingAreas?.Sum(area => area.Size.Area) ?? 0;
            var areas = new List<Area>();
            while (addedArea < size.Area * _settings.MinFillFactor) {
                var area = GetRandomZone();
                if (!area.Size.FitsInto(size - new Vector(2, 2)))
                    continue;
                if (addedArea + area.Size.Area >
                    size.Area * Math.Min(1, _settings.MinFillFactor * 2))
                    continue;
                areas.Add(area);
                addedArea += area.Size.Area;
            }
            return areas;
        }

        // for testing
        internal IEnumerable<Area> Generate(int count) {
            for (var i = 0; i < count; i++) {
                yield return GetRandomZone();
            }
        }

        private Area GetRandomZone() {
            var type = PickRandom(_settings.AreaTypeProbabilities);
            var size = RandomRotate(PickRandom(_settings.DimensionProbabilities));
            var tags = PickRandom(_settings.TagProbabilities[type]);
            return Area.CreateUnpositioned(size, type, tags);
        }

        private T PickRandom<T>(IDictionary<T, float> distribution) {
            var random = _settings.RandomSource.RandomSingle();
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

        private Vector RandomRotate(Vector size) {
            if (_settings.RandomSource.Next() % 2 == 0)
                return size;
            return new Vector(size.Y, size.X);
        }

        /// <summary />
        public class RandomAreaGeneratorSettings {
            internal float MinFillFactor { get; }

            internal Dictionary<Vector, float> DimensionProbabilities { get; }

            internal Dictionary<AreaType, float> AreaTypeProbabilities { get; }

            internal Dictionary<AreaType, Dictionary<string, float>> TagProbabilities { get; }

            internal RandomSource RandomSource { get; set; }

            /// <summary>
            /// Creates an instance of RandomAreaGeneratorSettings. Default
            /// values provide some basic settings.
            /// </summary>
            /// <param name="randomSource"></param>
            /// <param name="minFillFactor"></param>
            /// <param name="dimensionProbabilities"></param>
            /// <param name="areaTypeProbabilities"></param>
            /// <param name="tagProbabilities"></param>
            public RandomAreaGeneratorSettings(
                RandomSource randomSource,
                float minFillFactor = 0.3f,
                Dictionary<Vector, float> dimensionProbabilities = null,
                Dictionary<AreaType, float> areaTypeProbabilities = null,
                Dictionary<AreaType, Dictionary<string, float>> tagProbabilities = null) {
                RandomSource = randomSource;
                MinFillFactor = minFillFactor;
                DimensionProbabilities = dimensionProbabilities ?? s_default_dimensions;
                AreaTypeProbabilities = areaTypeProbabilities ?? s_default_area_types;
                TagProbabilities = tagProbabilities ?? s_default_tags;
            }

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
                { AreaType.Fill, 0.2f },
                { AreaType.Cave, 0.4f },
                { AreaType.Hall, 0.4f }
            };

            // TODO: Make tags dependent on the area type
            private static readonly Dictionary<AreaType, Dictionary<string, float>> s_default_tags = new Dictionary<AreaType, Dictionary<string, float>>() {
                { AreaType.Fill,
                    new Dictionary<string, float> {
                        { "ruins", 0.2f },
                        { "lake", 0.2f },
                        { "dirt", 0.2f },
                        { "swamp", 0.2f },
                        { "void", 0.2f },
                    }
                },
                { AreaType.Cave,
                    new Dictionary<string, float> {
                        { "ruins", 0.3f },
                        { "den", 0.2f },
                        { "cave", 0.5f },
                    }
                },
                { AreaType.Hall,
                    new Dictionary<string, float> {
                        { "room", 0.5f },
                        { "loot", 0.5f },
                    }
                },
            };
        }
    }
}