using System;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class VectorTest {
        [Test]
        public void Vector_IsInitialized() {
            Vector p = new Vector(2, 5);
            Assert.AreEqual(p.X, 2);
            Assert.AreEqual(p.Y, 5);
        }

        [Test]
        public void Size_NotInitialized() {
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Vector.Empty.X, 2));
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Vector.Empty.Y, 2));
        }

        [Test]
        public void Vector_EqualityIsCheckedByValue() {
            Vector p1 = new Vector(1, 2);
            Vector p2 = new Vector(1, 2);
            Assert.AreEqual(p1, p2);
            Assert.IsTrue(p1 == p2);
            Assert.AreNotEqual(Vector.Empty, p2);
        }

        [Test]
        public void Vector_GetHashCodeIsDerivedFromValue() {
            Vector p1 = new Vector(1, 2);
            Vector p2 = new Vector(1, 2);
            Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
            Assert.Throws<InvalidOperationException>(() => Vector.Empty.GetHashCode());
        }

        [Test]
        public void Vector_ToStringFormat() {
            Vector p1 = new Vector(new int[] { 1, 2, 2, 3 });
            Assert.AreEqual(p1.ToString(), "1x2x2x3");
            Assert.AreEqual(Vector.Empty.ToString(), "<empty>");
        }

        [Test]
        public void Vector_ZeroVectorIsNotEmpty() {
            Vector p = new Vector(new int[] { 0 });
            Assert.AreNotEqual(p, Vector.Empty);
        }

        [Test]
        public void Vector_EmptyVectorIsEmpty() {
            Vector p = new Vector();
            Assert.AreEqual(p, Vector.Empty);
        }

        [Test]
        public void Vector_EmptyIsEmpty() {
            Assert.AreEqual(Vector.Empty, Vector.Empty);
        }

        [Test]
        public void Vector_LessThanChecksAllComponents() {
            Vector p0 = new Vector(3, 2);
            Vector p1 = new Vector(2, 3);
            Vector p2 = new Vector(4, 5);
            Vector p3 = new Vector(3, 5);

            // <
            Assert.IsTrue(p1 < p2);
            Assert.IsFalse(p3 < p2);
            Assert.IsFalse(p0 < p1);
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(Vector.Empty < p2));
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(p2 < Vector.Empty));
        }

        [Test]
        public void Vector_GreaterThanChecksAllComponents() {
            Vector p0 = new Vector(3, 2);
            Vector p1 = new Vector(2, 3);
            Vector p2 = new Vector(4, 5);
            Vector p3 = new Vector(3, 5);

            // >
            Assert.IsTrue(p2 > p1);
            Assert.IsFalse(p2 > p3);
            Assert.IsFalse(p0 > p1);
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(Vector.Empty > p2));
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(p2 > Vector.Empty));
        }

        [Test]
        public void Vector_LessThanOrEqualsChecksAllComponents() {
            Vector p0 = new Vector(3, 2);
            Vector p1 = new Vector(2, 3);
            Vector p2 = new Vector(4, 5);
            Vector p3 = new Vector(3, 5);

            // <=
            Assert.IsTrue(p1 <= p3);
            Assert.IsTrue(p3 <= p2);
            Assert.IsFalse(p0 <= p1);
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(Vector.Empty <= p2));
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(p2 <= Vector.Empty));
        }

        [Test]
        public void Vector_GreaterThanOrEqualsChecksAllComponents() {
            Vector p0 = new Vector(3, 2);
            Vector p1 = new Vector(2, 3);
            Vector p2 = new Vector(4, 5);
            Vector p3 = new Vector(3, 5);

            // >=
            Assert.IsTrue(p2 >= p3);
            Assert.IsFalse(p3 >= p2);
            Assert.IsFalse(p0 >= p1);
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(Vector.Empty >= p2));
            Assert.Throws<InvalidOperationException>(() => Assert.IsTrue(p2 >= Vector.Empty));
        }

        [Test]
        public void Vector_NotEqualToNull() {
            Vector p0 = new Vector(3, 2);
            Vector p1 = new Vector();

            Assert.IsNull(p1.Value);
            Assert.IsTrue(p1 != p0);
            Assert.IsTrue(p0 != p1);
        }

        [Test]
        public void Vector_CanAddOrSubtract() {
            Vector p0 = new Vector(3, 2);
            Vector p1 = new Vector(1, 2);
            Vector s1 = new Vector(3, 4);

            Assert.AreEqual(p0 + p1, new Vector(4, 4));
            Assert.AreEqual(p0 - p1, new Vector(2, 0));
            Assert.AreEqual(p0 + s1, new Vector(6, 6));
            Assert.AreEqual(p0 - s1, new Vector(0, -2));
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Vector.Empty + p1, new Vector(4, 4)));
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Vector.Empty + s1, new Vector(4, 4)));
        }
    }
}