using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze;
using PlayersWorlds.Maps.Maze.PostProcessing;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps {
    [TestFixture]
    public class MazeGeneratorTest {

        private class CustomAreaGenerator : AreaGenerator {
            public override IEnumerable<MapArea> Generate(Vector size) {
                if (size.X <= 10 || size.Y <= 10) {
                    return Enumerable.Empty<MapArea>();
                }
                return new[] {
                    new MapArea(AreaType.Hall, new Vector(2, 3), new Vector(0, 0))
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
            Assert.That(maze.Areas, Has.Count.EqualTo(1));
            Assert.That(maze.Areas.First().Size, Is.EqualTo(new Vector(2, 3)));
            Assert.That(maze.Areas.First().Type, Is.EqualTo(AreaType.Hall));
        }

        [Test]
        public void CanGenerateMazes(
            [ValueSource("GetAllGenerators")] Type generatorType
        ) {
            var generator = (MazeGenerator)Activator.CreateInstance(generatorType);
            // var map = new Maze2D(3, 4);
            // Assert.IsTrue(map.Cells.All(cell => cell.Links().Count == 0));
            var map = (Maze2D)typeof(MazeGenerator).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "Generate" && m.IsGenericMethod)
                .MakeGenericMethod(generatorType)
                .Invoke(null, new object[] { new Vector(3, 4), new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.Full } });
            // generator.GenerateMaze(map);
            Assert.IsTrue(map.VisitableCells.Count == 12);
            Assert.IsTrue(map.VisitableCells.Any(cell => cell.Links().Count > 0));
        }

        [Test]
        public void OnlyFullGenerators() {
            Assert.Throws<ArgumentException>(() => MazeGenerator.Generate<BinaryTreeMazeGenerator>(new Vector(10, 10), new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.Half }));
            Assert.Throws<ArgumentException>(() => MazeGenerator.Generate<SidewinderMazeGenerator>(new Vector(10, 10), new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.Half }));
        }

        [Test]
        public void IsFillComplete_Half() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(size, new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.Half });
            Console.WriteLine(maze.ToString());
            Assert.GreaterOrEqual(maze.VisitedCells.Count(), size.Area / 2, maze.ToString());
        }

        [Test]
        public void IsFillComplete_Full() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(size, new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.Full });
            Console.WriteLine(maze.ToString());
            Assert.AreEqual(size.Area, maze.VisitedCells.Count());
        }

        [Test]
        public void IsFillComplete_Quarter() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(size, new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.Quarter });
            Console.WriteLine(maze.ToString());
            Assert.GreaterOrEqual(maze.VisitedCells.Count(), size.Area * 0.25, maze.ToString());
        }

        [Test]
        public void IsFillComplete_ThreeQuarters() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(size, new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.ThreeQuarters });
            Console.WriteLine(maze.ToString());
            Assert.GreaterOrEqual(maze.VisitedCells.Count(), size.Area * 0.75, maze.ToString());
        }

        [Test]
        public void IsFillComplete_NinetyPercent() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(size, new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.NinetyPercent });
            Console.WriteLine(maze.ToString());
            Assert.GreaterOrEqual(maze.VisitedCells.Count(), size.Area * 0.9, maze.ToString());
        }

        [Test]
        public void IsFillComplete_FullWidth() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(size, new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.FullWidth });
            Console.WriteLine(maze.ToString());
            Assert.IsTrue(maze.VisitableCells.Min(cell => cell.Y) == 0 && maze.VisitableCells.Max(cell => cell.Y) == 9, maze.ToString());
        }

        [Test]
        public void IsFillComplete_FullHeight() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(size, new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.FullHeight });
            Console.WriteLine(maze.ToString());
            Assert.IsTrue(maze.VisitableCells.Min(cell => cell.X) == 0 && maze.VisitableCells.Max(cell => cell.X) == 9, maze.ToString());
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
            if (!IsSupported(generatorType, fillFactor)) {
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
            Assert.IsNotEmpty(solution);

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
            public override void GenerateMaze(Maze2D map,
                                              GeneratorOptions options) { }
        }

        class TestGeneratorB : MazeGenerator {
            public override void GenerateMaze(Maze2D map,
                                              GeneratorOptions options) {
                map.AllCells.ForEach(cell => {
                    if (cell.Neighbors(Vector.East2D).HasValue)
                        cell.Link(cell.Neighbors(Vector.East2D).Value);
                    if (cell.Neighbors(Vector.North2D).HasValue)
                        cell.Link(cell.Neighbors(Vector.North2D).Value);
                });
            }
        }

        [Test]
        public void CanFindPaths(
            [ValueSource("GetAllGenerators")] Type generatorType
        ) {
            var generator = (MazeGenerator)Activator.CreateInstance(generatorType);
            var map = new Maze2D(3, 4);
            generator.GenerateMaze(map,
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Full
                });
            var solution = new List<MazeCell>();
            Assert.DoesNotThrow(() => solution = DijkstraDistance.FindLongestTrail(map));
            Assert.IsNotEmpty(solution);
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
                new MapArea(AreaType.Fill, new Vector(2, 3), new Vector(3, 2)) };
            yield return new List<MapArea>() {
                new MapArea(AreaType.Hall, new Vector(3, 2), new Vector(3, 2)) };
            yield return new List<MapArea>() {
                new MapArea(AreaType.Fill, new Vector(2, 3), new Vector(3, 2)),
                new MapArea(AreaType.Hall, new Vector(3, 2), new Vector(6, 5)) };
            yield return new List<MapArea>() {
                new MapArea(AreaType.Hall, new Vector(2, 3), new Vector(3, 2)),
                new MapArea(AreaType.Fill, new Vector(3, 2), new Vector(6, 5)) };
        }

        private static bool IsSupported(
            Type generatorType,
            GeneratorOptions.FillFactorOption fillFactor) {
            if ((generatorType == typeof(SidewinderMazeGenerator)
                 || generatorType == typeof(BinaryTreeMazeGenerator))
                && fillFactor != GeneratorOptions.FillFactorOption.Full) {
                return false;
            }
            return true;
        }
    }
}