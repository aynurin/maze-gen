using System;
using System.Linq;
using Nour.Play.Maze;
using Nour.Play.Maze.PostProcessing;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class Maze2DTest {
        [Test]
        public void Maze2D_IsInitialized() {
            Maze2D map = new Maze2D(2, 3);
            Assert.AreEqual(6, map.Area);
            Assert.AreEqual(6, map.Cells.Count);
            Assert.AreEqual(2, map.XHeightRows);
            Assert.AreEqual(3, map.YWidthColumns);
        }

        [Test]
        public void Maze2D_WrongSize() {
            Assert.Throws<ArgumentException>(() => new Maze2D(0, 3));
            Assert.Throws<ArgumentException>(() => new Maze2D(2, 0));
            Assert.Throws<ArgumentException>(() => new Maze2D(-1, 1));
            Assert.Throws<ArgumentException>(() => new Maze2D(1, -1));
            Assert.Throws<ArgumentException>(() => new Maze2D(-1, -1));
        }

        [Test]
        public void Maze2D_ToMapWrongOptions() {
            Assert.DoesNotThrow(() => new Maze2D(2, 3).ToMap(Maze2DToMap2DConverter.MazeToMapOptions.Custom(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 3, 4 })));
            Assert.Throws<ArgumentException>(() => new Maze2D(2, 3).ToMap(Maze2DToMap2DConverter.MazeToMapOptions.Custom(new int[] { 2 }, new int[] { 1, 2, 3 }, new int[] { 1 }, new int[] { 1, 2 })));
            Assert.Throws<ArgumentException>(() => new Maze2D(2, 3).ToMap(Maze2DToMap2DConverter.MazeToMapOptions.Custom(new int[] { 1, 2 }, new int[] { 1, 3 }, new int[] { 1 }, new int[] { 1, 2 })));
            Assert.Throws<ArgumentException>(() => new Maze2D(2, 3).ToMap(Maze2DToMap2DConverter.MazeToMapOptions.Custom(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { }, new int[] { 1, 2 })));
            Assert.Throws<ArgumentException>(() => new Maze2D(2, 3).ToMap(Maze2DToMap2DConverter.MazeToMapOptions.Custom(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { 1 }, new int[] { 1 })));
            Assert.Throws<ArgumentException>(() => new Maze2D(2, 3).ToMap(Maze2DToMap2DConverter.MazeToMapOptions.Custom(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { 1 }, new int[] { })));
            Assert.Throws<ArgumentNullException>(() => new Maze2D(2, 3).ToMap(Maze2DToMap2DConverter.MazeToMapOptions.Custom(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 1, 2 }, null)));
        }

        [Test]
        public void Maze2D_CellsNeighborsAreValid() {
            int rows = 5;
            int cols = 5;
            Maze2D map = new Maze2D(rows, cols);

            Assert.AreEqual(map.Cells.Count, rows * cols);
            for (int x = 0; x < rows; x++) {
                for (int y = 0; y < cols; y++) {
                    var cell = map[x, y];
                    var neighbors = map.Cells.Where(c =>
                        (c.X == cell.X && Math.Abs(c.Y - cell.Y) == 1) ||
                        (c.Y == cell.Y && Math.Abs(c.X - cell.X) == 1))
                            .ToList();
                    var nonNeighbors = map.Cells
                        .Where(c => !neighbors.Contains(c))
                        .ToList();
                    Assert.AreEqual(neighbors.Count, cell.Neighbors().Count);
                    Assert.IsTrue(neighbors.All(c => cell.Neighbors().Contains(c)));
                    Assert.IsFalse(nonNeighbors.Any(c => cell.Neighbors().Contains(c)));
                    if (x > 0) Assert.AreEqual(cell.Neighbors(Vector.North2D), map[x - 1, y]);
                    if (x + 1 < rows) Assert.AreEqual(cell.Neighbors(Vector.South2D), map[x + 1, y]);
                    if (y > 0) Assert.AreEqual(cell.Neighbors(Vector.West2D), map[x, y - 1]);
                    if (y + 1 > cols) Assert.AreEqual(cell.Neighbors(Vector.East2D), map[x, y + 1]);
                }
            }
        }

        [Test]
        public void Maze2D_CanScaleMap() {
            var map = MazeGenerator.Generate<AldousBroderMazeGenerator>(new Vector(3, 3),
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Full
                });
            var scaledMap = map.ToMap(Maze2DToMap2DConverter.MazeToMapOptions.Custom(new int[] { 2, 3, 2 }, new int[] { 2, 3, 2 }, new int[] { 1, 2, 1, 2 }, new int[] { 1, 3, 1, 3 }));

            var expectedSize = new Vector(13, 15);

            Assert.AreEqual(expectedSize.Area, scaledMap.Cells.Count);
        }

        [Test]
        public void Maze2D_HasAttributesSet() {
            var map = MazeGenerator.Generate<AldousBroderMazeGenerator>(new Vector(3, 3),
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Full
                });
            Assert.IsNotEmpty(map.Attributes[DeadEnd.DeadEndAttribute]);
            Assert.IsNotEmpty(map.Attributes[DijkstraDistance.LongestTrailAttribute]);
        }

        [Test]
        public void Maze2D_CanParse() {
            var maze = Maze2D.Parse("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            Assert.IsTrue(!maze.Cells[0].Links(Vector.East2D).HasValue);
            Assert.IsTrue(!maze.Cells[4].Links(Vector.East2D).HasValue);
            Assert.IsTrue(maze.Cells[4].Links(Vector.North2D).HasValue);
            Assert.IsTrue(maze.Cells[4].Links(Vector.South2D).HasValue);
            Assert.IsTrue(maze.Cells[7].Links(Vector.East2D).HasValue);
            Assert.IsTrue(maze.Cells[7].Links(Vector.West2D).HasValue);
            Assert.IsTrue(maze.Cells[7].Links(Vector.North2D).HasValue);
            Assert.IsTrue(!maze.Cells[7].Links(Vector.South2D).HasValue);
            Assert.IsTrue(maze.Cells[6].Links(Vector.East2D).HasValue);
            Assert.IsTrue(!maze.Cells[6].Links(Vector.West2D).HasValue);
            Assert.IsTrue(!maze.Cells[6].Links(Vector.North2D).HasValue);
            Assert.IsTrue(!maze.Cells[6].Links(Vector.South2D).HasValue);
            Assert.IsTrue(maze.Cells[6].Neighbors(Vector.East2D).HasValue);
            Assert.IsTrue(!maze.Cells[6].Neighbors(Vector.West2D).HasValue);
            Assert.IsTrue(maze.Cells[6].Neighbors(Vector.North2D).HasValue);
            Assert.IsTrue(!maze.Cells[6].Neighbors(Vector.South2D).HasValue);
        }
    }
}