using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps.Renderers {

    [TestFixture]
    internal class Map2DStringRendererTest {
        [Test]
        public void TestRender() {
            var map = new Map2D(new Vector(5, 5));
            map.CellsAt(new Vector(0, 0), new Vector(5, 5))
                .ForEach(c => c.Tags.Add(Maze2DRenderer.MapCellType.Trail));
            map.CellsAt(new Vector(0, 0), new Vector(5, 1))
                .ForEach(c => c.Tags.Add(Maze2DRenderer.MapCellType.Wall));
            map.CellsAt(new Vector(0, 0), new Vector(1, 5))
                .ForEach(c => c.Tags.Add(Maze2DRenderer.MapCellType.Wall));
            map.CellsAt(new Vector(4, 0), new Vector(1, 5))
                .ForEach(c => c.Tags.Add(Maze2DRenderer.MapCellType.Wall));
            map.CellsAt(new Vector(0, 4), new Vector(5, 1))
                .ForEach(c => c.Tags.Add(Maze2DRenderer.MapCellType.Wall));
            map.CellsAt(new Vector(2, 2), new Vector(1, 1))
                .ForEach(c => c.Tags.Add(Maze2DRenderer.MapCellType.Edge));
            var expected =
                "▓▓▓▓▓\n" +
                "▓░░░▓\n" +
                "▓░▒░▓\n" +
                "▓░░░▓\n" +
                "▓▓▓▓▓\n";
            var actual = map.ToString();
            Console.WriteLine(actual);
            Assert.AreEqual(expected, actual);
        }
    }
}