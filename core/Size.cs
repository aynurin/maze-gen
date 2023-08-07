using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Nour.Play.Maze {
    public struct Size : IEquatable<Size> {
        private static readonly Size _empty = new Size();

        public static Size Empty => _empty;

        private int[] _value;

        public int[] Value => _value;

        public int Rows => IsInitialized() && _value.Length == 2 ? _value[0] : throw new InvalidOperationException("Rows and columns are only supported in two-dimensional structures");
        public int Columns => IsInitialized() && _value.Length == 2 ? _value[1] : throw new InvalidOperationException("Rows and columns are only supported in two-dimensional structures");
        public int Area => IsInitialized() ? Rows * Columns : -1;

        public Size(int rows, int columns) : this(new int[] { rows, columns }) { }

        public Size(IEnumerable<int> dimensions) {
            _value = dimensions.ToArray();
        }

        public Size Rotate2d() {
            return new Size(Columns, Rows);
        }

        public bool IsInitialized() {
            if (_value == null) {
                throw new InvalidOperationException("Can't make operations with an empty Size");
            }
            return true;
        }

        public static bool operator ==(Size one, Size other) =>
            one.Equals(other);
        public static bool operator !=(Size one, Size other) =>
            (one._value == null && other._value != null)
            || other._value == null
            || !one.Equals(other);
        public static bool operator >(Size one, Size other) =>
            one.IsInitialized() && other.IsInitialized() &&
            one._value.Zip(other._value, (a, b) => a > b).All(_ => _);
        public static bool operator >=(Size one, Size other) =>
            one.IsInitialized() && other.IsInitialized() &&
            one._value.Zip(other._value, (a, b) => a >= b).All(_ => _);
        public static bool operator <(Size one, Size other) =>
            one.IsInitialized() && other.IsInitialized() &&
            one._value.Zip(other._value, (a, b) => a < b).All(_ => _);
        public static bool operator <=(Size one, Size other) =>
            one.IsInitialized() && other.IsInitialized() &&
            one._value.Zip(other._value, (a, b) => a <= b).All(_ => _);
        public static Size operator +(Size one, Size other) =>
            one.IsInitialized() && other.IsInitialized() ?
            new Size(one._value.Zip(other._value, (a, b) => a + b))
            : Size.Empty;
        public static Size operator -(Size one, Size other) =>
            one.IsInitialized() && other.IsInitialized() ?
            new Size(one._value.Zip(other._value, (a, b) => a - b))
            : Size.Empty;

        public override bool Equals(object obj) => this.Equals((Size)obj);
        public override int GetHashCode() => IsInitialized() ? this.Value.GetHashCode() : -1;
        public override string ToString() => IsInitialized() && _value.Length == 0 ? "<empty>" : String.Join("x", _value);

        public bool Equals(Size other) =>
            (this._value == null && other._value == null)
            || (this._value != null && other._value != null && this._value.SequenceEqual(other._value));
    }
}