using System;
using System.Collections.Generic;

namespace Nour.Play.Maze {
    internal static class QueueExtensions {
        public static bool TryDequeue<T>(this Queue<T> queue, out T result) {
            try {
                result = queue.Dequeue();
                return true;
            } catch (InvalidOperationException) {
                result = default(T);
                return false;
            }
        }
    }
}