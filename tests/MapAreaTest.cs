using System;
using NUnit.Framework;

namespace Nour.Play.Areas {

    [TestFixture]
    internal class MapAreaTest {
        private static MapArea NewArea(int x, int y, int width, int height) {
            return new MapArea(AreaType.None, new Vector(width, height), new Vector(x, y));
        }

        [Test]
        public void Overlaps_ThrowsIfSame() {
            var area1 = NewArea(1, 1, 10, 10);
            var area2 = NewArea(1, 1, 10, 10);
            Assert.Throws<InvalidOperationException>(() => area1.Overlaps(area1));
            Assert.DoesNotThrow(() => area1.Overlaps(area2));
        }

        [Test]
        public void Overlaps_ReturnsTrueIfOverlaps() {
            Assert.IsTrue(NewArea(4, 4, 4, 4).Overlaps(NewArea(4, 4, 4, 4)));
            Assert.IsTrue(NewArea(4, 4, 4, 4).Overlaps(NewArea(1, 1, 4, 4)));
            Assert.IsTrue(NewArea(4, 4, 4, 4).Overlaps(NewArea(4, 1, 4, 4)));
            Assert.IsTrue(NewArea(4, 4, 4, 4).Overlaps(NewArea(7, 1, 4, 4)));
            Assert.IsTrue(NewArea(4, 4, 4, 4).Overlaps(NewArea(1, 7, 4, 4)));
            Assert.IsTrue(NewArea(4, 4, 4, 4).Overlaps(NewArea(4, 7, 4, 4)));
            Assert.IsTrue(NewArea(4, 4, 4, 4).Overlaps(NewArea(7, 7, 4, 4)));
        }

        [Test]
        public void Overlaps_ReturnsFalseIfDoesNotOverlap() {
            Assert.IsFalse(NewArea(4, 4, 4, 4).Overlaps(NewArea(0, 0, 4, 4)));
            Assert.IsFalse(NewArea(4, 4, 4, 4).Overlaps(NewArea(1, 0, 4, 4)));
            Assert.IsFalse(NewArea(4, 4, 4, 4).Overlaps(NewArea(4, 0, 4, 4)));
            Assert.IsFalse(NewArea(4, 4, 4, 4).Overlaps(NewArea(7, 0, 4, 4)));
            Assert.IsFalse(NewArea(4, 4, 4, 4).Overlaps(NewArea(1, 8, 4, 4)));
            Assert.IsFalse(NewArea(4, 4, 4, 4).Overlaps(NewArea(4, 8, 4, 4)));
            Assert.IsFalse(NewArea(4, 4, 4, 4).Overlaps(NewArea(7, 8, 4, 4)));
        }

        [Test]
        public void Position_ThrowsIfNotInitialized() {
            var area1 = new MapArea(AreaType.None, new Vector(10, 10));
            Assert.Throws<InvalidOperationException>(
                () => { var pos = area1.Position; });
            area1.Position = new Vector(1, 1);
            Assert.DoesNotThrow(
                () => { var pos = area1.Position; });
        }
    }
}