using System.Collections.Generic;
using NUnit.Framework;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps {
    [TestFixture]
    public class ExtensionsTest {

        [Test]
        public void TryDequeue_DoesNotThrow() {
            var queue = new Queue<string>();
            string element;
            Assert.IsFalse(queue.TryDequeue(out element));
        }

        [Test]
        public void TryDequeue_Dequeues() {
            var queue = new Queue<string>();
            var a = "a";
            queue.Enqueue(a);
            string element;
            Assert.IsTrue(queue.TryDequeue(out element));
            Assert.AreEqual(a, element);
        }
    }
}