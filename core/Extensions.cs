using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nour.Play {
    public static class Extensions {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            source.ThrowIfNull("source");
            action.ThrowIfNull("action");
            foreach (T element in source) {
                action(element);
            }
        }

        public static void ThrowIfNull(this object item, string argName) {
            if (item == null) {
                throw new ArgumentNullException(argName);
            }
        }

        public static void ThrowIfNullOrEmpty(this IEnumerable item, string argName) {
            if (item == null) {
                throw new ArgumentNullException(argName);
            }
            if (!item.GetEnumerator().MoveNext()) {
                throw new ArgumentException(argName + " is empty.");
            }
        }

        public static BaseStats Stats(this IEnumerable<int> values) =>
            BaseStats.From(values.Select(x => (double)x));

        public static string DebugString(this object item) {
            return item.DebugString((member, value) => $"\t{member.Name} = {value}\n");
        }

        public static string ShortDebugString(this object item) {
            return item.DebugString((member, value) => value);
        }

        public static string DebugString(this object item, Func<MemberInfo, string, string> memberFormatter) {
            var members = item.GetType().GetMembers(
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);
            var values = new List<string>();
            foreach (var member in members) {
                object value;
                Type type;
                if (member is System.Reflection.FieldInfo field) {
                    value = field.GetValue(item);
                    type = field.FieldType;
                } else if (member is System.Reflection.PropertyInfo property) {
                    value = property.GetValue(item);
                    type = property.PropertyType;
                } else continue;
                string strValue;
                if (value == null) {
                    strValue = "<null>";
                } else if (value is ICollection collection) {
                    var generic = string.Join(",", type.GetGenericArguments().Select(a => a.Name));
                    strValue = type.Name +
                        (generic.Length > 0 ? $"<{generic}>" : "") +
                        "(" + string.Join(", ", collection.Cast<object>().Take(5)) +
                        (collection.Count > 5 ? $"...({collection.Count})" : "") + ")";
                } else {
                    strValue = value.ToString();
                }
                values.Add(memberFormatter(member, strValue));
            }
            var valuesStr = "(" + string.Join(", ", values.Take(50)) + (values.Count > 50 ? $"...({values.Count})" : "") + ")";
            return item.GetType().FullName + valuesStr;
        }

        public static void Set<K, V>(this Dictionary<K, V> dictionary,
            K key, V value) {
            if (dictionary.ContainsKey(key)) {
                dictionary[key] = value;
            } else {
                dictionary.Add(key, value);
            }
        }
        public static bool TryDequeue<T>(this Queue<T> queue, out T result) {
            try {
                result = queue.Dequeue();
                return true;
            } catch (InvalidOperationException) {
                result = default;
                return false;
            }
        }
    }

    public struct BaseStats {
        public double min;
        public double max;
        public double mean;
        public double median;
        public double mode;
        public double stddev;
        public double variance;
        public int count;

        public static BaseStats From(IEnumerable<double> values) {
            values.ThrowIfNull("values");
            var list = values.ToList();
            var stats = new BaseStats {
                count = list.Count
            };
            if (stats.count > 0) {
                stats.min = list.Min();
                stats.max = list.Max();
                stats.mean = list.Average();
                stats.median = list[list.Count / 2];
                stats.mode = list
                    .GroupBy(v => v)
                    .OrderByDescending(g => g.Count())
                    .First().Key;
                stats.variance = list.Select(v => v * v).Average() - stats.mean * stats.mean;
                stats.stddev = Math.Sqrt(stats.variance);
            }
            return stats;
        }
    }
}