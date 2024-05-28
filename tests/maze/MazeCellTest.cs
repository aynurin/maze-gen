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

            a.Link(b);
            Assert.That(a.Links(), Has.Count.EqualTo(1));
            Assert.That(b.Links(), Has.Count.EqualTo(1));
            Assert.That(a.Links(Vector.North2D).HasValue, Is.True);
            Assert.That(b.Links(Vector.South2D).HasValue, Is.True);
            Assert.That(a.Links(Vector.North2D).Value, Is.EqualTo(b));
            Assert.That(b.Links(Vector.South2D).Value, Is.EqualTo(a));

            b.Unlink(a);
            Assert.That(a.Links(Vector.North2D).HasValue, Is.False);
            Assert.That(b.Links(Vector.South2D).HasValue, Is.False);
            Assert.That(a.Links(), Has.Count.EqualTo(0));
            Assert.That(b.Links(), Has.Count.EqualTo(0));
        }

        [Test]
        public void LinksOnlyWithNeighbors() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var a = env[new Vector(2, 1)];
            var b = env[new Vector(2, 2)];
            var c = env[new Vector(2, 3)];

            Assert.That(() => a.Link(c), Throws.InstanceOf<NotImplementedException>());
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
        //     Assert.That(a.ChildAreas, Has.Exactly(1).Items);
        //     Assert.That(a.ChildAreas.First(), Is.SameAs(mapArea));
        // }

        [Test]
        public void ToStringTest() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var a = env[new Vector(2, 1)];
            var b = env[new Vector(2, 2)];
            var c = env[new Vector(1, 3)];
            var d = env[new Vector(2, 3)];

            a.Link(b);
            Assert.That(a.ToString(), Is.EqualTo("Cell(2x1V [])"));
            Assert.That(b.ToString(), Is.EqualTo("Cell(2x2V [])"));
            Assert.That(a.ToLongString(), Is.EqualTo("Cell(2x1V(N---) [])"));
            Assert.That(b.ToLongString(), Is.EqualTo("Cell(2x2V(--S-) [])"));

            c.Link(d);
            Assert.That(c.ToLongString(), Is.EqualTo("Cell(1x3V(-E--) [])"));
            Assert.That(d.ToLongString(), Is.EqualTo("Cell(2x3V(---W) [])"));

            b.Unlink(a);
            Assert.That(a.Links(Vector.South2D).HasValue, Is.False);
        }

        [Test]
        public void DoubleLinkingThrowsError() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var a = env[new Vector(1, 2)];
            var b = env[new Vector(2, 2)];
            a.Link(b);
            Assert.Throws<InvalidOperationException>(() => b.Link(a));
        }

    }
}