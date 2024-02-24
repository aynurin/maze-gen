using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze.PostProcessing;

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
            var log = Log.CreateForThisTest();
            var maze = MazeGenerator.Generate(
                new Vector(15, 15),
                new GeneratorOptions() {
                    AreaGenerator = new CustomAreaGenerator(),
                    FillFactor = GeneratorOptions.FillFactorOption.Full,
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    MapAreasOptions = GeneratorOptions.MapAreaOptions.Auto
                });
            log.D(5, maze.ToString());
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
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(20, 20), new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Full,
                    Algorithm = generatorType
                }, out var _);
            Log.CreateForThisTest().D(5, maze.ToString());
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
            Assert.That(maze.MazeCells.Count(), Is.GreaterThanOrEqualTo(size.Area / 2), maze.ToString());
        }

        [Test]
        public void IsFillComplete_Full() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.Full
                }, out var _);
            Assert.That(size.Area, Is.EqualTo(maze.MazeCells.Count()));
        }

        [Test]
        public void IsFillComplete_Quarter() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.Quarter
                }, out var _);
            Assert.That(maze.MazeCells.Count(), Is.GreaterThanOrEqualTo(size.Area * 0.25), maze.ToString());
        }

        [Test]
        public void IsFillComplete_ThreeQuarters() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.ThreeQuarters
                }, out var _);
            Assert.That(maze.MazeCells.Count(), Is.GreaterThanOrEqualTo(size.Area * 0.75), maze.ToString());
        }

        [Test]
        public void IsFillComplete_NinetyPercent() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.NinetyPercent
                }, out var _);
            Assert.That(maze.MazeCells.Count(), Is.GreaterThanOrEqualTo(size.Area * 0.9), maze.ToString());
        }

        [Test]
        public void IsFillComplete_FullWidth() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.FullWidth
                }, out var _);
            Assert.That(maze.MazeCells.Min(cell => cell.X) == 0 && maze.MazeCells.Max(cell => cell.X) == 9, Is.True, maze.ToString());
        }

        [Test]
        public void IsFillComplete_FullHeight() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.FullHeight
                }, out var _);
            Assert.That(maze.MazeCells.Min(cell => cell.Y) == 0 && maze.MazeCells.Max(cell => cell.Y) == 9, Is.True, maze.ToString());
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