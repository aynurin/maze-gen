using System;
using NUnit.Framework;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps.Maze {

    [TestFixture]
    internal class Maze2DRendererTest : Test {
        [Test]
        public void ThrowsIfCantFit() {
            void Act() =>
                new Maze2DRenderer(
                    Area.CreateEnvironment(new Vector(10, 10)),
                    MazeToMapOptions.SquareCells(2, 2)
                ).Render(Area.CreateEnvironment(new Vector(10, 10)));
            Assert.That(Act, Throws.Exception.TypeOf<ArgumentException>());
        }
    }
}