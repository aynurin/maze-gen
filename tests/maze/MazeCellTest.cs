using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Maze {
    [TestFixture]
    public class MazeCellTest {

        [Test]
        public void LinksAreMutual() {
            var a = new MazeCell(2, 1);
            var b = new MazeCell(2, 2);
            a.Neighbors().Add(b);
            b.Neighbors().Add(a);

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
            var a = new MazeCell(2, 1);
            var b = new MazeCell(2, 2);
            var c = new MazeCell(2, 3);
            a.Neighbors().Add(b);
            b.Neighbors().Add(a);
            b.Neighbors().Add(c);
            c.Neighbors().Add(b);

            Assert.That(() => a.Link(c), Throws.InstanceOf<NotImplementedException>());
        }

        [Test]
        public void CanAssignMapAreaOnce() {
            var a = new MazeCell(2, 1);
            var mapArea = MapArea.Create(
                AreaType.Fill, new Vector(2, 2), new Vector(2, 2));

            Assert.That(() => a.AddMapArea(mapArea, new List<MazeCell> { a }),
                Throws.Nothing);
            Assert.That(() => a.AddMapArea(mapArea, new List<MazeCell> { a }),
                Throws.ArgumentException);
            Assert.That(a.MapAreas, Has.Exactly(1).Items);
            Assert.That(a.MapAreas.First().Key, Is.SameAs(mapArea));
        }

        [Test]
        public void ToStringTest() {
            var a = new MazeCell(2, 1);
            var b = new MazeCell(2, 2);
            a.Neighbors().Add(b);
            b.Neighbors().Add(a);

            a.Link(b);
            Assert.That("2x1V", Is.EqualTo(a.ToString()));
            Assert.That("2x1V(N---)", Is.EqualTo(a.ToLongString()));
            Assert.That("2x2V", Is.EqualTo(b.ToString()));
            Assert.That("2x2V(--S-)", Is.EqualTo(b.ToLongString()));

            var c = new MazeCell(1, 1);
            var d = new MazeCell(2, 1);
            c.Neighbors().Add(d);
            d.Neighbors().Add(c);

            c.Link(d);
            Assert.That("1x1V(-E--)", Is.EqualTo(c.ToLongString()));
            Assert.That("2x1V(---W)", Is.EqualTo(d.ToLongString()));

            b.Unlink(a);
            Assert.That(a.Links(Vector.South2D).HasValue, Is.False);
        }

        [Test]
        public void DoubleLinkingThrowsError() {
            var a = new MazeCell(1, 2);
            var b = new MazeCell(2, 2);
            a.Neighbors().Add(b);
            b.Neighbors().Add(a);
            a.Link(b);
            Assert.Throws<InvalidOperationException>(() => b.Link(a));
        }

    }
}