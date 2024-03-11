using System;
using NUnit.Framework;

namespace PlayersWorlds.Maps.Areas {

    [TestFixture]
    internal class MapAreaTest : Test {
        private static MapArea NewArea(int x, int y, int width, int height) {
            return MapArea.Create(
                AreaType.None, new Vector(x, y), new Vector(width, height));
        }

        [Test]
        public void Overlaps_ThrowsIfSame() {
            var area1 = NewArea(1, 1, 10, 10);
            var area2 = NewArea(1, 1, 10, 10);
            Assert.Throws<InvalidOperationException>(
                () => area1.Overlap(area1));
            Assert.DoesNotThrow(() => area1.Overlap(area2));
        }

        [Test]
        public void Overlaps_ReturnsTrueIfOverlaps() {
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(4, 4, 4, 4)).Area, Is.GreaterThan(0));
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(1, 1, 4, 4)).Area, Is.GreaterThan(0));
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(4, 1, 4, 4)).Area, Is.GreaterThan(0));
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(7, 1, 4, 4)).Area, Is.GreaterThan(0));
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(1, 7, 4, 4)).Area, Is.GreaterThan(0));
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(4, 7, 4, 4)).Area, Is.GreaterThan(0));
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(7, 7, 4, 4)).Area, Is.GreaterThan(0));

        }

        [Test]
        public void Overlaps_ReturnsFalseIfDoesNotOverlap() {
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(0, 0, 4, 4)).Area, Is.Not.GreaterThan(0));
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(1, 0, 4, 4)).Area, Is.Not.GreaterThan(0));
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(4, 0, 4, 4)).Area, Is.Not.GreaterThan(0));
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(7, 0, 4, 4)).Area, Is.Not.GreaterThan(0));
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(1, 8, 4, 4)).Area, Is.Not.GreaterThan(0));
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(4, 8, 4, 4)).Area, Is.Not.GreaterThan(0));
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(7, 8, 4, 4)).Area, Is.Not.GreaterThan(0));

            // Touches:
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(1, 4, 3, 3)).Area, Is.Not.GreaterThan(0), "Map areas touch (case 1)");
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(5, 3, 2, 1)).Area, Is.Not.GreaterThan(0), "Map areas touch (case 2)");
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(8, 5, 3, 2)).Area, Is.Not.GreaterThan(0), "Map areas touch (case 3)");
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(5, 8, 1, 4)).Area, Is.Not.GreaterThan(0), "Map areas touch (case 4)");

            // Does not touch:
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(0, 4, 3, 3)).Area, Is.Not.GreaterThan(0));
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(5, 2, 2, 1)).Area, Is.Not.GreaterThan(0));
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(9, 5, 3, 2)).Area, Is.Not.GreaterThan(0));
            Assert.That(NewArea(4, 4, 4, 4).Overlap(NewArea(5, 9, 1, 4)).Area, Is.Not.GreaterThan(0));
        }

        [Test]
        public void Position_ThrowsIfNotInitialized() {
            var area1 = MapArea.CreateAutoPositioned(
                AreaType.None, new Vector(10, 10));
            Assert.That(() => { var pos = area1.Position; },
                Throws.TypeOf<InvalidOperationException>());
            area1.Position = new Vector(1, 1);
            Assert.That(
                () => { var pos = area1.Position; }, Throws.Nothing);
        }

        [Test]
        public void ThrowsIfWrongParameters() {
            Assert.That(() =>
                MapArea.Create(
                    AreaType.None,
                    Vector.Empty,
                    new Vector(10, 10)),
                Throws.TypeOf<ArgumentException>());
        }
    }
}