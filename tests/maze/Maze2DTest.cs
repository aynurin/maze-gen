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
            Assert.DoesNotThrow(() => Area.CreateMaze(new Vector(2, 3)).ToMap(new Maze2DRenderer.Maze2DRendererOptions(new Vector(1, 1), new Vector(2, 2))));
            Assert.Throws<ArgumentException>(() => Area.CreateMaze(new Vector(2, 3)).ToMap(new Maze2DRenderer.Maze2DRendererOptions(new Vector(1, 1), new Vector(new int[] { 1, 2, 3 }))));
            Assert.Throws<ArgumentException>(() => Area.CreateMaze(new Vector(2, 3)).ToMap(new Maze2DRenderer.Maze2DRendererOptions(new Vector(1, 1), new Vector(0, 2))));
            Assert.Throws<ArgumentException>(() => Area.CreateMaze(new Vector(2, 3)).ToMap(new Maze2DRenderer.Maze2DRendererOptions(new Vector(1, 0), new Vector(2, 2))));
        }

        [Test]
        public void Maze2D_CanRenderMap() {
            var map = MazeTestHelper.GenerateMaze(new Vector(3, 3), null,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full
                });
            var scaledMap = map.ToMap(new Maze2DRenderer.Maze2DRendererOptions(new Vector(3, 2), new Vector(2, 1)));

            Assert.That(scaledMap.Size, Is.EqualTo(new Vector(17, 10)));
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
            // 
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(5, 5), new List<Area> { MazeTestHelper.Parse("Area:{2x2;2x1;False;Hall;;;}") }, options);
            Assert.That(maze.ChildAreas().Count, Is.EqualTo(1));
        }

        [Test]
        public void Maze2D_AddsOneFill() {
            var options = new GeneratorOptions() {
                MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                FillFactor = GeneratorOptions.MazeFillFactor.Full,
                AreaGeneration = GeneratorOptions.AreaGenerationMode.Manual
            };
            // 
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(5, 5), new List<Area> { MazeTestHelper.Parse("Area:{2x2;2x1;False;Fill;;;}") }, options);
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
            var maze = MazeTestHelper.Parse("Area:{3x3;0x0;False;Maze;;[Cell:{;[0x1];},Cell:{;[2x0,1x1];},Cell:{;[1x0,2x1];},Cell:{;[0x0,1x1];},Cell:{;[1x0,0x1,1x2];},Cell:{;[2x0];},Cell:{;[1x2];},Cell:{;[1x1,0x2,2x2];},Cell:{;[1x2];}];}");
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
            // Area:{3x4;0x0;False;Maze;;;}
            var maze = MazeTestHelper.Parse("Area:{3x4;0x0;False;Maze;;;}");
            Assert.That(maze.Size.X == 3 && maze.Size.Y == 4, Is.True);
        }
    }
}