using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze;
using PlayersWorlds.Maps.Maze.PostProcessing;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps.Maze {
    [TestFixture]
    public class MazeGeneratorTest {

        private class CustomAreaGenerator : AreaGenerator {
            public override IEnumerable<MapArea> Generate(
                Vector size, List<MapArea> existingAreas) {
                if (size.X <= 10 || size.Y <= 10) {
                    return Enumerable.Empty<MapArea>();
                }
                return new[] {
                    MapArea.Create(
                        AreaType.Hall, new Vector(0, 0), new Vector(2, 3))
                };
            }
        }

        [Test]
        public void CanUseCustomAreaGenerator() {
            var maze = MazeGenerator.Generate(
                new Vector(15, 15),
                new GeneratorOptions() {
                    AreaGenerator = new CustomAreaGenerator(),
                    FillFactor = GeneratorOptions.FillFactorOption.Full,
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    MapAreasOptions = GeneratorOptions.MapAreaOptions.Auto
                });
            Console.WriteLine(maze.ToString());
            Assert.That(maze.MapAreas, Has.Count.EqualTo(1));
            Assert.That(maze.MapAreas.First().Key.Size,
                Is.EqualTo(new Vector(2, 3)));
            Assert.That(maze.MapAreas.First().Key.Position,
                Is.EqualTo(new Vector(0, 0)));
            Assert.That(maze.MapAreas.First().Key.Type,
                Is.EqualTo(AreaType.Hall));
        }

        [Test]
        public void CanGenerateMazes(
            [ValueSource("GetAllGenerators")] Type generatorType
        ) {
            var maze = MazeGenerator.Generate(
                new Vector(20, 20), new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Full,
                    Algorithm = generatorType
                });
            Log.CreateForThisTest().D(5, maze.ToString());
            Assert.That(maze.AllCells.Count(cell => cell.Links().Count == 0), Is.EqualTo(0));
        }

        [Test]
        public void OnlyFullGenerators() {
            Assert.Throws<MazeGenerationException>(() =>
                MazeGenerator.Generate(new Vector(10, 10),
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.BinaryTree,
                    FillFactor = GeneratorOptions.FillFactorOption.Half
                }));
            Assert.Throws<MazeGenerationException>(() =>
                MazeGenerator.Generate(new Vector(10, 10),
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.Sidewinder,
                    FillFactor = GeneratorOptions.FillFactorOption.Half
                }));
        }

        [Test]
        public void IsFillComplete_Half() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.Half
                });
            Console.WriteLine(maze.ToString());
            Assert.That(maze.VisitedCells.Count(), Is.GreaterThanOrEqualTo(size.Area / 2), maze.ToString());
        }

        [Test]
        public void IsFillComplete_Full() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.Full
                });
            Console.WriteLine(maze.ToString());
            Assert.That(size.Area, Is.EqualTo(maze.VisitedCells.Count()));
        }

        [Test]
        public void IsFillComplete_Quarter() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.Quarter
                });
            Console.WriteLine(maze.ToString());
            Assert.That(maze.VisitedCells.Count(), Is.GreaterThanOrEqualTo(size.Area * 0.25), maze.ToString());
        }

        [Test]
        public void IsFillComplete_ThreeQuarters() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.ThreeQuarters
                });
            Console.WriteLine(maze.ToString());
            Assert.That(maze.VisitedCells.Count(), Is.GreaterThanOrEqualTo(size.Area * 0.75), maze.ToString());
        }

        [Test]
        public void IsFillComplete_NinetyPercent() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.NinetyPercent
                });
            Console.WriteLine(maze.ToString());
            Assert.That(maze.VisitedCells.Count(), Is.GreaterThanOrEqualTo(size.Area * 0.9), maze.ToString());
        }

        [Test]
        public void IsFillComplete_FullWidth() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.FullWidth
                });
            Console.WriteLine(maze.ToString());
            Assert.That(maze.UnlinkedCells.Min(cell => cell.Y) == 0 && maze.UnlinkedCells.Max(cell => cell.Y) == 9, Is.True, maze.ToString());
        }

        [Test]
        public void IsFillComplete_FullHeight() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.FullHeight
                });
            Console.WriteLine(maze.ToString());
            Assert.That(maze.UnlinkedCells.Min(cell => cell.X) == 0 && maze.UnlinkedCells.Max(cell => cell.X) == 9, Is.True, maze.ToString());
        }

        [Test, Property("Category", "Load")]
        public void IsFillComplete(
            [ValueSource("GetAllGenerators")]
            Type generatorType,
            [ValueSource("GetGeneratorOptionsFillFactors")]
            GeneratorOptions.FillFactorOption fillFactor,
            [ValueSource("GetGeneratorOptionsMapAreaOptions")]
            GeneratorOptions.MapAreaOptions mapAreaOptions,
            [ValueSource("GetGeneratorOptionsMapAreas")]
            List<MapArea> mapAreas
        ) {
            if (!MazeTestHelper.IsSupported(generatorType, fillFactor)) {
                Assert.Ignore();
            }
            var size = new Vector(10, 10);
            var generator = (MazeGenerator)Activator.CreateInstance(generatorType);
            var options = new GeneratorOptions() {
                FillFactor = fillFactor,
                MapAreasOptions = mapAreaOptions,
                MapAreas = mapAreas
            };

            Assert.That(generator, Is.Not.Null, "Could not create generator of type {0}", generatorType.Name);
            var generate = typeof(MazeGenerator).GetMethods().First(m => m.Name == "Generate" && m.IsGenericMethod);
            Assert.That(generate, Is.Not.Null, "Could not find Generate method on generator of type {0}", generatorType.Name);
            Assert.That(generate.MakeGenericMethod(generatorType), Is.Not.Null, "Could not find generic Generate method on generator of type {0}", generatorType.Name);

            var map = (Maze2D)generate.MakeGenericMethod(generatorType)
                .Invoke(generator, new object[] { size, options });

            var solution = new List<MazeCell>();
            Assert.DoesNotThrow(() => solution = DijkstraDistance.FindLongestTrail(map));
            Assert.That(solution, Is.Not.Null.Or.Empty);

            map.Attributes.Set(DeadEnd.DeadEndAttribute, DeadEnd.Find(map));
            map.Attributes.Set(DijkstraDistance.LongestTrailAttribute,
                solution);
        }

        [Test]
        public void WrongGeneratorOptions() {
            Assert.That(() => MazeGenerator.Generate(new Vector(3, 4), new GeneratorOptions()), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => MazeGenerator.Generate(new Vector(3, 4), new GeneratorOptions { Algorithm = typeof(string) }), Throws.TypeOf<ArgumentException>());
            Assert.That(() => MazeGenerator.Generate(new Vector(3, 4), new GeneratorOptions { Algorithm = typeof(TestGeneratorA) }), Throws.TypeOf<ArgumentException>());
            Assert.That(() => MazeGenerator.Generate(new Vector(3, 4), new GeneratorOptions { Algorithm = typeof(TestGeneratorB) }), Throws.Nothing);
        }

        class TestGeneratorA : MazeGenerator {
            public TestGeneratorA(string _) : base() { }
            public override void GenerateMaze(Maze2DBuilder builder) { }
        }

        class TestGeneratorB : MazeGenerator {
            public override void GenerateMaze(Maze2DBuilder builder) {
                builder.AllMazeCells().ForEach(cell => {
                    try {
                        builder.Connect(cell, Vector.East2D);
                    } catch (InvalidOperationException) { }
                    try {
                        builder.Connect(cell, Vector.North2D);
                    } catch (InvalidOperationException) { }
                });
            }
        }

        [Test]
        public void CanFindPaths(
            [ValueSource("GetAllGenerators")] Type generatorType
        ) {
            var generator = (MazeGenerator)Activator.CreateInstance(generatorType);
            var map = new Maze2D(3, 4);
            var builder = new Maze2DBuilder(map,
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Full
                });
            generator.GenerateMaze(builder);
            var solution = new List<MazeCell>();
            Assert.DoesNotThrow(() => solution = DijkstraDistance.FindLongestTrail(map));
            Assert.That(solution, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void AreasAreAppliedProperly(
            [ValueSource("GetAllGenerators")] Type generatorType) {
            var maze = MazeGenerator.Generate(new Vector(10, 10),
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Full,
                    Algorithm = generatorType,
                    MapAreasOptions = GeneratorOptions.MapAreaOptions.Manual,
                    MapAreas = new List<MapArea>() {
                            MapArea.Create(AreaType.Fill, new Vector(2, 1), new Vector(2, 3)),
                            MapArea.Create(AreaType.Fill, new Vector(1, 5), new Vector(3, 4)),
                            MapArea.Create(AreaType.Hall, new Vector(5, 6), new Vector(3, 2)),
                            MapArea.Create(AreaType.Hall, new Vector(5, 1), new Vector(4, 3))
                    }
                });
            var log = Log.CreateForThisTest();
            log.D(5, maze.ToString());
            // 1. All cells of filled areas:
            //      - have no neighbors 
            //      - are not neighbors of any other cells
            // 2. All cells of hall areas:
            //      - are linked within the area
            //      - are linked to at least one outside cell
            // for mazes generated by any algorithm
            var fillAreas = 0;
            var hallAreas = 0;
            foreach (var area in maze.MapAreas) {
                if (area.Key.Type == AreaType.Fill) {
                    Assert.That(
                        area.Value.All(cell => cell.Neighbors().Count == 0),
                        "filled area cells should have no neighbors");
                    Assert.That(
                        area.Value.All(cell => cell.Links().Count == 0),
                        "filled area cells should have no links");
                    Assert.That(
                        !area.Value.Any(cell => maze.UnlinkedCells.Contains(cell)),
                        "filled area cells should not be in the maze unlinked cells pool");
                    Assert.That(
                        maze.UnlinkedCells, Has.None.AnyOf(area.Value),
                        "filled area cells should not be in the maze unlinked cells pool");
                    Assert.That(
                        maze.UnlinkedCells.SelectMany(cell => cell.Neighbors()),
                        Has.None.AnyOf(area.Value),
                        "filled area cells should not be neighbors of any other cells");
                    fillAreas++;
                } else if (area.Key.Type == AreaType.Hall) {
                    var lowX = area.Value.Min(cell => cell.X);
                    var highX = area.Value.Max(cell => cell.X);
                    var lowY = area.Value.Min(cell => cell.Y);
                    var highY = area.Value.Max(cell => cell.Y);
                    var corderCells = area.Value.Where(cell =>
                        (cell.X == lowX || cell.X == highX) &&
                        (cell.Y == lowY || cell.Y == highY)).ToList();
                    var edgeCells = area.Value.Where(cell =>
                        !corderCells.Contains(cell) && (
                        cell.X == lowX || cell.X == highX ||
                        cell.Y == lowY || cell.Y == highY)).ToList();
                    var innerCells = area.Value.Where(cell =>
                        cell.X != lowX && cell.X != highX &&
                        cell.Y != lowY && cell.Y != highY).ToList();
                    var perimeter = -4 + 2 *
                        (area.Key.Size.Value.Sum() - area.Key.Size.Value.Length);
                    Assert.That(corderCells, Has.Exactly(4).Items,
                        $"perimeter of S{area.Key.Size} should be exactly {perimeter} cells");
                    Assert.That(edgeCells, Has.Exactly(perimeter).Items,
                        $"perimeter of S{area.Key.Size} should be exactly {perimeter} cells");
                    Assert.That(corderCells.Count +
                                edgeCells.Count +
                                innerCells.Count,
                        Is.EqualTo(area.Key.Size.Area));
                    Assert.That(corderCells.All(cell => cell.Links().Count >= 2),
                        "hall area edge cells should have at least 3 links");
                    Assert.That(edgeCells.All(cell => cell.Links().Count >= 3),
                        "hall area edge cells should have at least 3 links");
                    if (innerCells.Count > 0) {
                        Assert.That(
                            innerCells.All(cell => cell.Links().Count == 4),
                            $"hall area cells should be interconnected");
                    }
                    // TODO: This is not implemented yet
                    // Assert.That(edgeCells.Any(cell => cell.Links().Count == 4), "hall areas should have exits");
                    hallAreas++;
                } else {
                    Assert.Fail($"Unknown area type: {area.Key.Type}");
                }
            }
            Assert.That(fillAreas, Is.EqualTo(2), "Wrong number of filled areas");
            Assert.That(hallAreas, Is.EqualTo(2), "Wrong number of hall areas");
        }

        public static IEnumerable<Type> GetAllGenerators() {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm.GetName().Name == "PlayersWorlds.Maps")
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(MazeGenerator) != p &&
                            typeof(MazeGenerator).IsAssignableFrom(p));
        }

        public static IEnumerable<GeneratorOptions.FillFactorOption>
            GetGeneratorOptionsFillFactors() {
            yield return GeneratorOptions.FillFactorOption.Full;
            yield return GeneratorOptions.FillFactorOption.NinetyPercent;
            yield return GeneratorOptions.FillFactorOption.ThreeQuarters;
            yield return GeneratorOptions.FillFactorOption.Half;
            yield return GeneratorOptions.FillFactorOption.FullHeight;
            yield return GeneratorOptions.FillFactorOption.FullWidth;
            yield return GeneratorOptions.FillFactorOption.Quarter;
        }

        public static IEnumerable<GeneratorOptions.MapAreaOptions>
            GetGeneratorOptionsMapAreaOptions() {
            yield return GeneratorOptions.MapAreaOptions.Auto;
            yield return GeneratorOptions.MapAreaOptions.Manual;
        }

        public static IEnumerable<List<MapArea>>
            GetGeneratorOptionsMapAreas() {
            yield return null;
            yield return new List<MapArea>();
            yield return new List<MapArea>() {
                MapArea.Create(AreaType.Fill, new Vector(3, 2), new Vector(2, 3)) };
            yield return new List<MapArea>() {
                MapArea.Create(AreaType.Hall, new Vector(3, 2), new Vector(3, 2)) };
            yield return new List<MapArea>() {
                MapArea.Create(AreaType.Fill, new Vector(3, 2), new Vector(2, 3)),
                MapArea.Create(AreaType.Hall, new Vector(6, 5), new Vector(3, 2)) };
            yield return new List<MapArea>() {
                MapArea.Create(AreaType.Hall, new Vector(3, 2), new Vector(2, 3)),
                MapArea.Create(AreaType.Fill, new Vector(6, 5), new Vector(3, 2)) };
        }
    }
}