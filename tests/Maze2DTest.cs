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
            Assert.AreEqual(2, map.XWidthColumns);
            Assert.AreEqual(3, map.YHeightRows);
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
            Assert.DoesNotThrow(() => new Maze2D(2, 3).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 3, 4 })));
            Assert.Throws<ArgumentException>(() => new Maze2D(2, 3).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 2 }, new int[] { 1, 2, 3 }, new int[] { 1 }, new int[] { 1, 2 })));
            Assert.Throws<ArgumentException>(() => new Maze2D(2, 3).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 3 }, new int[] { 1 }, new int[] { 1, 2 })));
            Assert.Throws<ArgumentException>(() => new Maze2D(2, 3).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { }, new int[] { 1, 2 })));
            Assert.Throws<ArgumentException>(() => new Maze2D(2, 3).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { 1 }, new int[] { 1 })));
            Assert.Throws<ArgumentException>(() => new Maze2D(2, 3).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { 1 }, new int[] { })));
            Assert.Throws<ArgumentNullException>(() => new Maze2D(2, 3).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 1, 2 }, null)));
        }

        [Test]
        public void Maze2D_CellsNeighborsAreValid() {
            var rows = 5;
            var cols = 5;
            var map = new Maze2D(cols, rows);

            Assert.AreEqual(map.Cells.Count, rows * cols, "Wrong number of cells.");
            for (var x = 0; x < cols; x++) {
                for (var y = 0; y < rows; y++) {
                    var cell = map.Cells.ElementAt(new Vector(x, y), map.Size);
                    var neighbors = map.Cells.Where(c =>
                        (c.X == cell.X && Math.Abs(c.Y - cell.Y) == 1) ||
                        (c.Y == cell.Y && Math.Abs(c.X - cell.X) == 1))
                            .ToList();
                    var nonNeighbors = map.Cells
                        .Where(c => c != cell && !neighbors.Contains(c))
                        .ToList();
                    Assert.AreEqual(neighbors.Count, cell.Neighbors().Count);
                    Assert.IsTrue(neighbors.All(c => cell.Neighbors().Contains(c)));
                    Assert.IsFalse(nonNeighbors.Any(c => cell.Neighbors().Contains(c)));

                    if (x > 0) Assert.AreEqual(cell.Neighbors(Vector.West2D), map.Cells.ElementAt(new Vector(x - 1, y), map.Size), "Neighbors(West2D)");
                    if (x + 1 < cols) Assert.AreEqual(cell.Neighbors(Vector.East2D), map.Cells.ElementAt(new Vector(x + 1, y), map.Size), "Neighbors(East2D)");
                    if (y > 0) Assert.AreEqual(cell.Neighbors(Vector.South2D), map.Cells.ElementAt(new Vector(x, y - 1), map.Size), "Neighbors(South2D)");
                    if (y + 1 > rows) Assert.AreEqual(cell.Neighbors(Vector.North2D), map.Cells.ElementAt(new Vector(x, y + 1), map.Size), "Neighbors(North2D)");
                }
            }
        }

        [Test]
        public void Maze2D_CanRenderMap() {
            var map = MazeGenerator.Generate<AldousBroderMazeGenerator>(new Vector(3, 3),
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Full
                });
            var scaledMap = map.ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 2, 3, 2 }, new int[] { 2, 3, 2 }, new int[] { 1, 2, 1, 2 }, new int[] { 1, 3, 1, 3 }));

            var expectedSize = new Vector(13, 15);

            Assert.AreEqual(expectedSize.Area, scaledMap.Cells.Count);
        }

        [Test]
        public void Maze2D_AddsNoRoomsWhenNoneRequested() {
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(new Vector(10, 10),
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Full,
                    MapAreas = GeneratorOptions.MapAreaOptions.Manual,
                });
            Assert.That(maze.Areas.Count, Is.EqualTo(0));
        }

        [Test]
        public void Maze2D_AddsNoRoomsToASmallMaze() {
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(new Vector(3, 3),
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Full,
                    MapAreas = GeneratorOptions.MapAreaOptions.Auto,
                });
            Assert.That(maze.Areas.Count, Is.EqualTo(0));
        }

        [Test]
        public void Maze2D_AddsRooms() {
            var maze = MazeGenerator.Generate<AldousBroderMazeGenerator>(new Vector(5, 5),
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Full,
                    MapAreas = GeneratorOptions.MapAreaOptions.Auto,
                });
            Assert.That(maze.Areas.Count, Is.GreaterThan(0));
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
            Console.WriteLine(maze.ToString());
            // ╔═══╤═══════╗
            // ║ 0 │ 1   2 ║
            // ║   ╵   ╷   ║
            // ║ 3   4 │ 5 ║
            // ╟───╴   └───╢
            // ║ 6   7   8 ║
            // ╚═══════════╝
            Assert.IsTrue(!maze.Cells[0].Links(Vector.East2D).HasValue);
            Assert.IsTrue(!maze.Cells[4].Links(Vector.East2D).HasValue);
            Assert.IsTrue(maze.Cells[4].Links(Vector.South2D).HasValue);
            Assert.IsTrue(maze.Cells[4].Links(Vector.North2D).HasValue);
            Assert.IsTrue(maze.Cells[7].Links(Vector.East2D).HasValue);
            Assert.IsTrue(maze.Cells[7].Links(Vector.West2D).HasValue);
            Assert.IsTrue(maze.Cells[7].Links(Vector.South2D).HasValue);
            Assert.IsTrue(!maze.Cells[7].Links(Vector.North2D).HasValue);
            Assert.IsTrue(maze.Cells[6].Links(Vector.East2D).HasValue);
            Assert.IsTrue(!maze.Cells[6].Links(Vector.West2D).HasValue);
            Assert.IsTrue(!maze.Cells[6].Links(Vector.South2D).HasValue);
            Assert.IsTrue(!maze.Cells[6].Links(Vector.North2D).HasValue);
            Assert.IsTrue(maze.Cells[6].Neighbors(Vector.East2D).HasValue);
            Assert.IsTrue(!maze.Cells[6].Neighbors(Vector.West2D).HasValue);
            Assert.IsTrue(maze.Cells[6].Neighbors(Vector.South2D).HasValue);
            Assert.IsTrue(!maze.Cells[6].Neighbors(Vector.North2D).HasValue);
        }

        [Test]
        public void Maze2D_CanParse2() {
            var maze = Maze2D.Parse("3x4");
            Assert.IsTrue(maze.Size.X == 3 && maze.Size.Y == 4);
        }
    }
}