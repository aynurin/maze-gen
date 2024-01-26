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
    }
}