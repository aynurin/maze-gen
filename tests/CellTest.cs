using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class CellTest {

        [Test]
        public void Map2D_LinksAreMutual() {
            Cell a = new Cell(1, 2);
            Cell b = new Cell(2, 2);
            a.Link(b);
            Assert.IsTrue(b.Links(Vector.North2D).HasValue);
            Assert.AreEqual(b.Links(Vector.North2D).Value, a);

            b.Unlink(a);
            Assert.IsFalse(a.Links(Vector.South2D).HasValue);
        }

        [Test]
        public void Map2D_ToLongString() {
            Cell a = new Cell(1, 2);
            Cell b = new Cell(2, 2);
            a.Link(b);
            Assert.AreEqual("1x2: ---W", a.ToLongString());
            Assert.AreEqual("2x2: -E--", b.ToLongString());

            Cell c = new Cell(1, 1);
            Cell d = new Cell(1, 2);
            c.Link(d);
            Assert.AreEqual("1x1: --S-", c.ToLongString());
            Assert.AreEqual("1x2: N---", d.ToLongString());

            b.Unlink(a);
            Assert.IsFalse(a.Links(Vector.South2D).HasValue);
        }

        [Test]
        public void Map2D_DoubleLinkingThrowsError() {
            Cell a = new Cell(1, 2);
            Cell b = new Cell(2, 2);
            a.Link(b);
            Assert.Throws<InvalidOperationException>(() => b.Link(a));
        }

    }
}