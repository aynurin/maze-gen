using System;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class OptionalTest {
        private class A {
            public int Value { get; set; }
        }

        [Test]
        public void Optional_Equality() {
            var a = new A() { Value = 1 };
            var oa = new Optional<A>(a);
            Assert.AreEqual(oa, a);
            Assert.AreNotEqual(a, oa);
        }

        [Test]
        public void Optional_ThrowsIfNoValue() {
            var opt = Optional<Object>.Empty;
            Assert.IsFalse(opt.HasValue);
            Assert.Throws<InvalidOperationException>(() => { var shouldFail = opt.Value; });
        }

        [Test]
        public void Optional_Equals() {
            var a = new { x = 1 };
            var optA = new Optional<dynamic>(a);
            var optB = new Optional<dynamic>(a);
            Optional<dynamic> optNull = null;
            Assert.IsFalse(optA.Equals(optB));
            Assert.IsTrue(optA.Equals(a));
            Assert.IsFalse(a.Equals(optA));
            Assert.IsFalse(optA == null);
            Assert.IsTrue(optNull == null);
        }

        [Test]
        public void Optional_GetHashCode() {
            var a = new { x = 1 };
            var optA = new Optional<dynamic>(a);
            var optB = new Optional<dynamic>(a);
            Assert.AreEqual(optA.GetHashCode(), a.GetHashCode());
            Assert.AreEqual(optB.GetHashCode(), a.GetHashCode());
            Assert.AreNotEqual(Optional<dynamic>.Empty.GetHashCode(), a.GetHashCode());
        }

        [Test]
        public void Optional_CanBeCastToT() {
            string a = "abc";
            Optional<string> optA = a;
            Assert.AreEqual("abc", (string)optA);
        }

        [Test]
        public void Optional_ToStringShowsValue() {
            Optional<string> optA = "abc";
            Assert.AreEqual("Optional<String>(abc)", optA.ToString());
            Assert.AreEqual("Optional<String>(<empty>)", Optional<string>.Empty.ToString());
        }
    }
}