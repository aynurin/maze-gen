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
            Assert.DoesNotThrow(() => Vector.Empty.GetHashCode());
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
        public void Vector_CanAddAndSubtract() {
            Vector p0 = new Vector(3, 2);
            Vector p1 = new Vector(1, 2);
            Vector s1 = new Vector(3, 4);

            Assert.AreEqual(new Vector(4, 4), p0 + p1);
            Assert.AreEqual(new Vector(2, 0), p0 - p1);
            Assert.AreEqual(new Vector(6, 6), p0 + s1);
            Assert.AreEqual(new Vector(0, -2), p0 - s1);
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Vector.Empty + p1, new Vector(4, 4)));
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Vector.Empty + s1, new Vector(4, 4)));

            Assert.AreEqual(new Vector(2, 1), p0 - 1);
            Assert.AreEqual(new Vector(-2, -1), 1 - p0);
            Assert.AreEqual(new Vector(4, 3), p0 + 1);
            Assert.AreEqual(new Vector(4, 3), 1 + p0);
        }

        [Test]
        public void Vector_CanDivideAndMultiply() {
            Vector one = new Vector(3, 2);
            Vector other = new Vector(1, 2);

            Assert.AreEqual(new Vector(3, 1), one / other);
            Assert.AreEqual(new Vector(2, 1), new Vector(4, 2) / 2);
            Assert.Throws<InvalidOperationException>(() => { var x = one / 2; });

            Assert.AreEqual(new Vector(3, 4), one * other);
            Assert.AreEqual(new Vector(6, 4), one * 2);
            Assert.AreEqual(new Vector(6, 4), 2 * one);
            Assert.AreEqual(new Vector(-1, -1), (Vector.Zero2D - one) / one);
        }

        [Test]
        public void Vector_ConstructorChecksArguments() {
            Assert.Throws<ArgumentNullException>(() => new Vector(null));
        }

        [Test]
        public void Vector_ThrowsIfNotAValidSize() {
            Assert.Throws<ArgumentException>(() => new Vector(new int[] { 0, 1, 2, 3 }).ThrowIfNotAValidSize());
            Assert.Throws<ArgumentException>(() => new Vector(new int[] { 1, -1 }).ThrowIfNotAValidSize());
            Assert.DoesNotThrow(() => new Vector(new int[] { 1 }).ThrowIfNotAValidSize());
        }

        [Test]
        public void Vector_ThrowsIfNot2D() {
            Assert.Throws<ArgumentException>(() => new Vector(new int[] { 0, 1, 2, 3 }).ThrowIfNot2D());
            Assert.DoesNotThrow(() => new Vector(new int[] { 1, -1 }).ThrowIfNot2D());
            Assert.Throws<ArgumentException>(() => new Vector(new int[] { 1 }).ThrowIfNot2D());
        }

        [Test]
        public void VectorD_NotZero() {
            var arg = new VectorD(0, 0);
            var direction = new VectorD(-1, 1);
            var expected = new VectorD(-0.1, 0.1);
            var actual = arg.NotZero(direction);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Vector_SnappedForce() {
            Assert.AreEqual(new Vector(-4, -2), new VectorD(new double[] { 2, 1 }).WithMagnitude(-5).RoundToInt());
            Assert.AreEqual(new Vector(-4, -2), new VectorD(new double[] { -2, -1 }).WithMagnitude(5).RoundToInt());
            Assert.AreEqual(new Vector(4, 2), new VectorD(new double[] { -2, -1 }).WithMagnitude(-5).RoundToInt());
            Assert.AreEqual(new Vector(4, 2), new VectorD(new double[] { 2, 1 }).WithMagnitude(5).RoundToInt());
            Assert.AreEqual(new Vector(2, 1), new VectorD(new double[] { 10, 5 }).WithMagnitude(2.5).RoundToInt());
            Assert.AreEqual(new Vector(0, 0), new VectorD(new double[] { -8, 4 }).WithMagnitude(0).RoundToInt());
            Assert.AreEqual(new Vector(0, 1), new VectorD(new double[] { 0, -10 }).WithMagnitude(-1).RoundToInt());
            Assert.AreEqual(new Vector(0, 0), new VectorD(new double[] { 0, 0 }).WithMagnitude(1044).RoundToInt());
            Assert.AreEqual(new Vector(566, 566), new VectorD(new double[] { -3, -3 }).WithMagnitude(-800).RoundToInt());
        }
    }
}