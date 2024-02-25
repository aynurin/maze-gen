using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PlayersWorlds.Maps.Areas.Evolving {

    [TestFixture]
    internal class FloatingAreaTest {
        [Test]
        public void Parse() {
            var data = "P0.53x7.5;S-3.01x0.01";
            var expectedPosition = new VectorD(0.53D, 7.5D);
            var expectedSize = new VectorD(-3.01D, 0.01D);
            var floatingArea = FloatingArea.Parse(data);
            Assert.That(floatingArea.Position, Is.EqualTo(expectedPosition));
            Assert.That(floatingArea.Size, Is.EqualTo(expectedSize));
        }

        [Test]
        public void ToStringIsValid() {
            var data = "P0.53x7.50;S-3.01x0.01";
            var floatingArea = FloatingArea.Parse(data);
            Assert.That(floatingArea.ToString(), Is.EqualTo(data));
        }

        [Test]
        public void Overlaps() {
            var area1 = FloatingArea.FromMapArea(MapArea.Create(
                AreaType.None, new Vector(0, 0), new Vector(10, 10)),
                Vector.Zero2D);
            var area2 = FloatingArea.FromMapArea(MapArea.Create(
                AreaType.None, new Vector(5, 5), new Vector(10, 10)),
                Vector.Zero2D);
            Assert.That(area1.Overlaps(area2), Is.True);
            Assert.Throws<InvalidOperationException>(() => area1.Overlaps(area1));
        }

        [Test]
        public void Contains() {
            var area1 = FloatingArea.FromMapArea(MapArea.Create(
                AreaType.None, new Vector(0, 0), new Vector(10, 10)),
                Vector.Zero2D);
            var area2 = FloatingArea.FromMapArea(MapArea.Create(
                AreaType.None, new Vector(5, 5), new Vector(10, 10)),
                Vector.Zero2D);
            Assert.That(area1.Contains(new VectorD(0.5D, 0.5D)), Is.True);
            Assert.That(area2.Contains(new VectorD(1.5D, 1.5D)), Is.False);
        }

        [Test]
        public void CenterIsValid() {
            Assert.That(
                FloatingArea.Unlinked(new VectorD(0, 0), new VectorD(4, 4))
                .Center, Is.EqualTo(new VectorD(2, 2)));
            Assert.That(
                FloatingArea.Unlinked(new VectorD(3, 3), new VectorD(4, 4))
                .Center, Is.EqualTo(new VectorD(5, 5)));
            Assert.That(
                FloatingArea.Unlinked(new VectorD(0, 0), new VectorD(5, 5))
                .Center, Is.EqualTo(new VectorD(2.5, 2.5)));
            Assert.That(
                FloatingArea.Unlinked(new VectorD(3, 3), new VectorD(5, 5))
                .Center, Is.EqualTo(new VectorD(5.5, 5.5)));
        }
    }
}