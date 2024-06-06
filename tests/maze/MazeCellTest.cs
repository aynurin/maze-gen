using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Maze {
    [TestFixture]
    public class MazeCellTest : Test {

        [Test]
        public void LinksAreMutual() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var a = env[new Vector(2, 1)];
            var b = env[new Vector(2, 2)];

            env.Link(a.Position, b.Position);
            Assert.That(a.HardLinks, Has.Count.EqualTo(1));
            Assert.That(b.HardLinks, Has.Count.EqualTo(1));
            Assert.That(env.CellsAreLinked(a.Position, a.Position + Vector.North2D), Is.True);
            Assert.That(env.CellsAreLinked(b.Position, b.Position + Vector.South2D), Is.True);
            Assert.That(env[a.Position + Vector.North2D], Is.EqualTo(b));
            Assert.That(env[b.Position + Vector.South2D], Is.EqualTo(a));

            b.HardLinks.Remove(a.Position);
            a.HardLinks.Remove(b.Position);
            Assert.That(env.CellsAreLinked(a.Position, a.Position + Vector.North2D), Is.False);
            Assert.That(env.CellsAreLinked(b.Position, b.Position + Vector.South2D), Is.False);
            Assert.That(a.HardLinks, Has.Count.EqualTo(0));
            Assert.That(b.HardLinks, Has.Count.EqualTo(0));
        }

        [Test]
        public void LinksOnlyWithNeighbors() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var a = env[new Vector(2, 1)];
            var b = env[new Vector(2, 2)];
            var c = env[new Vector(2, 3)];

            Assert.That(() => env.Link(a.Position, c.Position), Throws.InstanceOf<InvalidOperationException>());
        }

        // [Test]
        // public void CanAssignMapAreaOnce() {
        //     var a = new Cell(new Vector(2, 1));
        //     var mapArea = Area.Create(
        //         new Vector(2, 2), new Vector(2, 2), AreaType.Fill);

        //     Assert.That(() => a.AddMapArea(mapArea),
        //         Throws.Nothing);
        //     Assert.That(() => a.AddMapArea(mapArea),
        //         Throws.InvalidOperationException);
        //     Assert.That(a.ChildAreas(), Has.Exactly(1).Items);
        //     Assert.That(a.ChildAreas().First(), Is.SameAs(mapArea));
        // }

        [Test]
        public void ToStringTest() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var a = env[new Vector(2, 1)];
            var b = env[new Vector(2, 2)];
            var c = env[new Vector(1, 3)];
            var d = env[new Vector(2, 3)];

            env.Link(a.Position, b.Position);
            Assert.That(a.ToString(), Is.EqualTo("Cell:{2x1;[2x2];}"));
            Assert.That(b.ToString(), Is.EqualTo("Cell:{2x2;[2x1];}"));

            env.Link(c.Position, d.Position);
            Assert.That(c.ToString(), Is.EqualTo("Cell:{1x3;[2x3];}"));
            Assert.That(d.ToString(), Is.EqualTo("Cell:{2x3;[1x3];}"));

            b.HardLinks.Remove(a.Position);
            a.HardLinks.Remove(b.Position);
            Assert.That(env.CellsAreLinked(a.Position, a.Position + Vector.South2D), Is.False);
        }

        [Test]
        public void DoubleLinkingThrowsError() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var a = env[new Vector(1, 2)];
            var b = env[new Vector(2, 2)];
            env.Link(a.Position, b.Position);
            Assert.Throws<InvalidOperationException>(() => env.Link(a.Position, a.Position));
        }

    }
}