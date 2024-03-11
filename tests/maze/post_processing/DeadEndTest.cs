using System;
using System.Linq;
using NUnit.Framework;

namespace PlayersWorlds.Maps.Maze.PostProcessing {
    [TestFixture]
    public class DeadEndTest : Test {
        [Test]
        public void DeadEnd_CanFindDeadEnds() {
            var maze = Maze2D.Parse("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            var deadEnds = DeadEnd.Find(maze);
            Assert.That(deadEnds, Is.Not.Empty);
            Assert.That(4, Is.EqualTo(deadEnds.Count));
            Assert.That(deadEnds.Contains(maze.Cells[new Vector(0, 0)]), Is.True, "0,0");
            Assert.That(deadEnds.Contains(maze.Cells[new Vector(2, 1)]), Is.True, "2,1");
            Assert.That(deadEnds.Contains(maze.Cells[new Vector(0, 2)]), Is.True, "0,2");
            Assert.That(deadEnds.Contains(maze.Cells[new Vector(2, 2)]), Is.True, "2,2");
            Assert.That(5, Is.EqualTo(maze.Cells.Count(
                cell => !cell.Attributes.ContainsKey(
                    DeadEnd.DeadEndAttribute))));
        }
    }
}