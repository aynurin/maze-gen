using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze.PostProcessing;
using PlayersWorlds.Maps.Serializer;

namespace PlayersWorlds.Maps.Maze {
    [TestFixture]
    public class Maze2DTest : Test {
        [Test]
        public void Maze2D_IsInitialized() {
            var map = Area.CreateEnvironment(new Vector(2, 3));
            Assert.That(6, Is.EqualTo(map.Size.Area));
            Assert.That(6, Is.EqualTo(map.Cells.Count));
            Assert.That(2, Is.EqualTo(map.Size.X));
            Assert.That(3, Is.EqualTo(map.Size.Y));
        }

        [Test]
        public void Maze2D_WrongSize() {
            Assert.DoesNotThrow(() => Area.CreateEnvironment(new Vector(0, 3)));
            Assert.DoesNotThrow(() => Area.CreateEnvironment(new Vector(2, 0)));
            Assert.Throws<ArgumentException>(() => Area.CreateEnvironment(new Vector(-1, 1)));
            Assert.Throws<ArgumentException>(() => Area.CreateEnvironment(new Vector(1, -1)));
            Assert.Throws<ArgumentException>(() => Area.CreateEnvironment(new Vector(-1, -1)));
        }

        [Test]
        public void Maze2D_ToMapWrongOptions() {
            Assert.DoesNotThrow(() => Area.CreateEnvironment(new Vector(2, 3)).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 3, 4 })));
            Assert.Throws<ArgumentException>(() => Area.CreateEnvironment(new Vector(2, 3)).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 2 }, new int[] { 1, 2, 3 }, new int[] { 1 }, new int[] { 1, 2 })));
            Assert.Throws<ArgumentException>(() => Area.CreateEnvironment(new Vector(2, 3)).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 3 }, new int[] { 1 }, new int[] { 1, 2 })));
            Assert.Throws<ArgumentException>(() => Area.CreateEnvironment(new Vector(2, 3)).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { }, new int[] { 1, 2 })));
            Assert.DoesNotThrow(() => Area.CreateEnvironment(new Vector(2, 3)).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { 1 }, new int[] { 1 })));
            Assert.Throws<ArgumentException>(() => Area.CreateEnvironment(new Vector(2, 3)).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { 1 }, new int[] { })));
            Assert.Throws<ArgumentNullException>(() => Area.CreateEnvironment(new Vector(2, 3)).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 1, 2 }, null)));
        }

        [Test]
        public void Maze2D_CellsNeighborsAreValid() {
            var rows = 5;
            var cols = 5;
            var map = Area.CreateEnvironment(new Vector(cols, rows));

            Assert.That(map.Cells.Count, Is.EqualTo(rows * cols), "Wrong number of cells.");
            for (var x = 0; x < cols; x++) {
                for (var y = 0; y < rows; y++) {
                    var cell = new Vector(x, y);
                    var neighbors = map.Cells.Where(c =>
                        (c.X == cell.X && Math.Abs(c.Y - cell.Y) == 1) ||
                        (c.Y == cell.Y && Math.Abs(c.X - cell.X) == 1))
                            .ToList();
                    var nonNeighbors = map.Cells
                        .Where(c => c != cell && !neighbors.Contains(c))
                        .ToList();
                    if (neighbors.Count != map.NeighborsOf(cell).Count()) {
                        TestLog.CreateForThisTest().D(5, $"Cell {cell.X},{cell.Y} has {neighbors.Count} neighbors. It should have {map.NeighborsOf(cell).Count()} neighbors.");
                    }
                    Assert.That(neighbors.Count, Is.EqualTo(map.NeighborsOf(cell).Count()));
                    Assert.That(neighbors.All(c => map.NeighborsOf(cell).Contains(c)), Is.True);
                    Assert.That(nonNeighbors.Any(c => map.NeighborsOf(cell).Contains(c)), Is.False);
                }
            }
        }

        [Test]
        public void Maze2D_CanRenderMap() {
            var map = MazeTestHelper.GenerateMaze(new Vector(3, 3), null,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full
                });
            var scaledMap = map.ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 2, 3, 2 }, new int[] { 2, 3, 2 }, new int[] { 1, 2, 1, 2 }, new int[] { 1, 3, 1, 3 }));

            var expectedSize = new Vector(13, 15);

            Assert.That(expectedSize.Area, Is.EqualTo(scaledMap.Cells.Count));
        }

        [Test]
        public void Maze2D_AddsNoRoomsWhenNoneRequested() {
            var maze = MazeTestHelper.GenerateMaze(new Vector(10, 10), null,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full,
                    AreaGeneration = GeneratorOptions.AreaGenerationMode.Manual,
                });
            Assert.That(maze.ChildAreas().Count, Is.EqualTo(0));
        }

        [Test]
        public void Maze2D_AddsNoRoomsToASmallMaze() {
            var maze = MazeTestHelper.GenerateMaze(new Vector(3, 3), null,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full,
                    AreaGeneration = GeneratorOptions.AreaGenerationMode.Auto,
                });
            Assert.That(maze.ChildAreas().Count, Is.EqualTo(0));
        }

        [Test]
        public void Maze2D_AddsRooms() {
            var maze = MazeTestHelper.GenerateMaze(new Vector(5, 5), null,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full,
                    AreaGeneration = GeneratorOptions.AreaGenerationMode.Auto,
                });
            Assert.That(maze.ChildAreas().Count, Is.GreaterThan(0));
        }

        [Test]
        public void Maze2D_AddsOneHall() {
            var options = new GeneratorOptions() {
                MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                FillFactor = GeneratorOptions.MazeFillFactor.Full,
                AreaGeneration = GeneratorOptions.AreaGenerationMode.Manual,
            };
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(5, 5), new List<Area> { LegacyAreaSerializer.ParseV01AreaString("P2x1;S2x2;Hall") }, options);
            Assert.That(maze.ChildAreas().Count, Is.EqualTo(1));
        }

        [Test]
        public void Maze2D_AddsOneFill() {
            var options = new GeneratorOptions() {
                MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                FillFactor = GeneratorOptions.MazeFillFactor.Full,
                AreaGeneration = GeneratorOptions.AreaGenerationMode.Manual
            };
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(5, 5), new List<Area> { LegacyAreaSerializer.ParseV01AreaString("P2x1;S2x2;Fill") }, options);
            Assert.That(maze.ChildAreas().Count, Is.EqualTo(1));
        }

        [Test]
        public void Maze2D_HasExtensionsSet() {
            var map = MazeTestHelper.GenerateMaze(new Vector(3, 3), null,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full
                });
            Assert.That(map.X<DeadEnd.DeadEndsExtension>(), Is.Not.Null);
            Assert.That(map.X<DijkstraDistance.LongestTrailExtension>(), Is.Not.Null);
        }

        [Test]
        public void Maze2D_CanParse() {
            var maze = LegacyAreaSerializer.ParseV01MazeString("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            TestLog.CreateForThisTest().D(5, maze.ToString());
            // ╔═══╤═══════╗
            // ║ 0 │ 1   2 ║
            // ║   ╵   ╷   ║
            // ║ 3   4 │ 5 ║
            // ╟───╴   └───╢
            // ║ 6   7   8 ║
            // ╚═══════════╝
            Assert.That(!maze.CellsAreLinked(Vector.FromIndex(0, maze.Size), Vector.FromIndex(0, maze.Size) + Vector.East2D), Is.True);
            Assert.That(!maze.CellsAreLinked(Vector.FromIndex(4, maze.Size), Vector.FromIndex(4, maze.Size) + Vector.East2D), Is.True);
            Assert.That(maze.CellsAreLinked(Vector.FromIndex(4, maze.Size), Vector.FromIndex(4, maze.Size) + Vector.South2D), Is.True);
            Assert.That(maze.CellsAreLinked(Vector.FromIndex(4, maze.Size), Vector.FromIndex(4, maze.Size) + Vector.North2D), Is.True);
            Assert.That(maze.CellsAreLinked(Vector.FromIndex(7, maze.Size), Vector.FromIndex(7, maze.Size) + Vector.East2D), Is.True);
            Assert.That(maze.CellsAreLinked(Vector.FromIndex(7, maze.Size), Vector.FromIndex(7, maze.Size) + Vector.West2D), Is.True);
            Assert.That(maze.CellsAreLinked(Vector.FromIndex(7, maze.Size), Vector.FromIndex(7, maze.Size) + Vector.South2D), Is.True);
            Assert.That(!maze.CellsAreLinked(Vector.FromIndex(7, maze.Size), Vector.FromIndex(7, maze.Size) + Vector.North2D), Is.True);
            Assert.That(maze.CellsAreLinked(Vector.FromIndex(6, maze.Size), Vector.FromIndex(6, maze.Size) + Vector.East2D), Is.True);
            Assert.That(!maze.CellsAreLinked(Vector.FromIndex(6, maze.Size), Vector.FromIndex(6, maze.Size) + Vector.West2D), Is.True);
            Assert.That(!maze.CellsAreLinked(Vector.FromIndex(6, maze.Size), Vector.FromIndex(6, maze.Size) + Vector.South2D), Is.True);
            Assert.That(!maze.CellsAreLinked(Vector.FromIndex(6, maze.Size), Vector.FromIndex(6, maze.Size) + Vector.North2D), Is.True);
            Assert.That(maze.AreNeighbors(Vector.FromIndex(6, maze.Size), Vector.FromIndex(6, maze.Size) + Vector.East2D), Is.True);
            Assert.That(!maze.AreNeighbors(Vector.FromIndex(6, maze.Size), Vector.FromIndex(6, maze.Size) + Vector.West2D), Is.True);
            Assert.That(maze.AreNeighbors(Vector.FromIndex(6, maze.Size), Vector.FromIndex(6, maze.Size) + Vector.South2D), Is.True);
            Assert.That(!maze.AreNeighbors(Vector.FromIndex(6, maze.Size), Vector.FromIndex(6, maze.Size) + Vector.North2D), Is.True);
        }

        [Test]
        public void Maze2D_CanParse2() {
            var maze = LegacyAreaSerializer.ParseV01MazeString("3x4");
            Assert.That(maze.Size.X == 3 && maze.Size.Y == 4, Is.True);
        }
    }
}