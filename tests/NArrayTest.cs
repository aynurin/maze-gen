using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PlayersWorlds.Maps {

    [TestFixture]
    internal class NArrayTest : Test {
        [Test]
        public void NArray_Constructor_CreatesCorrectSize() {
            var size = new Vector(5, 3);
            var array = new NArray<int>(size);

            Assert.That(size, Is.EqualTo(array.Size));
            Assert.That(15, Is.EqualTo(array.Count())); // 5 * 3 elements
        }

        [Test]
        public void NArray_Constructor_SetsInitialValues() {
            var size = new Vector(2, 2);
            var initialValue = 10;
            var array = new NArray<int>(size, xy => initialValue);

            foreach (var cell in array) {
                Assert.That(initialValue, Is.EqualTo(array[cell]));
            }
        }

        [Test]
        public void NArray_Constructor_ThrowsOnInvalidSize() {
            Assert.That(
                () => new NArray<int>(new Vector(-1, 2)),
                Throws.ArgumentException);
        }

        [Test]
        public void NArray_Indexing_GetValuesCorrectly() {
            var size = new Vector(3, 3);
            var array = new NArray<Item>(size, xy => new Item(0));
            array[new Vector(1, 2)].Value = 20;

            Assert.That(array[new Vector(1, 2)].Value, Is.EqualTo(20));
        }

        [Test]
        public void NArray_Indexing_ThrowsOnOutOfBounds() {
            var size = new Vector(new int[] { 2, 2, 1 });
            var array = new NArray<int>(size);

            Assert.That(
                () => array[new Vector(new int[] { 3, 1, 1 })],
                Throws.InstanceOf<IndexOutOfRangeException>());

            Assert.That(
                () => array[new Vector(new int[] { 1, 3, 1 })],
                Throws.InstanceOf<IndexOutOfRangeException>());

            Assert.That(
                () => array[new Vector(new int[] { 1, 1, 2 })],
                Throws.InstanceOf<IndexOutOfRangeException>());

            Assert.That(
                () => array[new Vector(new int[] { 1, 1 })],
                Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public void NArray_Indexing_SetsValuesCorrectly() {
            var size = new Vector(3, 3);
            var array = new NArray<Item>(size, xy => new Item(0));
            array[new Vector(1, 2)].Value = 20;

            Assert.That(array[new Vector(1, 2)].Value, Is.EqualTo(20));
        }
        [Test]
        public void NArray_Iterate_TraversesAllCells() {
            var size = new Vector(new int[] { 2, 2, 3 });
            var array = new NArray<int>(size);
            var visited = new bool[size.Area];

            foreach (var cell in array) {
                Assert.That(visited[cell.ToIndex(size)], Is.False);
                visited[cell.ToIndex(size)] = true;
            }

            Assert.That(visited.All(v => v), Is.True);
        }

        [Test]
        public void NArray_IterateRegion_ReturnsCorrectCells() {
            var size = new Vector(3, 3);
            var array = new NArray<Item>(size, xy => new Item(0));
            array[new Vector(1, 1)].Value = 10;

            var region = new Vector(1, 1);
            var expectedCells = new[] { new Vector(1, 1) };

            var actualCells = array.Region(region, region).ToArray();
            Assert.That(expectedCells, Is.EqualTo(actualCells));
            Assert.That(array[actualCells.First()], Is.EqualTo(new Item(10)));
        }

        [Test]
        public void NArray_IterateRegion_HandlesOutOfBounds() {
            var array = new NArray<int>(new Vector(3, 3));

            var region = new Vector(4, 4);

            Assert.That(
                () => array.Region(new Vector(0, 0), region).ToArray(),
                Throws.InstanceOf<IndexOutOfRangeException>());

            Assert.That(
                () => array.Region(region, region).ToArray(),
                Throws.InstanceOf<IndexOutOfRangeException>());

            Assert.That(
                () => array.SafeRegion(new Vector(0, 0), region).ToArray(),
                Throws.Nothing);

            Assert.That(
                () => array.SafeRegion(region, region).ToArray(),
                Throws.Nothing);
        }

        [Test]
        public void NArray_IterateRegion_HandlesDimensionsMismatch() {
            var size = new Vector(new int[] { 3, 3, 3 });
            var array = new NArray<int>(size);

            var region1 = new Vector(new int[] { 1, 1 });
            var region2 = new Vector(new int[] { 1, 1, 1 });

            Assert.That(
                () => array.SafeRegion(region1, region2).ToList(),
                Throws.ArgumentException);

            Assert.That(
                () => array.SafeRegion(region2, region1).ToList(),
                Throws.ArgumentException);

            Assert.That(
                () => array.SafeRegion(region2, region2).ToList(),
                Throws.Nothing);
        }

        [Test]
        public void NArray_IterateIntersection_HandlesOverlap() {
            var size = new Vector(3, 3);
            var array = new NArray<Item>(size, xy => new Item(0));
            array[new Vector(1, 1)].Value = 10;

            var region = new Vector(0, 1);
            var size2 = new Vector(2, 1);
            var expectedCells = new[] { (new Vector(1, 1), new Item(10)) };

            var actualCells = array.SafeRegion(region, size2).ToArray();
            Assert.That(actualCells[1], Is.EqualTo(new Vector(1, 1)));
        }

        [Test]
        public void NArray_AdjacentRegion_ReturnsCorrectNeighbors() {
            var size = new Vector(3, 3);
            var array = new NArray<Item>(size, xy => new Item(0));
            array[new Vector(0, 0)].Value = 10;
            array[new Vector(1, 1)].Value = 20;
            array[new Vector(2, 2)].Value = 30;

            var actualCells = array.AdjacentRegion(new Vector(0, 0)).ToArray();
            Assert.That(actualCells, Has.Exactly(3).Items);
            Assert.That(array[actualCells[2]].Value, Is.EqualTo(20));

            actualCells = array.AdjacentRegion(new Vector(1, 1)).ToArray();
            Assert.That(actualCells, Has.Exactly(8).Items);
            Assert.That(
                actualCells.Select(cell => array[cell].Value).ToArray(),
                Is.EqualTo(new int[] { 10, 0, 0, 0, 0, 0, 0, 30 }));

            actualCells = array.AdjacentRegion(new Vector(2, 2)).ToArray();
            Assert.That(actualCells, Has.Exactly(3).Items);
            Assert.That(
                actualCells.Select(cell => array[cell].Value).ToArray(),
                Is.EqualTo(new int[] { 20, 0, 0 }));
        }

        [Test]
        public void NArray_AdjacentRegion_ReturnsCorrectNeighborsIn4DSpace() {
            var size = new Vector(new int[] { 3, 3, 3 });
            var array = new NArray<Item>(size, xy => new Item(0));
            array[new Vector(new int[] { 0, 0, 0 })].Value = 10;
            array[new Vector(new int[] { 1, 1, 1 })].Value = 20;
            array[new Vector(new int[] { 2, 2, 2 })].Value = 30;

            var actualCells = array.AdjacentRegion(new Vector(new int[] { 0, 0, 0 })).ToArray();
            Assert.That(actualCells, Has.Exactly(7).Items);
            Assert.That(array[actualCells[6]].Value, Is.EqualTo(20));

            actualCells = array.AdjacentRegion(new Vector(new int[] { 1, 1, 1 })).ToArray();
            Assert.That(actualCells, Has.Exactly(26).Items);
            Assert.That(
                actualCells.Select(cell => array[cell].Value).ToArray(),
                Is.EqualTo(new int[] { 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 30 }));

            actualCells = array.AdjacentRegion(new Vector(new int[] { 2, 2, 2 })).ToArray();
            Assert.That(actualCells, Has.Exactly(7).Items);
            Assert.That(
                actualCells.Select(cell => array[cell].Value).ToArray(),
                Is.EqualTo(new int[] { 20, 0, 0, 0, 0, 0, 0 }));
        }

        [Test]
        public void NArray_Iterate_OneCell() {
            var map = new NArray<TestCell>(new Vector(5, 5), xy => new TestCell(xy));
            var cells = map.Region(new Vector(0, 0), new Vector(1, 1)).ToList();
            var debugString = string.Join(",", cells.Select(c => c.ToIndex(map.Size)));
            Assert.That(1, Is.EqualTo(cells.Count()));
            Assert.That(cells.First(), Is.EqualTo(new Vector(0, 0)));
        }

        [Test]
        public void NArray_Iterate_TwoCells() {
            var map = new NArray<TestCell>(new Vector(5, 5), xy => new TestCell(xy));
            var cells = map.Region(new Vector(0, 0), new Vector(2, 1)).ToList();
            var debugString = string.Join(",", cells.Select(c => c.ToIndex(map.Size)));
            Assert.That(2, Is.EqualTo(cells.Count()));
            Assert.That(cells.First(), Is.EqualTo(new Vector(0, 0)), "0,0: " + debugString);
            Assert.That(cells.Last(), Is.EqualTo(new Vector(1, 0)), "1,0: " + debugString);
        }

        [Test]
        public void NArray_Iterate_TreeByTwo() {
            var map = new NArray<TestCell>(new Vector(5, 5), xy => new TestCell(xy));
            var cells = map.Region(new Vector(0, 0), new Vector(3, 2)).ToList();
            var debugString = string.Join(",", cells.Select(c => c.ToIndex(map.Size)));
            Assert.That(6, Is.EqualTo(cells.Count()));
            Assert.That(cells[0], Is.EqualTo(new Vector(0, 0)), "0,0: " + debugString);
            Assert.That(cells[1], Is.EqualTo(new Vector(1, 0)), "1,0: " + debugString);
            Assert.That(cells[2], Is.EqualTo(new Vector(2, 0)), "2,0: " + debugString);
            Assert.That(cells[3], Is.EqualTo(new Vector(0, 1)), "0,1: " + debugString);
            Assert.That(cells[4], Is.EqualTo(new Vector(1, 1)), "1,1: " + debugString);
            Assert.That(cells[5], Is.EqualTo(new Vector(2, 1)), "2,1: " + debugString);
        }

        [Test]
        public void NArray_Iterate_FourCellsFar() {
            var map = new NArray<TestCell>(new Vector(5, 5), xy => new TestCell(xy));
            var cells = map.Region(new Vector(3, 3), new Vector(2, 2)).ToList();
            var debugString = string.Join(",", cells.Select(c => c.ToIndex(map.Size)));
            Assert.That(4, Is.EqualTo(cells.Count()));
            Assert.That(cells[0], Is.EqualTo(new Vector(3, 3)), "3,3: " + debugString);
            Assert.That(cells[1], Is.EqualTo(new Vector(4, 3)), "4,3: " + debugString);
            Assert.That(cells[2], Is.EqualTo(new Vector(3, 4)), "3,4: " + debugString);
            Assert.That(cells[3], Is.EqualTo(new Vector(4, 4)), "4,4: " + debugString);
        }

        [Test]
        public void NArray_Iterate_OutOfBounds() {
            var map = new NArray<TestCell>(new Vector(5, 5), xy => new TestCell(xy));
            Assert.Throws<IndexOutOfRangeException>(() => map.Region(new Vector(5, 5), new Vector(1, 1)).ToList());
            Assert.Throws<IndexOutOfRangeException>(() => map.Region(new Vector(6, 3), new Vector(1, 1)).ToList());
            Assert.Throws<IndexOutOfRangeException>(() => map.Region(new Vector(0, 0), new Vector(6, 6)).ToList());
            Assert.Throws<IndexOutOfRangeException>(() => map.Region(new Vector(0, 0), new Vector(6, 1)).ToList());
        }

        [Test]
        public void NArray_IterateIntersection_AnyCellAtP1x1() {
            var map = new NArray<TestCell>(new Vector(5, 5), xy => new TestCell(xy));
            var cells = map.SafeRegion(new Vector(2, 2), new Vector(1, 1)).ToList();
            Assert.That(cells, Has.Exactly(1).Items);
            Assert.That(cells.First(), Is.EqualTo(new Vector(2, 2)));
        }

        [Test]
        public void CellsAt_AnyCellAt2x2() {
            var map = new NArray<TestCell>(new Vector(5, 5), xy => new TestCell(xy));
            var cells = map.SafeRegion(new Vector(2, 2), new Vector(2, 2)).ToList();
            Assert.That(cells, Has.Exactly(4).Items);
            Assert.That(cells[0], Is.EqualTo(new Vector(2, 2)));
            Assert.That(cells[1], Is.EqualTo(new Vector(3, 2)));
            Assert.That(cells[2], Is.EqualTo(new Vector(2, 3)));
            Assert.That(cells[3], Is.EqualTo(new Vector(3, 3)));
        }

        [Test]
        public void NArray_IterateIntersection_AnyCellAtEdge1x1() {
            var map = new NArray<TestCell>(new Vector(5, 5), xy => new TestCell(xy));
            var cells = map.SafeRegion(new Vector(4, 4), new Vector(1, 1)).ToList();
            Assert.That(cells, Has.Exactly(1).Items);
            Assert.That(cells[0], Is.EqualTo(new Vector(4, 4)));
        }

        [Test]
        public void NArray_IterateIntersection_AnyCellAtFarEdge2x2() {
            var map = new NArray<TestCell>(new Vector(5, 5), xy => new TestCell(xy));
            var cells = map.SafeRegion(new Vector(4, 4), new Vector(2, 2)).ToList();
            Assert.That(cells, Has.Exactly(1).Items);
            Assert.That(cells[0], Is.EqualTo(new Vector(4, 4)));
        }

        [Test]
        public void CellsAt_AnyCellAtCloseEdge2x2() {
            var map = new NArray<TestCell>(new Vector(5, 5), xy => new TestCell(xy));
            var cells = map.SafeRegion(new Vector(0, 0), new Vector(2, 2)).ToList();
            Assert.That(cells, Has.Exactly(4).Items);
            Assert.That(cells[0], Is.EqualTo(new Vector(0, 0)));
            Assert.That(cells[1], Is.EqualTo(new Vector(1, 0)));
            Assert.That(cells[2], Is.EqualTo(new Vector(0, 1)));
            Assert.That(cells[3], Is.EqualTo(new Vector(1, 1)));
        }

        [Test]
        public void SaveRegion_Empty() {
            var map = new NArray<TestCell>(new Vector(5, 5), xy => new TestCell(xy));
            Assert.That(() => map.SafeRegion(new Vector(2, 2), new Vector(0, 0)), Is.Empty);
        }

        class Item {
            public int Value { get; set; }

            public Item(int value) {
                Value = value;
            }

            public override bool Equals(object obj) {
                return obj is Item item && this.Value == item.Value;
            }

            override public int GetHashCode() {
                return Value.GetHashCode();
            }
        }

        class TestCell {
            private Vector _xy;

            public TestCell(Vector xy) {
                _xy = xy;
            }

            public Vector Xy { get => _xy; set => _xy = value; }
        }
    }
}