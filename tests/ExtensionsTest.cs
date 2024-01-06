using System.Collections.Generic;
using NUnit.Framework;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps {
    [TestFixture]
    public class ExtensionsTest {

        [Test]
        public void TryDequeue_DoesNotThrow() {
            var queue = new Queue<string>();
            Assert.IsFalse(queue.TryDequeue(out _));
        }

        [Test]
        public void TryDequeue_Dequeues() {
            var queue = new Queue<string>();
            var a = "a";
            queue.Enqueue(a);
            Assert.IsTrue(queue.TryDequeue(out var element));
            Assert.AreEqual(a, element);
        }
    }
}