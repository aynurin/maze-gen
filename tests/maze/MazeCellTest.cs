using System;
using NUnit.Framework;

namespace PlayersWorlds.Maps.Maze {
    [TestFixture]
    public class MazeCellTest : Test {

        [Test]
        public void ToStringTest() {
            var env = Area.CreateMaze(new Vector(5, 5));
            var a = new Vector(2, 1);
            var b = new Vector(2, 2);
            var c = new Vector(1, 3);
            var d = new Vector(2, 3);

            env[a].HardLinks.Add(b);
            env[b].HardLinks.Add(a);
            Assert.That(env[a].ToString(), Is.EqualTo("Cell:{Maze;[2x2];}"));
            Assert.That(env[b].ToString(), Is.EqualTo("Cell:{Maze;[2x1];}"));

            env[c].HardLinks.Add(d);
            env[d].HardLinks.Add(c);
            Assert.That(env[c].ToString(), Is.EqualTo("Cell:{Maze;[2x3];}"));
            Assert.That(env[d].ToString(), Is.EqualTo("Cell:{Maze;[1x3];}"));

            env[b].HardLinks.Remove(a);
            env[a].HardLinks.Remove(b);
            Assert.That(env[a].HasLinks(a + Vector.South2D), Is.False);
        }

    }
}