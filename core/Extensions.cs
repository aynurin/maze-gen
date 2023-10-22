using System;
using System.Collections.Generic;

namespace Nour.Play {
    public static class Extensions {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            source.ThrowIfNull("source");
            action.ThrowIfNull("action");
            foreach (T element in source) {
                action(element);
            }
        }

        public static void ThrowIfNull(this Object item, string argName) {
            if (item == null) {
                throw new ArgumentNullException(argName);
            }
        }
    }
}