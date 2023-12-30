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
            Assert.That(floatingArea.Position, Is.EqualTo(expectedPosition).Within(VectorD.MIN));
            Assert.That(floatingArea.Size, Is.EqualTo(expectedSize).Within(VectorD.MIN));
        }

        [Test]
        public void Overlaps() {
            var area1 = FloatingArea.FromMapArea(new MapArea(AreaType.None, new Vector(10, 10), new Vector(0, 0)));
            var area2 = FloatingArea.FromMapArea(new MapArea(AreaType.None, new Vector(10, 10), new Vector(5, 5)));
            Assert.That(area1.Overlaps(area2), Is.True);
            Assert.Throws<InvalidOperationException>(() => area1.Overlaps(area1));
        }

        [Test]
        public void Contains() {
            var area1 = FloatingArea.FromMapArea(new MapArea(AreaType.None, new Vector(10, 10), new Vector(0, 0)));
            var area2 = FloatingArea.FromMapArea(new MapArea(AreaType.None, new Vector(10, 10), new Vector(5, 5)));
            Assert.That(area1.Contains(new VectorD(0.5D, 0.5D)), Is.True);
            Assert.That(area2.Contains(new VectorD(1.5D, 1.5D)), Is.False);
        }
    }
}