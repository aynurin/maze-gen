using System.Linq;
using Nour.Play.Maze;
using Nour.Play.Maze.PostProcessing;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class DeadEndTest {
        [Test]
        public void DeadEnd_CanFindDeadEnds() {
            var maze = Maze2D.Parse("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            var deadEnds = DeadEnd.Find(maze);
            Assert.IsNotEmpty(deadEnds);
            Assert.AreEqual(4, deadEnds.Count);
            Assert.IsTrue(deadEnds.Contains(maze.Cells.ElementAt(new Vector(0, 0), maze.Size)));
            Assert.IsTrue(deadEnds.Contains(maze.Cells.ElementAt(new Vector(2, 0), maze.Size)));
            Assert.IsTrue(deadEnds.Contains(maze.Cells.ElementAt(new Vector(2, 2), maze.Size)));
            Assert.IsTrue(deadEnds.Contains(maze.Cells.ElementAt(new Vector(1, 2), maze.Size)));
            Assert.AreEqual(5, maze.Cells.Count(
                cell => !cell.Attributes.ContainsKey(
                    DeadEnd.DeadEndAttribute)));
        }
    }
}