
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Nour.Play.Maze {
    public struct Point {
        private static readonly Point _empty = new Point();

        public static Point Empty => _empty;

        private int[] _value;

        public int[] Value => _value;

        public int Row => _value.Length == 2 ? _value[0] : throw new InvalidOperationException("Rows and columns are only supported in two-dimensional structures");
        public int Column => _value.Length == 2 ? _value[1] : throw new InvalidOperationException("Rows and columns are only supported in two-dimensional structures");

        public Point(int rows, int columns) : this(new int[] { rows, columns }) { }

        public Point(IEnumerable<int> coordinates) {
            _value = coordinates.ToArray();
        }

        public static bool operator ==(Point one, Point another) =>
            (one._value == null && another._value == null)
            || (one._value != null && another._value != null && one._value.SequenceEqual(another._value));
        public static bool operator !=(Point one, Point another) =>
            (one._value == null && another._value != null)
            || another._value == null
            || !one._value.SequenceEqual(another._value);
        public static bool operator >(Point one, Point another) =>
            one._value.Zip(another._value, (a, b) => a > b).All(_ => _);
        public static bool operator >=(Point one, Point another) =>
            one._value.Zip(another._value, (a, b) => a >= b).All(_ => _);
        public static bool operator <(Point one, Point another) =>
            one._value.Zip(another._value, (a, b) => a < b).All(_ => _);
        public static bool operator <=(Point one, Point another) =>
            one._value.Zip(another._value, (a, b) => a <= b).All(_ => _);
        public static Point operator +(Point one, Point another) =>
            new Point(one._value.Zip(another._value, (a, b) => a + b));
        public static Point operator -(Point one, Point another) =>
            new Point(one._value.Zip(another._value, (a, b) => a - b));
        public static Point operator +(Point one, Size another) =>
            new Point(one._value.Zip(another.Value, (a, b) => a + b));
        public static Point operator -(Point one, Size another) =>
            new Point(one._value.Zip(another.Value, (a, b) => a - b));

        public override bool Equals(object obj) => this == (Point)obj;
        public override int GetHashCode() => this._value.GetHashCode();
        public override string ToString() => _value.Length == 0 ? "<empty>" : String.Join("x", _value);
    }
}