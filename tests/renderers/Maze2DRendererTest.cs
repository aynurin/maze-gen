using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.MapFilters;
using PlayersWorlds.Maps.Maze;
using PlayersWorlds.Maps.Maze.PostProcessing;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps {
    [TestFixture]
    public class Maze2DRendererTest : Test {
        private Maze2D _maze;

        [SetUp]
        public void SetUp() {
            _maze = Maze2D.Parse("4x4;0:1,4;1:2,5;2:3;3:7;4:5,8;8:12;12:13;13:14;14:10;10:11");
        }

        [Test]
        public void CanRenderAMapWithSmoothCorners() {
            var log = TestLog.CreateForThisTest();
            var mazeRenderingOptions = new MazeToMapOptions(
                trailWidths: new int[] { 1, 2, 1, 2 },
                trailHeights: new int[] { 2, 1, 2, 1 },
                wallWidths: new int[] { 1, 2, 1, 2, 1 },
                wallHeights: new int[] { 2, 1, 2, 1, 2 });
            var map = CreateMapForMaze(_maze, mazeRenderingOptions);

            new Maze2DRenderer(_maze, mazeRenderingOptions)
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail }, Cell.CellTag.MazeWall, 1, 1))
                .With(new Map2DSmoothCorners(Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner, 1, 1))
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner }, Cell.CellTag.MazeWall, 1, 1))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeVoid }, true, Cell.CellTag.MazeWall, 4, 4))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeWall, Cell.CellTag.MazeWallCorner }, false, Cell.CellTag.MazeTrail, 3, 3))
                .Render(map);
            var expected =
                "0000000000000\n" +
                "▓▓▓▓▓▓▓▓▓0000\n" +
                "▓░░░░░░░▓▓000\n" +
                "▓░▒▓▓▓▒░▒▓▓▓▓\n" +
                "▓░▓▓▓▓▓░░░░░▓\n" +
                "▓░▓▓▓▓▓░░░░░▓\n" +
                "▓░▓▓▓▓▓▓▓▓▓▓▓\n" +
                "▓░▒▓▓▓▓▓▓▓▓▓▓\n" +
                "▓░░░░░▓▓▓▓░░▓\n" +
                "▓░░░░░▒▓▓▒░░▓\n" +
                "▓░░░░░░░░░░░▓\n" +
                "▓░░░░░░░░░░░▓\n" +
                "▓▓▓▓▓▓▓▓▓▓▓▓▓\n" +
                "0000000000000\n";
            var actual = map.ToString();
            log.D(5, expected);
            log.D(5, actual);
            Assert.That(expected, Is.EqualTo(actual));
        }

        [Test]
        public void CanRenderAMapWithVoids() {
            var log = TestLog.CreateForThisTest();
            var mazeRenderingOptions = new MazeToMapOptions(
                trailWidths: new int[] { 2, 3, 3, 2 },
                trailHeights: new int[] { 1, 2, 1, 1 },
                wallWidths: new int[] { 2, 2, 2, 2, 2 },
                wallHeights: new int[] { 1, 2, 1, 2, 1 });
            var map = CreateMapForMaze(_maze, mazeRenderingOptions);

            new Maze2DRenderer(_maze, mazeRenderingOptions)
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail }, Cell.CellTag.MazeWall, 1, 1))
                .With(new Map2DSmoothCorners(Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner, 1, 1))
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner }, Cell.CellTag.MazeWall, 1, 1))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeVoid }, true, Cell.CellTag.MazeWall, 4, 4))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeWall, Cell.CellTag.MazeWallCorner }, false, Cell.CellTag.MazeTrail, 3, 3))
                .Render(map);

            var expected =
                "0▓▓▓▓▓▓▓▓▓▓▓▓▓▓00000\n" +
                "0▓░░░░░░░░░░░░▓00000\n" +
                "0▓░░▒▓▓▓▓▓▒░░░▓▓0000\n" +
                "0▓░░▓▓▓▓▓▓▓░░░▒▓▓▓▓0\n" +
                "0▓░░▓▓▓▓▓▓▓░░░░░░░▓0\n" +
                "0▓░░▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓0\n" +
                "0▓░░░░░░░▓00000▓░░▓0\n" +
                "0▓░░░░░░░▓00000▓░░▓0\n" +
                "0▓░░░░░░░▓▓000▓▓░░▓0\n" +
                "0▓░░░░░░░▒▓▓▓▓▓▒░░▓0\n" +
                "0▓░░░░░░░░░░░░░░░░▓0\n" +
                "0▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓0\n";
            var actual = map.ToString();
            log.D(5, expected);
            log.D(5, actual);
            Assert.That(expected, Is.EqualTo(actual));
        }

        [Test]
        public void CanRenderAMapWithFilledAreas() {
            var log = TestLog.CreateForThisTest();
            _maze.AddArea(
                MapArea.Create(
                    AreaType.Fill, new Vector(1, 1), new Vector(1, 1)));
            var builder = new Maze2DBuilder(_maze, new GeneratorOptions() { });
            builder.ApplyAreas();
            var mazeRenderingOptions = new MazeToMapOptions(
                trailWidths: new int[] { 2, 3, 3, 2 },
                trailHeights: new int[] { 1, 2, 1, 1 },
                wallWidths: new int[] { 2, 2, 2, 2, 2 },
                wallHeights: new int[] { 1, 2, 1, 2, 1 });
            var map = CreateMapForMaze(_maze, mazeRenderingOptions);

            new Maze2DRenderer(_maze, mazeRenderingOptions)
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail }, Cell.CellTag.MazeWall, 1, 1))
                .With(new Map2DSmoothCorners(Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner, 1, 1))
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner }, Cell.CellTag.MazeWall, 1, 1))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeVoid }, true, Cell.CellTag.MazeWall, 5, 5))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeWall, Cell.CellTag.MazeWallCorner }, false, Cell.CellTag.MazeTrail, 3, 3))
                .Render(map);

            var expected =
                "0▓▓▓▓▓▓▓▓▓▓▓▓▓▓00000\n" +
                "0▓░░░░░░░░░░░░▓00000\n" +
                "0▓░░▒▓▓▓▓▓▒░░░▓▓0000\n" +
                "0▓░░▓▓000▓▓░░░▒▓▓▓▓0\n" +
                "0▓░░▓00000▓░░░░░░░▓0\n" +
                "0▓░░▓00000▓▓▓▓▓▓▓▓▓0\n" +
                "0▓░░▓0000000000▓░░▓0\n" +
                "0▓░░▓0000000000▓░░▓0\n" +
                "0▓░░▓▓00000000▓▓░░▓0\n" +
                "0▓░░▒▓▓▓▓▓▓▓▓▓▓▒░░▓0\n" +
                "0▓░░░░░░░░░░░░░░░░▓0\n" +
                "0▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓0\n";
            var actual = map.ToString();
            log.D(5, expected);
            log.D(5, actual);
            Assert.That(expected, Is.EqualTo(actual));
        }

        [Test]
        public void CanRenderAMapWithHallAreas() {
            var log = TestLog.CreateForThisTest();
            _maze.AddArea(
                MapArea.Create(
                    AreaType.Hall, new Vector(0, 0), new Vector(4, 2)));
            var builder = new Maze2DBuilder(_maze, new GeneratorOptions() { });
            builder.ApplyAreas();
            var mazeRenderingOptions = new MazeToMapOptions(
                trailWidths: new int[] { 2, 3, 3, 2 },
                trailHeights: new int[] { 1, 2, 1, 1 },
                wallWidths: new int[] { 2, 2, 2, 2, 2 },
                wallHeights: new int[] { 1, 2, 1, 2, 1 });
            var map = CreateMapForMaze(_maze, mazeRenderingOptions);

            new Maze2DRenderer(_maze, mazeRenderingOptions)
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail }, Cell.CellTag.MazeWall, 1, 1))
                .With(new Map2DSmoothCorners(Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner, 1, 1))
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner }, Cell.CellTag.MazeWall, 1, 1))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeVoid }, true, Cell.CellTag.MazeWall, 5, 5))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeWall, Cell.CellTag.MazeWallCorner }, false, Cell.CellTag.MazeTrail, 3, 3))
                .Render(map);

            var expected =
                "0▓▓▓▓▓▓▓▓▓▓▓▓▓▓00000\n" +
                "0▓░░░░░░░░░░░░▓00000\n" +
                "0▓░░▒▓▓▓▓▓▒░░░▓▓0000\n" +
                "0▓░░▓▓▓▓▓▓▓░░░▒▓▓▓▓0\n" +
                "0▓░░▓▓▓▓▓▓▓░░░░░░░▓0\n" +
                "0▓░░▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓0\n" +
                "0▓░░░░░░░░░░░░░░░░▓0\n" +
                "0▓░░░░░░░░░░░░░░░░▓0\n" +
                "0▓░░░░░░░░░░░░░░░░▓0\n" +
                "0▓░░░░░░░░░░░░░░░░▓0\n" +
                "0▓░░░░░░░░░░░░░░░░▓0\n" +
                "0▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓0\n";
            log.D(5, expected);
            var actual = map.ToString();
            log.D(5, actual);
            Assert.That(expected, Is.EqualTo(actual));
        }

        [Test]
        public void Maze2DStringBoxRenderer_CanConvertToAscii() {
            var log = TestLog.CreateForThisTest();
            var expected =
                "┌───────────┐    \n" +
                "│           │    \n" +
                "│   ┌───┐   └───┐\n" +
                "│   │   │       │\n" +
                "│   └───┼───┬───┤\n" +
                "│       │   │   │\n" +
                "│   ┼   └───┘   │\n" +
                "│               │\n" +
                "└───────────────┘\n";
            log.D(5, _maze.ToString());
            Assert.That(expected, Is.EqualTo(_maze.ToString()));
        }

        [Test]
        public void Maze2DStringBoxRenderer_CanConvertToAsciiWithData() {
            var log = TestLog.CreateForThisTest();
            var expected =
                "┌───────────┐    \n" +
                "│ 4   3   2 │    \n" +
                "│   ┌───┐   └───┐\n" +
                "│ 5 │   │ 1   0 │\n" +
                "│   └───┼───┬───┤\n" +
                "│ 6     │   │ b │\n" +
                "│   ┼   └───┘   │\n" +
                "│ 7   8   9   a │\n" +
                "└───────────────┘\n";
            _maze.Attributes.Set(DeadEnd.DeadEndAttribute, DeadEnd.Find(_maze));
            _maze.Attributes.Set(DijkstraDistance.LongestTrailAttribute,
                DijkstraDistance.FindLongestTrail(_maze));
            log.D(5, _maze.ToString());
            Assert.That(expected, Is.EqualTo(_maze.ToString()));
        }

        [Test]
        public void Maze2DToMap2DConverter_ThrowsIfInvalidOptions() {
            Assert.Throws<ArgumentException>(() => MazeToMapOptions.RectCells(new Vector(1, 2), new Vector(3, 0)));
            Assert.Throws<ArgumentException>(() => MazeToMapOptions.RectCells(new Vector(1, -2), new Vector(3, 4)));
            Assert.Throws<ArgumentException>(() => MazeToMapOptions.SquareCells(1, -2));
            Assert.Throws<ArgumentException>(() => MazeToMapOptions.SquareCells(0, 2));
            var mazeRenderingOptions = new MazeToMapOptions(
                trailWidths: new int[] { 1, 2, 1, 1 },
                trailHeights: new int[] { 2, 2, 3, 2 },
                wallWidths: new int[] { 1, 2, 1, 2, 1 },
                wallHeights: new int[] { 2, 3, 2, 2, 2 });
            Assert.DoesNotThrow(() => mazeRenderingOptions.ThrowIfWrong(_maze.Size));
            Assert.Throws<ArgumentException>(() => mazeRenderingOptions.ThrowIfWrong(_maze.Size + Vector.East2D));
        }

        [Test]
        public void CellsMapping_ValidMapping() {
            // The goal is to make sure the cellMapping *Cells properties return
            // valid groups of cells
            // how to validate? make sure x,y matches expected values for all
            // returned cells
            // Assert.XYIn(expected, actual)
            // Assert.XIn([1, 2], cellMapping.NWCells.Select(cell => CellPosition(map, cell)))
            // Assert.YIn([1, 2], cellMapping.NWCells.Select(cell => CellPosition(map, cell)))

            var mazeToMapOptions = new MazeToMapOptions(
                trailWidths: new int[] { 1, 2, 1, 2 },
                trailHeights: new int[] { 2, 1, 2, 1 },
                wallWidths: new int[] { 1, 2, 1, 2, 1 },
                wallHeights: new int[] { 2, 1, 2, 1, 2 });
            var map = CreateMapForMaze(_maze, mazeToMapOptions);
            var mazeToMap = new Maze2DRenderer(_maze, mazeToMapOptions);
            var cellMapping = new CellsMapping(map, _maze.Cells[0], mazeToMapOptions);

            Assert.That(cellMapping.SWPosition, Is.EqualTo(new Vector(0, 0)), "SWPosition");
            Assert.That(cellMapping.SWSize, Is.EqualTo(new Vector(1, 2)), "SWSize");
            Assert.That(cellMapping.CenterPosition, Is.EqualTo(new Vector(1, 2)), "CenterPosition");
            Assert.That(cellMapping.CenterSize, Is.EqualTo(new Vector(1, 2)), "CenterSize");
            Assert.That(cellMapping.NEPosition, Is.EqualTo(new Vector(2, 4)), "NEPosition");
            Assert.That(cellMapping.NESize, Is.EqualTo(new Vector(2, 1)), "NESize");

            Assert.That(cellMapping.NWPosition, Is.EqualTo(new Vector(0, 4)), "NWPosition");
            Assert.That(cellMapping.NWSize, Is.EqualTo(new Vector(1, 1)), "NWSize");
            Assert.That(cellMapping.NPosition, Is.EqualTo(new Vector(1, 4)), "NPosition");
            Assert.That(cellMapping.NSize, Is.EqualTo(new Vector(1, 1)), "NSize");
            Assert.That(cellMapping.WPosition, Is.EqualTo(new Vector(0, 2)), "WPosition");
            Assert.That(cellMapping.WSize, Is.EqualTo(new Vector(1, 2)), "WSize");
            Assert.That(cellMapping.EPosition, Is.EqualTo(new Vector(2, 2)), "EPosition");
            Assert.That(cellMapping.ESize, Is.EqualTo(new Vector(2, 2)), "ESize");
            Assert.That(cellMapping.SPosition, Is.EqualTo(new Vector(1, 0)), "SPosition");
            Assert.That(cellMapping.SSize, Is.EqualTo(new Vector(1, 2)), "SSize");
            Assert.That(cellMapping.SEPosition, Is.EqualTo(new Vector(2, 0)), "SEPosition");
            Assert.That(cellMapping.SESize, Is.EqualTo(new Vector(2, 2)), "SESize");

            Assert.That(cellMapping.CenterCells.ToList(), Has.Count.EqualTo(2), "CenterCells Count");
            Assert.That(cellMapping.CenterCells.Select(cell => map.Cells.IndexOf(cell).X), Has.All.AnyOf(1), "CenterCells.X");
            Assert.That(cellMapping.CenterCells.Select(cell => map.Cells.IndexOf(cell).Y), Has.All.AnyOf(2, 3), "CenterCells.Y");

            Assert.That(cellMapping.NWCells.ToList(), Has.Count.EqualTo(1), "NWCells Count");
            Assert.That(cellMapping.NWCells.Select(cell => map.Cells.IndexOf(cell).X), Has.All.AnyOf(0), "NWCells.X");
            Assert.That(cellMapping.NWCells.Select(cell => map.Cells.IndexOf(cell).Y), Has.All.AnyOf(4), "NWCells.Y");

            Assert.That(cellMapping.NCells.ToList(), Has.Count.EqualTo(1), "NCells Count");
            Assert.That(cellMapping.NCells.Select(cell => map.Cells.IndexOf(cell).X), Has.All.AnyOf(1), "NCells.X");
            Assert.That(cellMapping.NCells.Select(cell => map.Cells.IndexOf(cell).Y), Has.All.AnyOf(4), "NCells.Y");

            Assert.That(cellMapping.NECells.ToList(), Has.Count.EqualTo(2), "NECells Count");
            Assert.That(cellMapping.NECells.Select(cell => map.Cells.IndexOf(cell).X), Has.All.AnyOf(2, 3), "NECells.X");
            Assert.That(cellMapping.NECells.Select(cell => map.Cells.IndexOf(cell).Y), Has.All.AnyOf(4), "NECells.Y");

            Assert.That(cellMapping.WCells.ToList(), Has.Count.EqualTo(2), "WCells Count");
            Assert.That(cellMapping.WCells.Select(cell => map.Cells.IndexOf(cell).X), Has.All.AnyOf(0), "WCells.X");
            Assert.That(cellMapping.WCells.Select(cell => map.Cells.IndexOf(cell).Y), Has.All.AnyOf(2, 3), "WCells.Y");

            Assert.That(cellMapping.ECells.ToList(), Has.Count.EqualTo(4), "ECells Count");
            Assert.That(cellMapping.ECells.Select(cell => map.Cells.IndexOf(cell).X), Has.All.AnyOf(2, 3), "ECells.X");
            Assert.That(cellMapping.ECells.Select(cell => map.Cells.IndexOf(cell).Y), Has.All.AnyOf(2, 3), "ECells.Y");

            Assert.That(cellMapping.SWCells.ToList(), Has.Count.EqualTo(2), "SWCells Count");
            Assert.That(cellMapping.SWCells.Select(cell => map.Cells.IndexOf(cell).X), Has.All.AnyOf(0), "SWCells.X");
            Assert.That(cellMapping.SWCells.Select(cell => map.Cells.IndexOf(cell).Y), Has.All.AnyOf(0, 1), "SWCells.Y");

            Assert.That(cellMapping.SCells.ToList(), Has.Count.EqualTo(2), "SCells Count");
            Assert.That(cellMapping.SCells.Select(cell => map.Cells.IndexOf(cell).X), Has.All.AnyOf(1), "SCells.X");
            Assert.That(cellMapping.SCells.Select(cell => map.Cells.IndexOf(cell).Y), Has.All.AnyOf(0, 1), "SCells.Y");

            Assert.That(cellMapping.SECells.ToList(), Has.Count.EqualTo(4), "SECells Count");
            Assert.That(cellMapping.SECells.Select(cell => map.Cells.IndexOf(cell).X), Has.All.AnyOf(2, 3), "SECells.X");
            Assert.That(cellMapping.SECells.Select(cell => map.Cells.IndexOf(cell).Y), Has.All.AnyOf(0, 1), "SECells.Y");
        }
    }
}