
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze.PostProcessing;
using static PlayersWorlds.Maps.Maze.GeneratorOptions;

namespace PlayersWorlds.Maps.Maze {
    internal static class MazeTestHelper {
        private static readonly Log s_log = Log.ToConsole("MazeTestHelper");

        public static bool IsSolveable(Maze2D maze) {
            var cells = new HashSet<MazeCell>(maze.MazeCells);
            var dijkstra = DijkstraDistance.Find(cells.First());
            cells.ExceptWith(dijkstra.Keys);

            if (cells.Count > 0) {
                s_log.I(
                    $"No solution for this maze between {cells.First()} and " +
                    $"{string.Join(",", cells)}:\n" + maze.ToString());
                return false;
            }
            return true;
        }

        public static Maze2D GenerateMaze(
                                   Vector size,
                                   GeneratorOptions options,
                                   out Maze2DBuilder builder) {
            if (options.RandomSource == null) {
                options.RandomSource = RandomSource.CreateFromEnv();
            }
            if (options.AreaGenerator == null) {
                options.AreaGenerator = new RandomAreaGenerator(
                    new RandomAreaGenerator.RandomAreaGeneratorSettings(
                        options.RandomSource));
            }
            var maze = MazeGenerator.Generate(size,
                options, out builder);
            s_log.D(2, maze.ToString());
            if (options.MapAreas != null) {
                Assert.That(maze.MapAreas.Count,
                    Is.GreaterThanOrEqualTo(options.MapAreas.Count),
                    "Wrong number of areas");
            }
            return maze;
        }

        public static Maze2D GenerateMaze(
                                   Vector size,
                                   GeneratorOptions options) {
            return GenerateMaze(size, options, out _);
        }

        public static bool IsSupported(
            Type generatorType,
            FillFactorOption fillFactor) {
            if ((generatorType == typeof(SidewinderMazeGenerator)
                 || generatorType == typeof(BinaryTreeMazeGenerator))
                && fillFactor != GeneratorOptions.FillFactorOption.Full) {
                return false;
            }
            return true;
        }

        public static IEnumerable<Type> GetAllGenerators() {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm.GetName().Name == "PlayersWorlds.Maps")
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(MazeGenerator) != p &&
                            typeof(MazeGenerator).IsAssignableFrom(p))
                .Where(p =>
                    !TestContext.Parameters.Exists("MAZEGEN") ||
                    p.Name == TestContext.Parameters["MAZEGEN"]);
        }

        public static IEnumerable<AreaType> GetAllAreaTypes() {
            if (!TestContext.Parameters.Exists("MAZEGEN_AREATYPE")) {
                yield return AreaType.Cave;
                yield return AreaType.Hall;
                yield return AreaType.Fill;
            } else {
                yield return (AreaType)
                    Enum.Parse(typeof(AreaType),
                    TestContext.Parameters["MAZEGEN_AREATYPE"]);
            }
        }

        public static IEnumerable<FillFactorOption> GetAllFillFactors() {
            if (!TestContext.Parameters.Exists("MAZEGEN_FILLFACTOR")) {
                yield return FillFactorOption.Quarter;
                yield return FillFactorOption.Half;
                yield return FillFactorOption.ThreeQuarters;
                yield return FillFactorOption.NinetyPercent;
                yield return FillFactorOption.FullWidth;
                yield return FillFactorOption.FullHeight;
                yield return FillFactorOption.Full;
            } else {
                yield return (FillFactorOption)
                    Enum.Parse(typeof(FillFactorOption),
                    TestContext.Parameters["MAZEGEN_FILLFACTOR"]);
            }
        }
    }
}