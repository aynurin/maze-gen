using System;
using System.Collections.Generic;
using System.Linq;
using Nour.Play.Maze;
using Nour.Play.Maze.PostProcessing;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class MazeGeneratorTest {

        [Test]
        public void MazeGenerator_CanGenerateMazes(
            [ValueSource("GetAllGenerators")] Type generatorType
        ) {
            var generator = (MazeGenerator)Activator.CreateInstance(generatorType);
            // var map = new Maze2D(3, 4);
            // Assert.IsTrue(map.Cells.All(cell => cell.Links().Count == 0));
            var map = (Maze2D)typeof(MazeGenerator).GetMethod("Generate")
                .MakeGenericMethod(generatorType)
                .Invoke(null, new Object[] { new Vector(3, 4), new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.Full } });
            // generator.GenerateMaze(map);
            Assert.IsTrue(map.Cells.Count == 12);
            Assert.IsTrue(map.Cells.Any(cell => cell.Links().Count > 0));
        }

        [Test]
        public void MazeGenerator_OnlyFullGenerators() {
            Assert.Throws<ArgumentException>(() => MazeGenerator.Generate<BinaryTreeMazeGenerator>(new Vector(10, 10), new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.Half }));
            Assert.Throws<ArgumentException>(() => MazeGenerator.Generate<SidewinderMazeGenerator>(new Vector(10, 10), new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.Half }));
        }

        [Test]
        public void MazeGenerator_IsFillComplete_Half() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(size, new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.Half });
            Assert.GreaterOrEqual(maze.Cells.Count(c => c.IsVisited), size.Area / 2, maze.ToString());
        }

        [Test]
        public void MazeGenerator_IsFillComplete_Full() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(size, new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.Full });
            Assert.AreEqual(size.Area, maze.Cells.Count(c => c.IsVisited), maze.ToString());
        }

        [Test]
        public void MazeGenerator_IsFillComplete_Quarter() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(size, new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.Quarter });
            Assert.GreaterOrEqual(maze.Cells.Count(c => c.IsVisited), size.Area * 0.25, maze.ToString());
        }

        [Test]
        public void MazeGenerator_IsFillComplete_ThreeQuarters() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(size, new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.ThreeQuarters });
            Assert.GreaterOrEqual(maze.Cells.Count(c => c.IsVisited), size.Area * 0.75, maze.ToString());
        }

        [Test]
        public void MazeGenerator_IsFillComplete_NinetyPercent() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(size, new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.NinetyPercent });
            Assert.GreaterOrEqual(maze.Cells.Count(c => c.IsVisited), size.Area * 0.9, maze.ToString());
        }

        [Test]
        public void MazeGenerator_IsFillComplete_FullWidth() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(size, new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.FullWidth });
            Assert.IsTrue(maze.Cells.Min(cell => cell.Y) == 0 && maze.Cells.Max(cell => cell.Y) == 9, maze.ToString());
        }

        [Test]
        public void MazeGenerator_IsFillComplete_FullHeight() {
            var size = new Vector(10, 10);
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(size, new GeneratorOptions() { FillFactor = GeneratorOptions.FillFactorOption.FullHeight });
            Assert.IsTrue(maze.Cells.Min(cell => cell.X) == 0 && maze.Cells.Max(cell => cell.X) == 9, maze.ToString());
        }

        [Test]
        public void DijkstraDistance_CanFindPaths(
            [ValueSource("GetAllGenerators")] Type generatorType
        ) {
            var generator = (MazeGenerator)Activator.CreateInstance(generatorType);
            var map = new Maze2D(3, 4);
            generator.GenerateMaze(map,
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Full
                });
            List<MazeCell> solution = new List<MazeCell>();
            Assert.DoesNotThrow(() => solution = DijkstraDistance.FindLongestTrail(map));
            Assert.IsNotEmpty(solution);
        }

        public static IEnumerable<Type> GetAllGenerators() {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm.GetName().Name == "Nour.Play.Maze")
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(MazeGenerator) != p &&
                            typeof(MazeGenerator).IsAssignableFrom(p));
        }
    }
}