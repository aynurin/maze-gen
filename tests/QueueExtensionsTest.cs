using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Nour.Play.Maze;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class QueueExtensionsTest {

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