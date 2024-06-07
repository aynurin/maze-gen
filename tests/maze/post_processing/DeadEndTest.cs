using System;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Serializer;

namespace PlayersWorlds.Maps.Maze.PostProcessing {
    [TestFixture]
    public class DeadEndTest : Test {
        [Test]
        public void DeadEnd_CanFindDeadEnds() {
            var maze = LegacyAreaSerializer.ParseV01MazeString("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            var deadEnds = DeadEnd.Find(maze);
            Assert.That(deadEnds.DeadEnds, Is.Not.Empty);
            Assert.That(4, Is.EqualTo(deadEnds.DeadEnds.Count));
            Assert.That(deadEnds.DeadEnds.Contains(new Vector(0, 0)), Is.True, "0,0");
            Assert.That(deadEnds.DeadEnds.Contains(new Vector(2, 1)), Is.True, "2,1");
            Assert.That(deadEnds.DeadEnds.Contains(new Vector(0, 2)), Is.True, "0,2");
            Assert.That(deadEnds.DeadEnds.Contains(new Vector(2, 2)), Is.True, "2,2");
            Assert.That(maze.Cells.Count(
                cell => cell.X<DeadEnd.IsDeadEndExtension>() != null), Is.EqualTo(4));
        }
    }
}