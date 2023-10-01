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
        public void Size_NotInitialized() {
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Point.Empty.Row, 2));
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Point.Empty.Column, 2));
        }

        [Test]
        public void Point_EqualityIsCheckedByValue() {
            Point p1 = new Point(1, 2);
            Point p2 = new Point(1, 2);
            Assert.AreEqual(p1, p2);
            Assert.IsTrue(p1 == p2);
            Assert.AreNotEqual(Point.Empty, p2);
        }

        [Test]
        public void Point_GetHashCodeIsDerivedFromValue() {
            Point p1 = new Point(1, 2);
            Point p2 = new Point(1, 2);
            Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
            Assert.Throws<InvalidOperationException>(() => Point.Empty.GetHashCode());
        }

        [Test]
        public void Point_ToStringFormat() {
            Point p1 = new Point(new int[] { 1, 2, 2, 3 });
            Assert.AreEqual(p1.ToString(), "1x2x2x3");
            Assert.Throws<InvalidOperationException>(() => Point.Empty.ToString());
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
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(Point.Empty < p2));
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(p2 < Point.Empty));
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
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(Point.Empty > p2));
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(p2 > Point.Empty));
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
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(Point.Empty <= p2));
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(p2 <= Point.Empty));
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
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(Point.Empty >= p2));
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(p2 >= Point.Empty));
        }

        [Test]
        public void Point_NotEqualToNull() {
            Point p0 = new Point(3, 2);
            Point p1 = new Point();

            Assert.IsNull(p1.Value);
            Assert.IsTrue(p1 != p0);
            Assert.IsTrue(p0 != p1);
        }

        [Test]
        public void Point_CanAddOrSubtract() {
            Point p0 = new Point(3, 2);
            Point p1 = new Point(1, 2);
            Size s1 = new Size(3, 4);

            Assert.AreEqual(p0 + p1, new Point(4, 4));
            Assert.AreEqual(p0 - p1, new Point(2, 0));
            Assert.AreEqual(p0 + s1, new Point(6, 6));
            Assert.AreEqual(p0 - s1, new Point(0, -2));
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Point.Empty + p1, new Point(4, 4)));
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Point.Empty + s1, new Point(4, 4)));
        }
    }
}