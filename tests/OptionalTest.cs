using System;
using System.Security.AccessControl;
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
    }
}