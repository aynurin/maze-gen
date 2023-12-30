using System;
using System.Collections.Generic;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps {
    [TestFixture]
    public class CellTest {

        [Test]
        public void Map2D_LinksAreMutual() {
            var a = new MazeCell(2, 1);
            var b = new MazeCell(2, 2);
            a.Neighbors().Add(b);
            b.Neighbors().Add(a);

            a.Link(b);
            Assert.That(a.Links(), Has.Count.EqualTo(1));
            Assert.That(b.Links(), Has.Count.EqualTo(1));
            Assert.IsTrue(a.Links(Vector.North2D).HasValue);
            Assert.IsTrue(b.Links(Vector.South2D).HasValue);
            Assert.AreEqual(a.Links(Vector.North2D).Value, b);
            Assert.AreEqual(b.Links(Vector.South2D).Value, a);

            b.Unlink(a);
            Assert.IsFalse(a.Links(Vector.North2D).HasValue);
            Assert.IsFalse(b.Links(Vector.South2D).HasValue);
            Assert.That(a.Links(), Has.Count.EqualTo(0));
            Assert.That(b.Links(), Has.Count.EqualTo(0));
        }

        [Test]
        public void Map2D_CanAssignMapAreaOnce() {
            var a = new MazeCell(2, 1);
            var mapArea = new MapArea(AreaType.Fill, new Vector(2, 2), new Vector(2, 2));

            Assert.That(() => a.AssignMapArea(mapArea, new List<MazeCell> { a }), Throws.Nothing);
            Assert.That(() => a.AssignMapArea(mapArea, new List<MazeCell> { a }), Throws.InvalidOperationException);
            Assert.That(a.MapArea, Is.SameAs(mapArea));
        }

        [Test]
        public void Map2D_ToString() {
            var a = new MazeCell(2, 1);
            var b = new MazeCell(2, 2);
            a.Neighbors().Add(b);
            b.Neighbors().Add(a);

            a.Link(b);
            Assert.AreEqual("2x1V", a.ToString());
            Assert.AreEqual("2x1V(N---)", a.ToLongString());
            Assert.AreEqual("2x2V", b.ToString());
            Assert.AreEqual("2x2V(--S-)", b.ToLongString());

            var c = new MazeCell(1, 1);
            var d = new MazeCell(2, 1);
            c.Neighbors().Add(d);
            d.Neighbors().Add(c);

            c.Link(d);
            Assert.AreEqual("1x1V(-E--)", c.ToLongString());
            Assert.AreEqual("2x1V(---W)", d.ToLongString());

            b.Unlink(a);
            Assert.IsFalse(a.Links(Vector.South2D).HasValue);
        }

        [Test]
        public void Map2D_DoubleLinkingThrowsError() {
            var a = new MazeCell(1, 2);
            var b = new MazeCell(2, 2);
            a.Neighbors().Add(b);
            b.Neighbors().Add(a);
            a.Link(b);
            Assert.Throws<InvalidOperationException>(() => b.Link(a));
        }

    }
}