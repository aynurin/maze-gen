using System;
using System.Linq;
using NUnit.Framework;

namespace Nour.Play {

    [TestFixture]
    internal class Map2DTest {
        [Test]
        public void CellsAt_OneCell() {
            var map = new Map2D(new Vector(5, 5));
            var cells = map.CellsAt(new Vector(0, 0), new Vector(1, 1)).ToList();
            var debugString = String.Join(",", cells.Select(c => map.Cells.IndexOf(c)));
            Assert.AreEqual(1, cells.Count());
            Assert.AreEqual(new Vector(0, 0).ToIndex(5), map.Cells.IndexOf(cells.First()));
        }

        [Test]
        public void CellsAt_TwoCells() {
            var map = new Map2D(new Vector(5, 5));
            var cells = map.CellsAt(new Vector(0, 0), new Vector(2, 1)).ToList();
            var debugString = String.Join(",", cells.Select(c => map.Cells.IndexOf(c)));
            Assert.AreEqual(2, cells.Count());
            Assert.AreEqual(new Vector(0, 0).ToIndex(5), map.Cells.IndexOf(cells.First()), "0,0: " + debugString);
            Assert.AreEqual(new Vector(1, 0).ToIndex(5), map.Cells.IndexOf(cells.Last()), "1,0: " + debugString);
        }

        [Test]
        public void CellsAt_TreeByTwo() {
            var map = new Map2D(new Vector(5, 5));
            var cells = map.CellsAt(new Vector(0, 0), new Vector(3, 2)).ToList();
            var debugString = String.Join(",", cells.Select(c => map.Cells.IndexOf(c)));
            Assert.AreEqual(6, cells.Count());
            Assert.AreEqual(new Vector(0, 0).ToIndex(5), map.Cells.IndexOf(cells[0]), "0,0: " + debugString);
            Assert.AreEqual(new Vector(0, 1).ToIndex(5), map.Cells.IndexOf(cells[1]), "0,1: " + debugString);
            Assert.AreEqual(new Vector(1, 0).ToIndex(5), map.Cells.IndexOf(cells[2]), "1,0: " + debugString);
            Assert.AreEqual(new Vector(1, 1).ToIndex(5), map.Cells.IndexOf(cells[3]), "1,1: " + debugString);
            Assert.AreEqual(new Vector(2, 0).ToIndex(5), map.Cells.IndexOf(cells[4]), "2,0: " + debugString);
            Assert.AreEqual(new Vector(2, 1).ToIndex(5), map.Cells.IndexOf(cells[5]), "2,1: " + debugString);
        }

        [Test]
        public void CellsAt_FourCellsFar() {
            var map = new Map2D(new Vector(5, 5));
            var cells = map.CellsAt(new Vector(3, 3), new Vector(2, 2)).ToList();
            var debugString = String.Join(",", cells.Select(c => map.Cells.IndexOf(c)));
            Assert.AreEqual(4, cells.Count());
            Assert.AreEqual(new Vector(3, 3).ToIndex(5), map.Cells.IndexOf(cells[0]), "3,3: " + debugString);
            Assert.AreEqual(new Vector(3, 4).ToIndex(5), map.Cells.IndexOf(cells[1]), "3,4: " + debugString);
            Assert.AreEqual(new Vector(4, 3).ToIndex(5), map.Cells.IndexOf(cells[2]), "4,3: " + debugString);
            Assert.AreEqual(new Vector(4, 4).ToIndex(5), map.Cells.IndexOf(cells[3]), "4,4: " + debugString);
        }

        [Test]
        public void CellsAt_OutOfBounds() {
            var map = new Map2D(new Vector(5, 5));
            Assert.Throws<IndexOutOfRangeException>(() => map.CellsAt(new Vector(5, 5), new Vector(1, 1)).ToList(), "1");
            Assert.Throws<IndexOutOfRangeException>(() => map.CellsAt(new Vector(6, 3), new Vector(1, 1)).ToList(), "2");
            Assert.Throws<IndexOutOfRangeException>(() => map.CellsAt(new Vector(0, 0), new Vector(6, 6)).ToList(), "3");
            Assert.Throws<IndexOutOfRangeException>(() => map.CellsAt(new Vector(0, 0), new Vector(6, 1)).ToList(), "4");
        }
    }
}