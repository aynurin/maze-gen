using System;
using System.Collections.Generic;
using System.Linq;
using Nour.Play.MapFilters;
using Nour.Play.Maze;
using NUnit.Framework;
using static Nour.Play.Maze.Maze2DRenderer;

namespace Nour.Play {
    [TestFixture]
    public class Maze2DRendererTest {
        private Maze2D _maze;
        private Map2D _map;

        [SetUp]
        public void SetUp() {
            _maze = Maze2D.Parse("4x4;0:1,4;1:2,5;2:3;3:7;4:5,8;8:12;12:13;13:14;14:10;10:11");
            Console.WriteLine(_maze.ToString());
        }
        [Test]
        public void Maze2DToMap2DConverter_CanGenerateMap() {
            var mazeRenderingOptions = new MazeToMapOptions(
                trailXWidths: new int[] { 1, 2, 1, 2 },
                trailYHeights: new int[] { 2, 1, 2, 1 },
                wallXWidths: new int[] { 1, 2, 1, 2, 1 },
                wallYHeights: new int[] { 2, 1, 2, 1, 2 });
            var map = CreateMapForMaze(mazeRenderingOptions);

            var mazeRenderer = new Maze2DRenderer(_maze, mazeRenderingOptions);
            mazeRenderer.Render(map);
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
            Console.WriteLine(actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Maze2DToMap2DConverter_CanGenerateMap2() {
            var mazeRenderingOptions = new MazeToMapOptions(
                trailXWidths: new int[] { 2, 3, 3, 2 },
                trailYHeights: new int[] { 1, 2, 1, 1 },
                wallXWidths: new int[] { 2, 2, 2, 2, 2 },
                wallYHeights: new int[] { 1, 2, 1, 2, 1 });
            var map = CreateMapForMaze(mazeRenderingOptions);

            var mazeRenderer = new Maze2DRenderer(_maze, mazeRenderingOptions);
            mazeRenderer.Render(map);
            Console.WriteLine(map.ToString());

            var expected =
                "0▓▓▓▓▓▓▓▓▓▓▓▓▓▓00000\n" +
                "0▓░░░░░░░░░░░░▓00000\n" +
                "0▓░░▒▓▓▓▓▓▒░░░▓▓0000\n" +
                "0▓░░▓▓▓▓▓▓▓░░░▒▓▓▓▓0\n" +
                "0▓░░▓▓▓▓▓▓▓░░░░░░░▓0\n" +
                "0▓░░▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓0\n" +
                "0▓░░░░░░░▓00000▓░░▓0\n" +
                "0▓░░░░░░░▓00000▓░░▓0\n" +
                "0▓░░░░░░░▓▓▓▓▓▓▓░░▓0\n" +
                "0▓░░░░░░░▒▓▓▓▓▓▒░░▓0\n" +
                "0▓░░░░░░░░░░░░░░░░▓0\n" +
                "0▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓0\n";
            var actual = map.ToString();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Maze2DAsciiBoxRenderer_CanConvertToAscii() {
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
            Console.WriteLine(_maze.ToString());
            Assert.AreEqual(expected, _maze.ToString());
        }

        [Test]
        public void Maze2DToMap2DConverter_ThrowsIfInvalidOptions() {
            var maze = Maze2D.Parse("4x4;0:1,4;1:2,5;2:3;3:7;4:5,8;8:12;12:13;13:14;14:10;10:11");
            Assert.Throws<ArgumentException>(() => new MazeToMapOptions(new int[] { 1 }, new int[] { 2 }, new int[] { 3 }, new int[] { 0 }));
            Assert.Throws<ArgumentException>(() => new MazeToMapOptions(new int[] { 1 }, new int[] { 2 }, new int[] { -3 }, new int[] { 4 }));
            Assert.Throws<ArgumentException>(() => new MazeToMapOptions(new int[] { 1 }, new int[] { -2 }, new int[] { 3 }, new int[] { 4 }));
            Assert.Throws<ArgumentException>(() => new MazeToMapOptions(new int[] { -1 }, new int[] { 2 }, new int[] { 3 }, new int[] { 4 }));
            Assert.DoesNotThrow(() => new MazeToMapOptions(new int[] { 1, 2, 1, 1 }, new int[] { 2, 2, 3, 2 }, new int[] { 1, 2, 1, 2, 1 }, new int[] { 2, 3, 2, 2, 2 }).ThrowIfWrong(maze));
            Assert.Throws<ArgumentException>(() => new MazeToMapOptions(new int[] { -1 }, new int[] { 2 }, new int[] { 3 }, new int[] { 4 }).ThrowIfWrong(maze));
        }

        [Test]
        public void CellsMapping_ValidMapping() {
            var maze = Maze2D.Parse("4x4;0:1,4;1:2,5;2:3;3:7;4:5,8;8:12;12:13;13:14;14:10;10:11");
            // Console.WriteLine(maze.ToString());
            // ╔═══════════════╗
            // ║0x0 1x0 2x0 3x0║
            // ║   ┼   ╔═══╗   ║
            // ║0x1 1x1║   ║3x1║
            // ║   ╔═══┼═══╝───╢
            // ║0x2║   ║2x2 3x2║
            // ║   ╚═══╝   ╔═══╝
            // ║0x3 1x3 2x3║3x3
            // ╚═══════════╝
            // Console.WriteLine(new Maze2DToMap2DConverter().Convert(
            //     maze,
            //     Maze2DToMap2DConverter.MazeToMapOptions.Custom(
            //         trailXWidths: new int[] { 1, 2, 1, 2 },
            //         trailYHeights: new int[] { 2, 1, 2, 1 },
            //         wallXWidths: new int[] { 1, 2, 1, 2, 1 },
            //         wallYHeights: new int[] { 2, 1, 2, 1, 2 }))
            //         .ToString());
            //
            //           13 x 14
            //  WYH_4 -- ▓▓▓▓▓▓▓▓     
            //  WYH_4 -- ▓▓▓▓▓▓▓▓▓    
            //  TYH_3    ▓░░░░░░░▓▓▓▓ 
            //  WYH_3 -- ▓░▒▓▓▓▒░▒▓▓▓▓
            //  TYH_2    ▓░▓▓▓▓▓░░░░░▓
            //  TYH_2    ▓░▓▓▓▓▓░░░░░▓
            //  WYH_2 -- ▓░▓▓▓▓▓▓▓▓▓▓▓
            //  WYH_2 -- ▓░▒▓▓▓▓▓▓▓▓▓▓
            //  TYH_1    ▓░░░░░▓▓▓▓░░▓
            //  WYH_1 -- ▓░░░░░▒▓▓▒░░▓
            //  TYH_0    ▓░░░░░░░░░░░▓
            //  TYH_0    ▓░░░░░░░░░░░▓
            //  WYH_0 -- ▓▓▓▓▓▓▓▓▓▓▓▓▓
            //  WYH_0 -- ▓▓▓▓▓▓▓▓▓▓▓▓▓
            //           WTWWTTWTWWTTW
            //           001 1 223 3 4

            // The goal is to make sure the cellMapping *Cells properties return
            // valid groups of cells
            // how to validate? make sure x,y matches expected values for all
            // returned cells
            // Assert.XYIn(expected, actual)
            // Assert.XIn([1, 2], cellMapping.NWCells.Select(cell => CellPosition(map, cell)))
            // Assert.YIn([1, 2], cellMapping.NWCells.Select(cell => CellPosition(map, cell)))

            var mazeToMapOptions = new MazeToMapOptions(
                trailXWidths: new int[] { 1, 2, 1, 2 },
                trailYHeights: new int[] { 2, 1, 2, 1 },
                wallXWidths: new int[] { 1, 2, 1, 2, 1 },
                wallYHeights: new int[] { 2, 1, 2, 1, 2 });
            var map = CreateMapForMaze(mazeToMapOptions);
            var mazeToMap = new Maze2DRenderer(maze, mazeToMapOptions);
            var cellMapping = mazeToMap.CreateCellsMapping(map, maze.Cells[0]);

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
            Assert.That(cellMapping.CenterCells.Select(cell => cell.Position(map).X), Has.All.AnyOf(1), "CenterCells.X");
            Assert.That(cellMapping.CenterCells.Select(cell => cell.Position(map).Y), Has.All.AnyOf(2, 3), "CenterCells.Y");

            Assert.That(cellMapping.NWCells.ToList(), Has.Count.EqualTo(1), "NWCells Count");
            Assert.That(cellMapping.NWCells.Select(cell => cell.Position(map).X), Has.All.AnyOf(0), "NWCells.X");
            Assert.That(cellMapping.NWCells.Select(cell => cell.Position(map).Y), Has.All.AnyOf(4), "NWCells.Y");

            Assert.That(cellMapping.NCells.ToList(), Has.Count.EqualTo(1), "NCells Count");
            Assert.That(cellMapping.NCells.Select(cell => cell.Position(map).X), Has.All.AnyOf(1), "NCells.X");
            Assert.That(cellMapping.NCells.Select(cell => cell.Position(map).Y), Has.All.AnyOf(4), "NCells.Y");

            Assert.That(cellMapping.NECells.ToList(), Has.Count.EqualTo(2), "NECells Count");
            Assert.That(cellMapping.NECells.Select(cell => cell.Position(map).X), Has.All.AnyOf(2, 3), "NECells.X");
            Assert.That(cellMapping.NECells.Select(cell => cell.Position(map).Y), Has.All.AnyOf(4), "NECells.Y");

            Assert.That(cellMapping.WCells.ToList(), Has.Count.EqualTo(2), "WCells Count");
            Assert.That(cellMapping.WCells.Select(cell => cell.Position(map).X), Has.All.AnyOf(0), "WCells.X");
            Assert.That(cellMapping.WCells.Select(cell => cell.Position(map).Y), Has.All.AnyOf(2, 3), "WCells.Y");

            Assert.That(cellMapping.ECells.ToList(), Has.Count.EqualTo(4), "ECells Count");
            Assert.That(cellMapping.ECells.Select(cell => cell.Position(map).X), Has.All.AnyOf(2, 3), "ECells.X");
            Assert.That(cellMapping.ECells.Select(cell => cell.Position(map).Y), Has.All.AnyOf(2, 3), "ECells.Y");

            Assert.That(cellMapping.SWCells.ToList(), Has.Count.EqualTo(2), "SWCells Count");
            Assert.That(cellMapping.SWCells.Select(cell => cell.Position(map).X), Has.All.AnyOf(0), "SWCells.X");
            Assert.That(cellMapping.SWCells.Select(cell => cell.Position(map).Y), Has.All.AnyOf(0, 1), "SWCells.Y");

            Assert.That(cellMapping.SCells.ToList(), Has.Count.EqualTo(2), "SCells Count");
            Assert.That(cellMapping.SCells.Select(cell => cell.Position(map).X), Has.All.AnyOf(1), "SCells.X");
            Assert.That(cellMapping.SCells.Select(cell => cell.Position(map).Y), Has.All.AnyOf(0, 1), "SCells.Y");

            Assert.That(cellMapping.SECells.ToList(), Has.Count.EqualTo(4), "SECells Count");
            Assert.That(cellMapping.SECells.Select(cell => cell.Position(map).X), Has.All.AnyOf(2, 3), "SECells.X");
            Assert.That(cellMapping.SECells.Select(cell => cell.Position(map).Y), Has.All.AnyOf(0, 1), "SECells.Y");
        }
    }
}