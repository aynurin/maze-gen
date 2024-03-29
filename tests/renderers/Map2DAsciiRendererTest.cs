using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps.Renderers {

    [TestFixture]
    internal class Map2DStringRendererTest : Test {
        [Test]
        public void TestRender() {
            var map = new Map2D(new Vector(5, 5));
            map.Cells.Iterate(new Vector(0, 0), new Vector(5, 5))
                .ForEach(c => c.cell.Tags.Add(Cell.CellTag.MazeTrail));
            map.Cells.Iterate(new Vector(0, 0), new Vector(5, 1))
                .ForEach(c => c.cell.Tags.Add(Cell.CellTag.MazeWall));
            map.Cells.Iterate(new Vector(0, 0), new Vector(1, 5))
                .ForEach(c => c.cell.Tags.Add(Cell.CellTag.MazeWall));
            map.Cells.Iterate(new Vector(4, 0), new Vector(1, 5))
                .ForEach(c => c.cell.Tags.Add(Cell.CellTag.MazeWall));
            map.Cells.Iterate(new Vector(0, 4), new Vector(5, 1))
                .ForEach(c => c.cell.Tags.Add(Cell.CellTag.MazeWall));
            map.Cells.Iterate(new Vector(2, 2), new Vector(1, 1))
                .ForEach(c => c.cell.Tags.Add(Cell.CellTag.MazeWallCorner));
            var expected =
                "▓▓▓▓▓\n" +
                "▓░░░▓\n" +
                "▓░▒░▓\n" +
                "▓░░░▓\n" +
                "▓▓▓▓▓\n";
            var actual = map.ToString();
            TestLog.CreateForThisTest().D(5, actual);
            Assert.That(expected, Is.EqualTo(actual));
        }
    }
}