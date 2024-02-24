
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;
using static PlayersWorlds.Maps.Maze.GeneratorOptions;

namespace PlayersWorlds.Maps.Maze {
    internal class MazeTestHelper {
        public static Maze2D GenerateMaze(Vector size,
                                          Type generatorType,
                                          List<MapArea> areas,
                                          FillFactorOption fillFactor,
                                          out Maze2DBuilder builder) {
            var maze = GenerateMaze(size,
                  new GeneratorOptions() {
                      FillFactor = fillFactor,
                      Algorithm = generatorType,
                      MapAreasOptions = MapAreaOptions.Manual,
                      MapAreas = areas
                  }, out builder);
            Log.Create(new StackTrace().GetFrame(1).GetMethod().Name
                                .Split('_').Last()).D(5, maze.ToString());
            Assert.That(maze.MapAreas.Count, Is.EqualTo(areas.Count),
                "Wrong number of areas");
            return maze;
        }

        public static Maze2D GenerateMaze(
                                   Vector size,
                                   GeneratorOptions options,
                                   out Maze2DBuilder builder) {
            var maze = MazeGenerator.Generate(size,
                options, out builder);
            Log.Create(
                new StackTrace().GetFrame(1).GetMethod().Name
                    .Split('_').Last())
                    .D(5, maze.ToString());
            return maze;
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
            var debugGenType = TestContext.Parameters.Exists("MAZEGEN") ?
                TestContext.Parameters["MAZEGEN"] : null;
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm.GetName().Name == "PlayersWorlds.Maps")
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(MazeGenerator) != p &&
                            typeof(MazeGenerator).IsAssignableFrom(p))
                .Where(p => debugGenType == null || p.Name == debugGenType);
        }

        public static IEnumerable<AreaType> GetAllAreaTypes() {
            var debugAreaType =
                TestContext.Parameters.Exists("MAZEGEN_AREATYPE") ?
                TestContext.Parameters["MAZEGEN_AREATYPE"] : null;
            if (debugAreaType == null) {
                yield return AreaType.Cave;
                yield return AreaType.Hall;
                yield return AreaType.Fill;
            } else {
                yield return (AreaType)
                    Enum.Parse(typeof(AreaType), debugAreaType);
            }
        }

        public static IEnumerable<FillFactorOption> GetAllFillFactors() {
            var debugFillFactor =
                TestContext.Parameters.Exists("MAZEGEN_FILLFACTOR") ?
                TestContext.Parameters["MAZEGEN_FILLFACTOR"] : null;
            if (debugFillFactor == null) {
                yield return FillFactorOption.Quarter;
                yield return FillFactorOption.Half;
                yield return FillFactorOption.ThreeQuarters;
                yield return FillFactorOption.NinetyPercent;
                yield return FillFactorOption.FullWidth;
                yield return FillFactorOption.FullHeight;
                yield return FillFactorOption.Full;
            } else {
                yield return (FillFactorOption)
                    Enum.Parse(typeof(FillFactorOption), debugFillFactor);
            }
        }
    }
}