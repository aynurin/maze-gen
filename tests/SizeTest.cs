using System;
using NUnit.Framework;

namespace Nour.Play.Maze {
    [TestFixture]
    public class SizeTest {
        [Test]
        public void Size_IsInitialized() {
            Size p = new Size(2, 5);
            Assert.AreEqual(p.Rows, 2);
            Assert.AreEqual(p.Columns, 5);
            Assert.AreEqual(p.Area, 10);
        }

        [Test]
        public void Size_NotInitialized() {
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Size.Empty.Rows, 2));
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Size.Empty.Columns, 2));
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Size.Empty.Area, 2));
        }

        [Test]
        public void Size_EqualityIsCheckedByValue() {
            Size p1 = new Size(1, 2);
            Size p2 = new Size(1, 2);
            Assert.AreEqual(p1, p2);
            Assert.IsTrue(p1 == p2);
            Assert.AreNotEqual(Size.Empty, p2);
        }

        [Test]
        public void Size_GetHashCodeIsDerivedFromValue() {
            Size p1 = new Size(1, 2);
            Assert.AreEqual(p1.GetHashCode(), p1.Value.GetHashCode());
            Assert.Throws<InvalidOperationException>(() => Size.Empty.GetHashCode());
        }

        [Test]
        public void Size_ToStringFormat() {
            Size p1 = new Size(new int[] { 1, 2, 2, 3 });
            Assert.AreEqual(p1.ToString(), "1x2x2x3");
            Assert.Throws<InvalidOperationException>(() => Size.Empty.ToString());
        }

        [Test]
        public void Size_ZeroSizeIsNotEmpty() {
            Size p = new Size(new int[] { 0 });
            Assert.AreNotEqual(p, Size.Empty);
        }

        [Test]
        public void Size_EmptySizeIsEmpty() {
            Size p = new Size();
            Assert.AreEqual(p, Size.Empty);
        }

        [Test]
        public void Size_EmptyIsEmpty() {
            Assert.AreEqual(Size.Empty, Size.Empty);
        }

        [Test]
        public void Size_LessThanChecksAllComponents() {
            Size p0 = new Size(3, 2);
            Size p1 = new Size(2, 3);
            Size p2 = new Size(4, 5);
            Size p3 = new Size(3, 5);

            // <
            Assert.IsTrue(p1 < p2);
            Assert.IsFalse(p3 < p2);
            Assert.IsFalse(p0 < p1);
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(Size.Empty < p2));
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(p2 < Size.Empty));
        }

        [Test]
        public void Size_GreaterThanChecksAllComponents() {
            Size p0 = new Size(3, 2);
            Size p1 = new Size(2, 3);
            Size p2 = new Size(4, 5);
            Size p3 = new Size(3, 5);

            // >
            Assert.IsTrue(p2 > p1);
            Assert.IsFalse(p2 > p3);
            Assert.IsFalse(p0 > p1);
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(Size.Empty > p2));
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(p2 > Size.Empty));
        }

        [Test]
        public void Size_LessThanOrEqualsChecksAllComponents() {
            Size p0 = new Size(3, 2);
            Size p1 = new Size(2, 3);
            Size p2 = new Size(4, 5);
            Size p3 = new Size(3, 5);

            // <=
            Assert.IsTrue(p1 <= p3);
            Assert.IsTrue(p3 <= p2);
            Assert.IsFalse(p0 <= p1);
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(Size.Empty <= p2));
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(p2 <= Size.Empty));
        }

        [Test]
        public void Size_GreaterThanOrEqualsChecksAllComponents() {
            Size p0 = new Size(3, 2);
            Size p1 = new Size(2, 3);
            Size p2 = new Size(4, 5);
            Size p3 = new Size(3, 5);

            // >=
            Assert.IsTrue(p2 >= p3);
            Assert.IsFalse(p3 >= p2);
            Assert.IsFalse(p0 >= p1);
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(Size.Empty >= p2));
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(p2 >= Size.Empty));
        }

        [Test]
        public void Size_NotEqualToNull() {
            Size p0 = new Size(3, 2);
            Size p1 = new Size();

            Assert.IsNull(p1.Value);
            Assert.IsTrue(p1 != p0);
            Assert.IsTrue(p0 != p1);
        }

        [Test]
        public void Size_CanAddOrSubtract() {
            Size p0 = new Size(3, 2);
            Size p1 = new Size(1, 2);

            Assert.AreEqual(p0 + p1, new Size(4, 4));
            Assert.AreEqual(p0 - p1, new Size(2, 0));
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Size.Empty + p1, new Size(4, 4)));
        }

        [Test]
        public void Size_Rotate2d() {
            Size p0 = new Size(3, 2);
            Size p1 = new Size(2, 3);

            Assert.AreEqual(p0.Rotate2d(), p1);
        }
    }
}