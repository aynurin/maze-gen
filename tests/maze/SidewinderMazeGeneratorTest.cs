
using System.Collections.Generic;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Maze {
    [TestFixture]
    public class SidewinderMazeGeneratorTest {
        [Test]
        public void ArchShapedAreas() {
            var area1 = MapArea.Create(AreaType.Hall, new Vector(2, 2), new Vector(3, 13));
            var area2 = MapArea.Create(AreaType.Hall, new Vector(10, 2), new Vector(3, 13));
            var area3 = MapArea.Create(AreaType.Hall, new Vector(4, 8), new Vector(7, 3));
            MazeTestHelper.GenerateMaze(
                new Vector(15, 15),
                GeneratorOptions.Algorithms.Sidewinder,
                new List<MapArea>() { area1, area2, area3 },
                GeneratorOptions.FillFactorOption.Full,
                out var builder);
            Assert.That(builder.CellsToConnect, Is.Empty);
        }

        [Test]
        [Repeat(10)] // random factor
        public void ArchShapedAreasLeftExit() {
            var area1 = MapArea.Create(AreaType.Hall, new Vector(2, 2), new Vector(3, 13));
            var area2 = MapArea.Create(AreaType.Hall, new Vector(10, 2), new Vector(3, 13));
            var area3 = MapArea.Create(AreaType.Hall, new Vector(6, 8), new Vector(7, 3));
            MazeTestHelper.GenerateMaze(
                new Vector(15, 15),
                GeneratorOptions.Algorithms.Sidewinder,
                new List<MapArea>() { area1, area2, area3 },
                GeneratorOptions.FillFactorOption.Full,
                out var builder);
            Assert.That(builder.CellsToConnect, Is.Empty);
        }
    }
}