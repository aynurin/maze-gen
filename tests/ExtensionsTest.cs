using System.Collections.Generic;
using NUnit.Framework;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps {
    [TestFixture]
    public class ExtensionsTest {

        [Test]
        public void TryDequeue_DoesNotThrow() {
            var queue = new Queue<string>();
            Assert.That(queue.TryDequeue(out _), Is.False);
        }

        [Test]
        public void TryDequeue_Dequeues() {
            var queue = new Queue<string>();
            var a = "a";
            queue.Enqueue(a);
            Assert.That(queue.TryDequeue(out var element), Is.True);
            Assert.That(a, Is.EqualTo(element));
        }

        [Test]
        public void DebugString() {
            var o = new X { A = 1, B = "a" };
            var expectedLong =
                "PlayersWorlds.Maps.ExtensionsTest+X(\tA = 1\n, \tB = a\n)";
            var expectedShort =
                "PlayersWorlds.Maps.ExtensionsTest+X(1, a)";
            Assert.That(o.DebugString(), Is.EqualTo(expectedLong));
            Assert.That(o.ShortDebugString(), Is.EqualTo(expectedShort));
        }

        private class X {
            public int A { get; set; }
            public string B { get; set; }
        }
    }
}