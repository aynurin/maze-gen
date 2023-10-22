using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Nour.Play.Maze;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class CellTest {

        [Test]
        public void Map2D_LinksAreMutual() {
            MazeCell a = new MazeCell(1, 2);
            MazeCell b = new MazeCell(2, 2);
            a.Link(b);
            Assert.IsTrue(b.Links(Vector.North2D).HasValue);
            Assert.AreEqual(b.Links(Vector.North2D).Value, a);

            b.Unlink(a);
            Assert.IsFalse(a.Links(Vector.South2D).HasValue);
        }

        [Test]
        public void Map2D_ToString() {
            MazeCell a = new MazeCell(1, 2);
            MazeCell b = new MazeCell(2, 2);
            a.Link(b);
            Assert.AreEqual("1x2", a.ToString());
            Assert.AreEqual("1x2: ---W", a.ToLongString());
            Assert.AreEqual("2x2", b.ToString());
            Assert.AreEqual("2x2: -E--", b.ToLongString());

            MazeCell c = new MazeCell(1, 1);
            MazeCell d = new MazeCell(1, 2);
            c.Link(d);
            Assert.AreEqual("1x1: --S-", c.ToLongString());
            Assert.AreEqual("1x2: N---", d.ToLongString());

            b.Unlink(a);
            Assert.IsFalse(a.Links(Vector.South2D).HasValue);
        }

        [Test]
        public void Map2D_DoubleLinkingThrowsError() {
            MazeCell a = new MazeCell(1, 2);
            MazeCell b = new MazeCell(2, 2);
            a.Link(b);
            Assert.Throws<InvalidOperationException>(() => b.Link(a));
        }

    }
}