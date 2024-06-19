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
            var map = Area.CreateMaze(new Vector(2, 3));
            Assert.That(6, Is.EqualTo(map.Size.Area));
            Assert.That(6, Is.EqualTo(map.Count));
            Assert.That(2, Is.EqualTo(map.Size.X));
            Assert.That(3, Is.EqualTo(map.Size.Y));
        }

        [Test]
        public void Maze2D_WrongSize() {
            Assert.DoesNotThrow(() => Area.CreateMaze(new Vector(0, 3)));
            Assert.DoesNotThrow(() => Area.CreateMaze(new Vector(2, 0)));
            Assert.Throws<ArgumentException>(() => Area.CreateMaze(new Vector(-1, 1)));
            Assert.Throws<ArgumentException>(() => Area.CreateMaze(new Vector(1, -1)));
            Assert.Throws<ArgumentException>(() => Area.CreateMaze(new Vector(-1, -1)));
        }

        [Test]
        public void Maze2D_ToMapWrongOptions() {
            Assert.DoesNotThrow(() => Area.CreateMaze(new Vector(2, 3)).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 3, 4 })));
            Assert.Throws<ArgumentException>(() => Area.CreateMaze(new Vector(2, 3)).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 2 }, new int[] { 1, 2, 3 }, new int[] { 1 }, new int[] { 1, 2 })));
            Assert.Throws<ArgumentException>(() => Area.CreateMaze(new Vector(2, 3)).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 3 }, new int[] { 1 }, new int[] { 1, 2 })));
            Assert.Throws<ArgumentException>(() => Area.CreateMaze(new Vector(2, 3)).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { }, new int[] { 1, 2 })));
            Assert.DoesNotThrow(() => Area.CreateMaze(new Vector(2, 3)).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { 1 }, new int[] { 1 })));
            Assert.Throws<ArgumentException>(() => Area.CreateMaze(new Vector(2, 3)).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2 }, new int[] { 1, 2, 3 }, new int[] { 1 }, new int[] { })));
            Assert.Throws<ArgumentNullException>(() => Area.CreateMaze(new Vector(2, 3)).ToMap(new Maze2DRenderer.MazeToMapOptions(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, new int[] { 1, 2, 1, 2 }, null)));
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

            Assert.That(expectedSize.Area, Is.EqualTo(scaledMap.Count));
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
            var random = RandomSource.CreateFromEnv();
            var maze = MazeTestHelper.GenerateMaze(new Vector(3, 3), null,
                new GeneratorOptions() {
                    RandomSource = random,
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full,
                    AreaGeneration = GeneratorOptions.AreaGenerationMode.Auto,
                    AreaGenerator = new RandomAreaGenerator(random)
                });
            Assert.That(maze.ChildAreas().Count, Is.EqualTo(0));
        }

        [Test]
        public void Maze2D_AddsRooms() {
            var random = RandomSource.CreateFromEnv();
            var maze = MazeTestHelper.GenerateMaze(new Vector(5, 5), null,
                new GeneratorOptions() {
                    RandomSource = random,
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full,
                    AreaGeneration = GeneratorOptions.AreaGenerationMode.Auto,
                    AreaGenerator = new RandomAreaGenerator(random)
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
            Assert.That(!maze[Vector.FromIndex(0, maze.Size)].HasLinks(Vector.FromIndex(0, maze.Size) + Vector.East2D), Is.True);
            Assert.That(!maze[Vector.FromIndex(4, maze.Size)].HasLinks(Vector.FromIndex(4, maze.Size) + Vector.East2D), Is.True);
            Assert.That(maze[Vector.FromIndex(4, maze.Size)].HasLinks(Vector.FromIndex(4, maze.Size) + Vector.South2D), Is.True);
            Assert.That(maze[Vector.FromIndex(4, maze.Size)].HasLinks(Vector.FromIndex(4, maze.Size) + Vector.North2D), Is.True);
            Assert.That(maze[Vector.FromIndex(7, maze.Size)].HasLinks(Vector.FromIndex(7, maze.Size) + Vector.East2D), Is.True);
            Assert.That(maze[Vector.FromIndex(7, maze.Size)].HasLinks(Vector.FromIndex(7, maze.Size) + Vector.West2D), Is.True);
            Assert.That(maze[Vector.FromIndex(7, maze.Size)].HasLinks(Vector.FromIndex(7, maze.Size) + Vector.South2D), Is.True);
            Assert.That(!maze[Vector.FromIndex(7, maze.Size)].HasLinks(Vector.FromIndex(7, maze.Size) + Vector.North2D), Is.True);
            Assert.That(maze[Vector.FromIndex(6, maze.Size)].HasLinks(Vector.FromIndex(6, maze.Size) + Vector.East2D), Is.True);
            Assert.That(!maze[Vector.FromIndex(6, maze.Size)].HasLinks(Vector.FromIndex(6, maze.Size) + Vector.West2D), Is.True);
            Assert.That(!maze[Vector.FromIndex(6, maze.Size)].HasLinks(Vector.FromIndex(6, maze.Size) + Vector.South2D), Is.True);
            Assert.That(!maze[Vector.FromIndex(6, maze.Size)].HasLinks(Vector.FromIndex(6, maze.Size) + Vector.North2D), Is.True);
        }

        [Test]
        public void Maze2D_CanParse2() {
            var maze = LegacyAreaSerializer.ParseV01MazeString("3x4");
            Assert.That(maze.Size.X == 3 && maze.Size.Y == 4, Is.True);
        }
    }
}