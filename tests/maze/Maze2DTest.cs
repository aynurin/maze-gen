using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze.PostProcessing;

namespace PlayersWorlds.Maps.Maze {
    [TestFixture]
    public class Maze2DTest : Test {
        [Test]
        public void Maze2D_IsInitialized() {
            var map = new Maze2D(2, 3);
            Assert.That(6, Is.EqualTo(map.Size.Area));
            Assert.That(6, Is.EqualTo(map.Cells.Count));
            Assert.That(2, Is.EqualTo(map.Size.X));
            Assert.That(3, Is.EqualTo(map.Size.Y));
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
        public void Maze2D_AllowsOnly2D() {
            Assert.Throws<NotImplementedException>(() => new Maze2D(
                new Vector(new int[] { 1, 2, 3 })));
        }

        [Test]
        public void Maze2D_ToMapWrongOptions() {
            Assert.DoesNotThrow(() => new Maze2D(2, 3).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 3, 4 })));
            Assert.Throws<ArgumentException>(() => new Maze2D(2, 3).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 2 }, new int[] { 1, 2, 3 }, new int[] { 1 }, new int[] { 1, 2 })));
            Assert.Throws<ArgumentException>(() => new Maze2D(2, 3).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 3 }, new int[] { 1 }, new int[] { 1, 2 })));
            Assert.Throws<ArgumentException>(() => new Maze2D(2, 3).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { }, new int[] { 1, 2 })));
            Assert.DoesNotThrow(() => new Maze2D(2, 3).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { 1 }, new int[] { 1 })));
            Assert.Throws<ArgumentException>(() => new Maze2D(2, 3).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { 1 }, new int[] { })));
            Assert.Throws<ArgumentNullException>(() => new Maze2D(2, 3).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 1, 2 }, null)));
        }

        [Test]
        public void Maze2D_CellsNeighborsAreValid() {
            var rows = 5;
            var cols = 5;
            var map = new Maze2D(cols, rows);

            Assert.That(map.Cells.Count, Is.EqualTo(rows * cols), "Wrong number of cells.");
            for (var x = 0; x < cols; x++) {
                for (var y = 0; y < rows; y++) {
                    var cell = map.Cells[new Vector(x, y)];
                    var neighbors = map.Cells.Where(c =>
                        (c.X == cell.X && Math.Abs(c.Y - cell.Y) == 1) ||
                        (c.Y == cell.Y && Math.Abs(c.X - cell.X) == 1))
                            .ToList();
                    var nonNeighbors = map.Cells
                        .Where(c => c != cell && !neighbors.Contains(c))
                        .ToList();
                    if (neighbors.Count != cell.Neighbors().Count) {
                        TestLog.CreateForThisTest().D(5, $"Cell {cell.X},{cell.Y} has {neighbors.Count} neighbors. It should have {cell.Neighbors().Count} neighbors.");
                    }
                    Assert.That(neighbors.Count, Is.EqualTo(cell.Neighbors().Count));
                    Assert.That(neighbors.All(c => cell.Neighbors().Contains(c)), Is.True);
                    Assert.That(nonNeighbors.Any(c => cell.Neighbors().Contains(c)), Is.False);

                    if (x > 0) Assert.That(
                        cell.Neighbors(Vector.West2D).Value,
                        Is.EqualTo(map.Cells[new Vector(x - 1, y)]));
                    if (x + 1 < cols) Assert.That(
                        cell.Neighbors(Vector.East2D).Value,
                        Is.EqualTo(map.Cells[new Vector(x + 1, y)]));
                    if (y > 0) Assert.That(
                        cell.Neighbors(Vector.South2D).Value,
                        Is.EqualTo(map.Cells[new Vector(x, y - 1)]));
                    if (y + 1 > rows) Assert.That(
                        cell.Neighbors(Vector.North2D).Value,
                        Is.EqualTo(map.Cells[new Vector(x, y + 1)]));
                }
            }
        }

        [Test]
        public void Maze2D_CanRenderMap() {
            var map = MazeTestHelper.GenerateMaze(new Vector(3, 3),
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.Full
                });
            var scaledMap = map.ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 2, 3, 2 }, new int[] { 2, 3, 2 }, new int[] { 1, 2, 1, 2 }, new int[] { 1, 3, 1, 3 }));

            var expectedSize = new Vector(13, 15);

            Assert.That(expectedSize.Area, Is.EqualTo(scaledMap.Cells.Count));
        }

        [Test]
        public void Maze2D_AddsNoRoomsWhenNoneRequested() {
            var maze = MazeTestHelper.GenerateMaze(new Vector(10, 10),
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.Full,
                    MapAreasOptions = GeneratorOptions.MapAreaOptions.Manual,
                });
            Assert.That(maze.MapAreas.Count, Is.EqualTo(0));
        }

        [Test]
        public void Maze2D_AddsNoRoomsToASmallMaze() {
            var maze = MazeTestHelper.GenerateMaze(new Vector(3, 3),
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.Full,
                    MapAreasOptions = GeneratorOptions.MapAreaOptions.Auto,
                });
            Assert.That(maze.MapAreas.Count, Is.EqualTo(0));
        }

        [Test]
        public void Maze2D_AddsRooms() {
            var maze = MazeTestHelper.GenerateMaze(new Vector(5, 5),
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.Full,
                    MapAreasOptions = GeneratorOptions.MapAreaOptions.Auto,
                });
            Assert.That(maze.MapAreas.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Maze2D_AddsOneHall() {
            var options = new GeneratorOptions() {
                Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                FillFactor = GeneratorOptions.FillFactorOption.Full,
                MapAreasOptions = GeneratorOptions.MapAreaOptions.Manual,
                MapAreas = new List<MapArea> { MapArea.Parse("P2x1;S2x2;Hall") }
            };
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(5, 5), options);
            Assert.That(maze.MapAreas.Count, Is.EqualTo(1));
        }

        [Test]
        public void Maze2D_AddsOneFill() {
            var options = new GeneratorOptions() {
                Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                FillFactor = GeneratorOptions.FillFactorOption.Full,
                MapAreasOptions = GeneratorOptions.MapAreaOptions.Manual,
                MapAreas = new List<MapArea> { MapArea.Parse("P2x1;S2x2;Fill") }
            };
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(5, 5), options);
            Assert.That(maze.MapAreas.Count, Is.EqualTo(1));
        }

        [Test]
        public void Maze2D_HasAttributesSet() {
            var map = MazeTestHelper.GenerateMaze(new Vector(3, 3),
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.Full
                });
            Assert.That(map.Attributes[DeadEnd.DeadEndAttribute], Is.Not.Empty);
            Assert.That(map.Attributes[DijkstraDistance.LongestTrailAttribute], Is.Not.Empty);
        }

        [Test]
        public void Maze2D_CanParse() {
            var maze = Maze2D.Parse("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            TestLog.CreateForThisTest().D(5, maze.ToString());
            // ╔═══╤═══════╗
            // ║ 0 │ 1   2 ║
            // ║   ╵   ╷   ║
            // ║ 3   4 │ 5 ║
            // ╟───╴   └───╢
            // ║ 6   7   8 ║
            // ╚═══════════╝
            Assert.That(!maze.Cells[0].Links(Vector.East2D).HasValue, Is.True);
            Assert.That(!maze.Cells[4].Links(Vector.East2D).HasValue, Is.True);
            Assert.That(maze.Cells[4].Links(Vector.South2D).HasValue, Is.True);
            Assert.That(maze.Cells[4].Links(Vector.North2D).HasValue, Is.True);
            Assert.That(maze.Cells[7].Links(Vector.East2D).HasValue, Is.True);
            Assert.That(maze.Cells[7].Links(Vector.West2D).HasValue, Is.True);
            Assert.That(maze.Cells[7].Links(Vector.South2D).HasValue, Is.True);
            Assert.That(!maze.Cells[7].Links(Vector.North2D).HasValue, Is.True);
            Assert.That(maze.Cells[6].Links(Vector.East2D).HasValue, Is.True);
            Assert.That(!maze.Cells[6].Links(Vector.West2D).HasValue, Is.True);
            Assert.That(!maze.Cells[6].Links(Vector.South2D).HasValue, Is.True);
            Assert.That(!maze.Cells[6].Links(Vector.North2D).HasValue, Is.True);
            Assert.That(maze.Cells[6].Neighbors(Vector.East2D).HasValue, Is.True);
            Assert.That(!maze.Cells[6].Neighbors(Vector.West2D).HasValue, Is.True);
            Assert.That(maze.Cells[6].Neighbors(Vector.South2D).HasValue, Is.True);
            Assert.That(!maze.Cells[6].Neighbors(Vector.North2D).HasValue, Is.True);
        }

        [Test]
        public void Maze2D_CanParse2() {
            var maze = Maze2D.Parse("3x4");
            Assert.That(maze.Size.X == 3 && maze.Size.Y == 4, Is.True);
        }
    }
}