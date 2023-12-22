using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class VectorDTest {
        [Test]
        public void IsInitialized() {
            var p = new VectorD(2.5, -5.1);
            Assert.That(p.X, Is.EqualTo(2.5).Within(VectorD.MIN));
            Assert.That(p.Y, Is.EqualTo(-5.1).Within(VectorD.MIN));
        }

        [Test]
        public void EqualityIsCheckedByValue() {
            var p1 = new VectorD(1.2, -2.8);
            var p2 = new VectorD(1.2, -2.8);
            Assert.AreEqual(p1, p2);
            Assert.IsTrue(p1 == p2);
            Assert.IsTrue(p1.Equals(p2));
            Assert.IsTrue(p1.Equals((object)p2));
        }

        [Test]
        public void GetHashCodeIsDerivedFromValue() {
            var p1 = new VectorD(1, 2);
            var p2 = new VectorD(1, 2);
            Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
        }

        [Test]
        public void ToStringFormat() {
            var p1 = new VectorD(new double[] { 1, 2, 2, 3 });
            Assert.AreEqual(p1.ToString(), "1.00x2.00x2.00x3.00");
        }

        [Test]
        public void InitializedEmpty() {
            var p1 = new VectorD();

            Assert.IsNull(p1.Value, "Value should be null");
            Assert.IsTrue(p1.IsEmpty, "IsEmpty should be true");
            Assert.IsTrue(p1 != VectorD.Zero2D, "p1!= VectorD.Zero2D");
            Assert.IsTrue(VectorD.Zero2D != p1, "VectorD.Zero2D!= p1");
        }

        [Test]
        public void InitializedAsNull() {
            IEnumerable<double> n = null;
            Assert.Throws<ArgumentNullException>(() => new VectorD(n));
        }

        [Test]
        public void IsZero() {
            var p1 = new VectorD(1, 2);
            var p2 = new VectorD(0, 0);
            Assert.IsFalse(p2.IsEmpty);
            Assert.IsTrue(p2.IsZero());
            Assert.IsFalse(p1.IsZero());
        }

        [Test]
        public void CanAddAndSubtract() {
            var p0 = new VectorD(3.5, -2.6);
            var p1 = new VectorD(1.1, 2.3);
            var s1 = new VectorD(-3.0, 4.7);

            Assert.AreEqual(new VectorD(4.6, -0.3), p0 + p1);
            Assert.AreEqual(new VectorD(2.4, -4.9), p0 - p1);
            Assert.AreEqual(new VectorD(0.5, 2.1), p0 + s1);
            Assert.AreEqual(new VectorD(6.5, -7.3), p0 - s1);

            Assert.Throws<InvalidOperationException>(() => { var _ = p0 + new VectorD(); });
        }

        [Test]
        public void ConstructorChecksArguments() {
            Assert.Throws<ArgumentNullException>(() => new VectorD(null));
        }

        [Test]
        public void SnappedForce() {
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
        public void VectorD_Parse() {
            Assert.AreEqual(new VectorD(0.3, -1), VectorD.Parse("S0.3x-1"));
            Assert.Throws<FormatException>(() => VectorD.Parse("wrong format"));
        }
    }
}