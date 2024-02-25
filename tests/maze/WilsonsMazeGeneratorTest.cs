

using Moq;
using NUnit.Framework;

namespace PlayersWorlds.Maps.Maze {
    [TestFixture]
    public class WilsonsMazeGeneratorTest {
        [Test]
        public void DuplicateRandom() {
            var maze = new Maze2D(5, 5);
            var builderMock = new Mock<Maze2DBuilder>(
                maze, new GeneratorOptions()
            );
            var firstCell = maze.Cells[new Vector(4, 3)];
            var randomNeighbor = maze.Cells[new Vector(3, 3)];

            builderMock.SetupSequence(b => b.PickNextCellToLink())
                .Returns(firstCell)
                .Returns(firstCell);
            builderMock.Setup(b => b.TryPickRandomNeighbor(
                    firstCell, out randomNeighbor, false, false))
                .Returns(true);
            builderMock.Setup(b => b.TryPickRandomNeighbor(
                    randomNeighbor, out firstCell, false, false))
                .Returns(true);
            builderMock.SetupSequence(b => b.IsFillComplete())
                .Returns(false)
                .Returns(true);

            Assert.That(() =>
                new WilsonsMazeGenerator()
                    .GenerateMaze(builderMock.Object),
                Throws.Nothing);
        }
    }
}