using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PlayersWorlds.Maps {

    [TestFixture]
    internal class GlobalRandomTest {
        [Test]
        public void RandomCollectionItem() {
            var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var item = GlobalRandom.GetRandom(items);
            Assert.IsTrue(items.Contains(item));
        }
        [Test]
        public void ThrowsIfEmptyCollection() {
            var items = new List<int>();
            void Act() => GlobalRandom.GetRandom(items);
            Assert.That(Act, Throws.TypeOf<InvalidOperationException>());
        }
        [Test]
        public void RandomInRange() {
            var min = 1;
            var max = 10;
            for (var i = 0; i < 1000; i++) {
                var item = GlobalRandom.Next(min, max);
                Assert.That(item, Is.LessThan(max).And.GreaterThanOrEqualTo(min));
            }
        }
    }
}