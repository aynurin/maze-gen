using System;
using System.Linq;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class Map2DTest {
        [Test]
        public void Map2D_IsInitialized() {
            Map2D map = new Map2D(2, 3);
            Assert.AreEqual(6, map.Area);
            Assert.AreEqual(6, map.Cells.Count);
            Assert.AreEqual(2, map.XHeightRows);
            Assert.AreEqual(3, map.YWidthColumns);
        }

        [Test]
        public void Map2D_WrongSize() {
            Assert.Throws<ArgumentException>(() => new Map2D(0, 3));
            Assert.Throws<ArgumentException>(() => new Map2D(2, 0));
            Assert.Throws<ArgumentException>(() => new Map2D(-1, 1));
            Assert.Throws<ArgumentException>(() => new Map2D(1, -1));
            Assert.Throws<ArgumentException>(() => new Map2D(-1, -1));
        }

        [Test]
        public void Map2D_CellsNeighborsAreValid() {
            int rows = 5;
            int cols = 5;
            Map2D map = new Map2D(rows, cols);

            Assert.AreEqual(map.Cells.Count, rows * cols);
            for (int x = 0; x < rows; x++) {
                for (int y = 0; y < cols; y++) {
                    var cell = map[x, y];
                    var neighbors = map.Cells.Where(c =>
                        (c.X == cell.X && Math.Abs(c.Y - cell.Y) == 1) ||
                        (c.Y == cell.Y && Math.Abs(c.X - cell.X) == 1))
                            .ToList();
                    var nonNeighbors = map.Cells
                        .Where(c => !neighbors.Contains(c))
                        .ToList();
                    Assert.AreEqual(neighbors.Count, cell.Neighbors().Count);
                    Assert.IsTrue(neighbors.All(c => cell.Neighbors().Contains(c)));
                    Assert.IsFalse(nonNeighbors.Any(c => cell.Neighbors().Contains(c)));
                    if (x > 0) Assert.AreEqual(cell.Neighbors(Vector.North2D), map[x - 1, y]);
                    if (x + 1 < rows) Assert.AreEqual(cell.Neighbors(Vector.South2D), map[x + 1, y]);
                    if (y > 0) Assert.AreEqual(cell.Neighbors(Vector.West2D), map[x, y - 1]);
                    if (y + 1 > cols) Assert.AreEqual(cell.Neighbors(Vector.East2D), map[x, y + 1]);
                }
            }
        }
    }
}