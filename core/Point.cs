
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Nour.Play.Maze {
    public struct Point : IEquatable<Point> {
        private static readonly Point _empty = new Point();

        public static Point Empty => _empty;

        private short[] _value;

        public short[] Value => _value;

        public short Row => IsInitialized() && _value.Length == 2 ? _value[0] : throw new InvalidOperationException("Point is not initialized or contains more than two dimensions (rows and columns are only supported in two-dimensional structures)");
        public short Column => IsInitialized() && _value.Length == 2 ? _value[1] : throw new InvalidOperationException("Point is not initialized or contains more than two dimensions (rows and columns are only supported in two-dimensional structures)");

        public Point(short rows, short columns) : this(new short[] { rows, columns }) { }

        public Point(int rows, int columns) : this(new short[] { checked((short)rows), checked((short)columns) }) { }

        public Point(IEnumerable<short> coordinates) {
            _value = coordinates.ToArray();
        }

        public Point(IEnumerable<int> dimensions) : this(dimensions.Select(x => checked((short)x))) { }

        public bool IsInitialized() {
            if (_value == null) {
                throw new InvalidOperationException("Can't make operations with an empty Point");
            }
            return true;
        }

        public static bool operator ==(Point one, Point other) =>
            one.Equals(other);
        public static bool operator !=(Point one, Point other) =>
            (one._value == null && other._value != null)
            || other._value == null
            || !one.Equals(other);
        public static bool operator >(Point one, Point other) =>
            one.IsInitialized() && other.IsInitialized() &&
            one._value.Zip(other._value, (a, b) => a > b).All(_ => _);
        public static bool operator >=(Point one, Point other) =>
            one.IsInitialized() && other.IsInitialized() &&
            one._value.Zip(other._value, (a, b) => a >= b).All(_ => _);
        public static bool operator <(Point one, Point other) =>
            one.IsInitialized() && other.IsInitialized() &&
            one._value.Zip(other._value, (a, b) => a < b).All(_ => _);
        public static bool operator <=(Point one, Point other) =>
            one.IsInitialized() && other.IsInitialized() &&
            one._value.Zip(other._value, (a, b) => a <= b).All(_ => _);
        public static Point operator +(Point one, Point other) =>
            one.IsInitialized() && other.IsInitialized() ?
            new Point(one._value.Zip(other._value, (a, b) => a + b))
            : Point.Empty;
        public static Point operator -(Point one, Point other) =>
            one.IsInitialized() && other.IsInitialized() ?
            new Point(one._value.Zip(other._value, (a, b) => a - b))
            : Point.Empty;
        public static Point operator +(Point one, Size other) =>
            one.IsInitialized() && other.IsInitialized() ?
            new Point(one._value.Zip(other.Value, (a, b) => a + b))
            : Point.Empty;
        public static Point operator -(Point one, Size other) =>
            one.IsInitialized() && other.IsInitialized() ?
            new Point(one._value.Zip(other.Value, (a, b) => a - b))
            : Point.Empty;

        public override bool Equals(object obj) => this.Equals((Point)obj);
        public override int GetHashCode() => IsInitialized() ? ((IStructuralEquatable)_value).GetHashCode(EqualityComparer<short>.Default) : base.GetHashCode();
        public override string ToString() => IsInitialized() && _value.Length == 0 ? "<empty>" : String.Join("x", _value);

        public bool Equals(Point other) =>
            (this._value == null && other._value == null)
            || (this._value != null && other._value != null && this._value.SequenceEqual(other._value));
    }
}