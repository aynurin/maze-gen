
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Nour.Play.Maze {
    public struct Point : IEquatable<Point> {
        private static readonly Point _empty = new Point();

        public static Point Empty => _empty;

        private byte[] _value;

        public byte[] Value => _value;

        public byte Row => IsInitialized() && _value.Length == 2 ? _value[0] : throw new InvalidOperationException("Point is not initialized or contains more than two dimensions (rows and columns are only supported in two-dimensional structures)");
        public byte Column => IsInitialized() && _value.Length == 2 ? _value[1] : throw new InvalidOperationException("Point is not initialized or contains more than two dimensions (rows and columns are only supported in two-dimensional structures)");

        public Point(byte rows, byte columns) : this(new byte[] { rows, columns }) { }

        public Point(int rows, int columns) : this(new byte[] { checked((byte)rows), checked((byte)columns) }) { }

        public Point(IEnumerable<byte> coordinates) {
            _value = coordinates.ToArray();
        }

        public Point(IEnumerable<int> dimensions) : this(dimensions.Select(x => checked((byte)x))) { }

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
        public override int GetHashCode() => IsInitialized() ? ((IStructuralEquatable)_value).GetHashCode(EqualityComparer<byte>.Default) : -1;
        public override string ToString() => IsInitialized() && _value.Length == 0 ? "<empty>" : String.Join("x", _value);

        public bool Equals(Point other) =>
            (this._value == null && other._value == null)
            || (this._value != null && other._value != null && this._value.SequenceEqual(other._value));
    }
}