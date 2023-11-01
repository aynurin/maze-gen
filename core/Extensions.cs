using System;
using System.Collections.Generic;
using System.Linq;

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

        public static void Set<K, V>(this Dictionary<K, V> dictionary,
            K key, V value) {
            if (dictionary.ContainsKey(key)) {
                dictionary[key] = value;
            } else {
                dictionary.Add(key, value);
            }
        }
    }
}