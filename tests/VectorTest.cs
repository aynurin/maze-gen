using System;
using NUnit.Framework;

namespace PlayersWorlds.Maps {
    [TestFixture]
    public class VectorTest {
        [Test]
        public void Vector_IsInitialized() {
            var p = new Vector(2, 5);
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
            var p1 = new Vector(1, 2);
            var p2 = new Vector(1, 2);
            Assert.AreEqual(p1, p2);
            Assert.IsTrue(p1 == p2);
            Assert.IsTrue(p1.Equals(p2));
            Assert.IsTrue(p1.Equals((object)p2));
            Assert.AreNotEqual(Vector.Empty, p2);
        }

        [Test]
        public void Vector_GetHashCodeIsDerivedFromValue() {
            var p1 = new Vector(1, 2);
            var p2 = new Vector(1, 2);
            Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
            Assert.DoesNotThrow(() => Vector.Empty.GetHashCode());
        }

        [Test]
        public void Vector_ToStringFormat() {
            var p1 = new Vector(new int[] { 1, 2, 2, 3 });
            Assert.AreEqual(p1.ToString(), "1x2x2x3");
            Assert.AreEqual(Vector.Empty.ToString(), "<empty>");
        }

        [Test]
        public void Vector_ZeroVectorIsNotEmpty() {
            var p = new Vector(new int[] { 0 });
            Assert.AreNotEqual(p, Vector.Empty);
        }

        [Test]
        public void Vector_EmptyVectorIsEmpty() {
            var p = new Vector();
            Assert.AreEqual(p, Vector.Empty);
        }

        [Test]
        public void Vector_EmptyIsEmpty() {
            Assert.AreEqual(Vector.Empty, Vector.Empty);
        }

        [Test]
        public void Vector_NotEqualToNull() {
            var p0 = new Vector(3, 2);
            var p1 = new Vector();

            Assert.IsNull(p1.Value);
            Assert.IsTrue(p1 != p0);
            Assert.IsTrue(p0 != p1);
        }

        [Test]
        public void Vector_CanAddAndSubtract() {
            var p0 = new Vector(3, 2);
            var p1 = new Vector(1, 2);
            var s1 = new Vector(3, 4);

            Assert.AreEqual(new Vector(4, 4), p0 + p1);
            Assert.AreEqual(new Vector(2, 0), p0 - p1);
            Assert.AreEqual(new Vector(6, 6), p0 + s1);
            Assert.AreEqual(new Vector(0, -2), p0 - s1);
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Vector.Empty + p1, new Vector(4, 4)));
            Assert.Throws<InvalidOperationException>(() => Assert.AreEqual(Vector.Empty + s1, new Vector(4, 4)));
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

        [Test]
        public void Vector_ToIndex() {
            Assert.That(new Vector(1, 2).ToIndex(3), Is.EqualTo(7));
            Assert.That(new Vector(2, 3).ToIndex(3), Is.EqualTo(11));
            Assert.Throws<IndexOutOfRangeException>(() => new Vector(5, 2).ToIndex(4));
        }

        [Test]
        public void Vector_FromIndex() {
            Assert.That(Vector.FromIndex(2, 3), Is.EqualTo(new Vector(2, 0)));
            Assert.That(Vector.FromIndex(5, 3), Is.EqualTo(new Vector(2, 1)));
        }

        [Test]
        public void VectorD_Parse() {
            Assert.AreEqual(new VectorD(0.3, -1), VectorD.Parse("S0.3x-1"));
        }
    }
}