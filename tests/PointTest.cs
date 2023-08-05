using System;
using NUnit.Framework;

namespace Nour.Play.Maze {
    [TestFixture]
    public class PointTest {
        [Test]
        public void Point_IsInitialized() {
            Point p = new Point(2, 5);
            Assert.AreEqual(p.Row, 2);
            Assert.AreEqual(p.Column, 5);
        }
    }
}