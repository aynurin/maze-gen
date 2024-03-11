using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PlayersWorlds.Maps {

    [TestFixture]
    internal class CellTest : Test {
        [Test]
        public void CellTagEqualityTest() {
            var tag = new Cell.CellTag("test");
            Assert.That(tag.GetHashCode(), Is.EqualTo("test".GetHashCode()));
            Assert.That(tag.Equals("test"), Is.True);
            Assert.That(tag.ToString(), Is.EqualTo("CellTag('test')"));
        }
    }
}