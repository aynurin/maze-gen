using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze.PostProcessing;
using static PlayersWorlds.Maps.Maze.PostProcessing.DijkstraDistance;

namespace PlayersWorlds.Maps.Maze {
    [TestFixture]
    public class MazeGeneratorTest : Test {

        private class CustomAreaGenerator : AreaGenerator {
            public override IEnumerable<Area> Generate(
                Vector size, List<Area> existingAreas) {
                if (size.X <= 10 || size.Y <= 10) {
                    return Enumerable.Empty<Area>();
                }
                return new[] {
                    Area.Create(
                        new Vector(0, 0), new Vector(2, 3), AreaType.Hall)
                };
            }
        }

        [Test]
        public void CanUseCustomAreaGenerator() {
            var log = Log.ToConsole("MazeGeneratorTest.CanUseCustomAreaGenerator");
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(15, 15),
                new GeneratorOptions() {
                    AreaGenerator = new CustomAreaGenerator(),
                    FillFactor = GeneratorOptions.FillFactorOption.Full,
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    MapAreasOptions = GeneratorOptions.MapAreaOptions.Auto
                });
            log.D(5, maze.ToString());
            Assert.That(maze.ChildAreas, Has.Count.EqualTo(1));
            Assert.That(maze.ChildAreas.First().Size,
                Is.EqualTo(new Vector(2, 3)));
            Assert.That(maze.ChildAreas.First().Position,
                Is.EqualTo(new Vector(0, 0)));
            Assert.That(maze.ChildAreas.First().Type,
                Is.EqualTo(AreaType.Hall));
        }

        [Test]
        public void CanGenerateMazes(
            [ValueSource("GetAllGenerators")] Type generatorType
        ) {
            var log = Log.ToConsole($"MazeGeneratorTest.CanGenerateMazes({generatorType.Name})");
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(20, 20), new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Full,
                    Algorithm = generatorType
                }, out var _);
            Assert.That(MazeTestHelper.IsSolveable(maze));
            log.D(5, maze.ToString());
            Assert.That(maze.Cells.Count(cell => cell.Links().Count == 0), Is.EqualTo(0));
        }

        [Test]
        public void OnlyFullGenerators() {
            Assert.Throws<MazeGenerationException>(() =>
                MazeTestHelper.GenerateMaze(new Vector(10, 10),
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.BinaryTree,
                    FillFactor = GeneratorOptions.FillFactorOption.Half
                }, out var _));
            Assert.Throws<MazeGenerationException>(() =>
                MazeTestHelper.GenerateMaze(new Vector(10, 10),
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.Sidewinder,
                    FillFactor = GeneratorOptions.FillFactorOption.Half
                }, out var _));
        }

        [Test]
        public void IsFillComplete_Half() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.Half
                }, out var _);
            Assert.That(MazeTestHelper.IsSolveable(maze));
            var mazeCells = maze.Cells.Where(cell => cell.Links().Count > 0).ToList();
            Assert.That(mazeCells.Count(), Is.GreaterThanOrEqualTo(size.Area / 2), maze.ToString());
        }

        [Test]
        public void IsFillComplete_Full() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.Full
                }, out var _);
            Assert.That(MazeTestHelper.IsSolveable(maze));
            var mazeCells = maze.Cells.Where(cell => cell.Links().Count > 0).ToList();
            Assert.That(size.Area, Is.EqualTo(mazeCells.Count()));
        }

        [Test]
        public void IsFillComplete_Quarter() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.Quarter
                }, out var _);
            Assert.That(MazeTestHelper.IsSolveable(maze));
            var mazeCells = maze.Cells.Where(cell => cell.Links().Count > 0).ToList();
            Assert.That(mazeCells.Count(), Is.GreaterThanOrEqualTo(size.Area * 0.25), maze.ToString());
        }

        [Test]
        public void IsFillComplete_ThreeQuarters() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.ThreeQuarters
                }, out var _);
            Assert.That(MazeTestHelper.IsSolveable(maze));
            var mazeCells = maze.Cells.Where(cell => cell.Links().Count > 0).ToList();
            Assert.That(mazeCells.Count(), Is.GreaterThanOrEqualTo(size.Area * 0.75), maze.ToString());
        }

        [Test]
        public void IsFillComplete_NinetyPercent() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.NinetyPercent
                }, out var _);
            Assert.That(MazeTestHelper.IsSolveable(maze));
            var mazeCells = maze.Cells.Where(cell => cell.Links().Count > 0).ToList();
            Assert.That(mazeCells.Count(), Is.GreaterThanOrEqualTo(size.Area * 0.9), maze.ToString());
        }

        [Test]
        public void IsFillComplete_FullWidth() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.FullWidth
                }, out var _);
            Assert.That(MazeTestHelper.IsSolveable(maze));
            var mazeCells = maze.Cells.Where(cell => cell.Links().Count > 0).ToList();
            Assert.That(mazeCells.Min(cell => cell.Position.X) == 0 && mazeCells.Max(cell => cell.Position.X) == 9, Is.True, maze.ToString());
        }

        [Test]
        public void IsFillComplete_FullHeight() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.FullHeight
                }, out var _);
            Assert.That(MazeTestHelper.IsSolveable(maze));
            var mazeCells = maze.Cells.Where(cell => cell.Links().Count > 0).ToList();
            Assert.That(mazeCells.Min(cell => cell.Position.Y) == 0 && mazeCells.Max(cell => cell.Position.Y) == 9, Is.True, maze.ToString());
        }

        [Test, Category("Integration")]
        public void IsFillComplete(
            [ValueSource("GetAllGenerators")]
            Type generatorType,
            [ValueSource("GetGeneratorOptionsFillFactors")]
            GeneratorOptions.FillFactorOption fillFactor,
            [ValueSource("GetGeneratorOptionsMapAreaOptions")]
            GeneratorOptions.MapAreaOptions mapAreaOptions,
            [ValueSource("GetGeneratorOptionsMapAreas")]
            List<Area> mapAreas
        ) {
            if (!MazeTestHelper.IsSupported(generatorType, fillFactor)) {
                Assert.Ignore();
            }
            var size = new Vector(10, 10);
            var options = new GeneratorOptions() {
                Algorithm = generatorType,
                FillFactor = fillFactor,
                MapAreasOptions = mapAreaOptions,
                MapAreas = mapAreas,
            };

            var map = MazeTestHelper.GenerateMaze(size, options);
            Assert.That(map, Is.Not.Null);

            var solution = new LongestTrailExtension(new List<Cell>());
            Assert.DoesNotThrow(() => solution = DijkstraDistance.FindLongestTrail(map));
            Assert.That(solution, Is.Not.Null);
            Assert.That(solution.LongestTrail, Is.Not.Null.Or.Empty);

            map.X(DeadEnd.Find(map));
            map.X(solution);
        }

        [Test, Category("Integration")]
        public void IsFillComplete_Debug() {
            IsFillComplete(typeof(HuntAndKillMazeGenerator),
                           GeneratorOptions.FillFactorOption.ThreeQuarters,
                           GeneratorOptions.MapAreaOptions.Auto,
                           new List<Area>() {
                Area.CreateUnpositioned(new Vector(3, 2), new Vector(2, 3), AreaType.Hall),
                Area.CreateUnpositioned(new Vector(6, 5), new Vector(3, 2), AreaType.Fill) });
        }

        [Test]
        public void WrongGeneratorOptions() {
            Assert.That(() => MazeTestHelper.GenerateMaze(new Vector(3, 4), new GeneratorOptions()), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => MazeTestHelper.GenerateMaze(new Vector(3, 4), new GeneratorOptions { Algorithm = typeof(string) }), Throws.TypeOf<ArgumentException>());
            Assert.That(() => MazeTestHelper.GenerateMaze(new Vector(3, 4), new GeneratorOptions { Algorithm = typeof(TestGeneratorA) }), Throws.TypeOf<ArgumentException>());
            Assert.That(() => MazeTestHelper.GenerateMaze(new Vector(3, 4), new GeneratorOptions { Algorithm = typeof(TestGeneratorB) }), Throws.Nothing);
        }

        class TestGeneratorA : MazeGenerator {
            public TestGeneratorA(string _) : base() { }
            public override void GenerateMaze(Maze2DBuilder builder) { }
        }

        class TestGeneratorB : MazeGenerator {
            public override void GenerateMaze(Maze2DBuilder builder) {
                builder.AllCells.ForEach(cell => {
                    try {
                        builder.Connect(cell, Vector.East2D);
                    } catch (InvalidOperationException) { }
                    try {
                        builder.Connect(cell, Vector.North2D);
                    } catch (InvalidOperationException) { }
                });
            }
        }

        [Test, Category("Integration")]
        public void CanFindPaths(
            [ValueSource("GetAllGenerators")] Type generatorType
        ) {
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(3, 4),
                new GeneratorOptions() { Algorithm = generatorType },
                out var builder);
            var solution = new LongestTrailExtension(new List<Cell>());
            Assert.DoesNotThrow(() => solution = DijkstraDistance.FindLongestTrail(maze));
            Assert.That(solution, Is.Not.Null);
            Assert.That(solution.LongestTrail, Is.Not.Null.Or.Empty);
        }

        public static IEnumerable<Type> GetAllGenerators() =>
            MazeTestHelper.GetAllGenerators();

        public static IEnumerable<GeneratorOptions.FillFactorOption>
            GetGeneratorOptionsFillFactors() =>
            MazeTestHelper.GetAllFillFactors();

        public static IEnumerable<GeneratorOptions.MapAreaOptions>
            GetGeneratorOptionsMapAreaOptions() {
            yield return GeneratorOptions.MapAreaOptions.Auto;
            yield return GeneratorOptions.MapAreaOptions.Manual;
        }

        public static IEnumerable<List<Area>>
            GetGeneratorOptionsMapAreas() {
            yield return null;
            yield return new List<Area>();
            yield return new List<Area>() {
                Area.CreateUnpositioned(new Vector(3, 2), new Vector(2, 3), AreaType.Fill) };
            yield return new List<Area>() {
                Area.CreateUnpositioned(new Vector(3, 2), new Vector(3, 2), AreaType.Hall) };
            yield return new List<Area>() {
                Area.CreateUnpositioned(new Vector(3, 2), new Vector(2, 3), AreaType.Fill),
                Area.CreateUnpositioned(new Vector(6, 5), new Vector(3, 2), AreaType.Hall) };
            yield return new List<Area>() {
                Area.CreateUnpositioned(new Vector(3, 2), new Vector(2, 3), AreaType.Hall),
                Area.CreateUnpositioned(new Vector(6, 5), new Vector(3, 2), AreaType.Fill) };
        }
    }
}