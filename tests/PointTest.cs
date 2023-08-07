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

        [Test]
        public void Point_ZeroPointIsNotEmpty() {
            Point p = new Point(new int[] { 0 });
            Assert.AreNotEqual(p, Point.Empty);
        }

        [Test]
        public void Point_EmptyPointIsEmpty() {
            Point p = new Point();
            Assert.AreEqual(p, Point.Empty);
        }

        [Test]
        public void Point_EmptyIsEmpty() {
            Assert.AreEqual(Point.Empty, Point.Empty);
        }

        [Test]
        public void Point_LessThanChecksAllComponents() {
            Point p0 = new Point(3, 2);
            Point p1 = new Point(2, 3);
            Point p2 = new Point(4, 5);
            Point p3 = new Point(3, 5);

            // <
            Assert.IsTrue(p1 < p2);
            Assert.IsFalse(p3 < p2);
            Assert.IsFalse(p0 < p1);
        }

        [Test]
        public void Point_GreaterThanChecksAllComponents() {
            Point p0 = new Point(3, 2);
            Point p1 = new Point(2, 3);
            Point p2 = new Point(4, 5);
            Point p3 = new Point(3, 5);

            // >
            Assert.IsTrue(p2 > p1);
            Assert.IsFalse(p2 > p3);
            Assert.IsFalse(p0 > p1);
        }

        [Test]
        public void Point_LessThanOrEqualsChecksAllComponents() {
            Point p0 = new Point(3, 2);
            Point p1 = new Point(2, 3);
            Point p2 = new Point(4, 5);
            Point p3 = new Point(3, 5);

            // <=
            Assert.IsTrue(p1 <= p3);
            Assert.IsTrue(p3 <= p2);
            Assert.IsFalse(p0 <= p1);
        }

        [Test]
        public void Point_GreaterThanOrEqualsChecksAllComponents() {
            Point p0 = new Point(3, 2);
            Point p1 = new Point(2, 3);
            Point p2 = new Point(4, 5);
            Point p3 = new Point(3, 5);

            // >=
            Assert.IsTrue(p2 >= p3);
            Assert.IsFalse(p3 >= p2);
            Assert.IsFalse(p0 >= p1);
        }
    }
}