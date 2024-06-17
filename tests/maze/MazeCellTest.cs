using System;
using NUnit.Framework;

namespace PlayersWorlds.Maps.Maze {
    [TestFixture]
    public class MazeCellTest : Test {

        [Test]
        public void LinksAreMutual() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var a = new Vector(2, 1);
            var b = new Vector(2, 2);

            env.Link(a, b);
            Assert.That(env[a].HardLinks, Has.Count.EqualTo(1));
            Assert.That(env[b].HardLinks, Has.Count.EqualTo(1));
            Assert.That(env.CellsAreLinked(a, a + Vector.North2D), Is.True);
            Assert.That(env.CellsAreLinked(b, b + Vector.South2D), Is.True);
            Assert.That(env[a + Vector.North2D], Is.EqualTo(env[b]));
            Assert.That(env[b + Vector.South2D], Is.EqualTo(env[a]));

            env[b].HardLinks.Remove(a);
            env[a].HardLinks.Remove(b);
            Assert.That(env.CellsAreLinked(a, a + Vector.North2D), Is.False);
            Assert.That(env.CellsAreLinked(b, b + Vector.South2D), Is.False);
            Assert.That(env[a].HardLinks, Has.Count.EqualTo(0));
            Assert.That(env[b].HardLinks, Has.Count.EqualTo(0));
        }

        [Test]
        public void LinksOnlyWithNeighbors() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var a = new Vector(2, 1);
            var b = new Vector(2, 2);
            var c = new Vector(2, 3);

            Assert.That(() => env.Link(a, c), Throws.InstanceOf<InvalidOperationException>());
        }

        [Test]
        public void ToStringTest() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var a = new Vector(2, 1);
            var b = new Vector(2, 2);
            var c = new Vector(1, 3);
            var d = new Vector(2, 3);

            env.Link(a, b);
            Assert.That(env[a].ToString(), Is.EqualTo("Cell:{Environment;[2x2];}"));
            Assert.That(env[b].ToString(), Is.EqualTo("Cell:{Environment;[2x1];}"));

            env.Link(c, d);
            Assert.That(env[c].ToString(), Is.EqualTo("Cell:{Environment;[2x3];}"));
            Assert.That(env[d].ToString(), Is.EqualTo("Cell:{Environment;[1x3];}"));

            env[b].HardLinks.Remove(a);
            env[a].HardLinks.Remove(b);
            Assert.That(env.CellsAreLinked(a, a + Vector.South2D), Is.False);
        }

        [Test]
        public void DoubleLinkingThrowsError() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var a = new Vector(1, 2);
            var b = new Vector(2, 2);
            env.Link(a, b);
            Assert.Throws<InvalidOperationException>(() => env.Link(a, a));
        }

    }
}