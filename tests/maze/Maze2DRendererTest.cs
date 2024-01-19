using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps.Maze {

    [TestFixture]
    internal class Maze2DRendererTest {
        [Test]
        public void ThrowsIfCantFit() {
            Console.WriteLine(MazeToMapOptions.SquareCells(2, 2).RenderedSize(new Vector(10, 10)));
            void Act() =>
                new Maze2DRenderer(
                    new Maze2D(10, 10),
                    MazeToMapOptions.SquareCells(2, 2)
                ).Render(new Map2D(new Vector(10, 10)));
            Assert.That(Act, Throws.Exception.TypeOf<ArgumentException>());
        }
    }
}