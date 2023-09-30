using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nour.Play.Maze {
    public struct Size : IEquatable<Size> {
        private static readonly Size _empty = new Size();

        public static Size Empty => _empty;

        private byte[] _value;

        public byte[] Value => _value;

        public byte Rows => IsInitialized() && _value.Length == 2 ? _value[0] : throw new InvalidOperationException("Rows and columns are only supported in two-dimensional structures");
        public byte Columns => IsInitialized() && _value.Length == 2 ? _value[1] : throw new InvalidOperationException("Rows and columns are only supported in two-dimensional structures");
        public int Area => IsInitialized() ? Rows * Columns : -1;

        public Size(byte rows, byte columns) : this(new byte[] { rows, columns }) { }

        public Size(int rows, int columns) : this(new byte[] { checked((byte)rows), checked((byte)columns) }) { }

        public Size(IEnumerable<byte> dimensions) {
            _value = dimensions.ToArray();
        }

        public Size(IEnumerable<int> dimensions) : this(dimensions.Select(x => checked((byte)x))) { }

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
        public override int GetHashCode() => IsInitialized() ? ((IStructuralEquatable)_value).GetHashCode(EqualityComparer<byte>.Default) : -1;
        public override string ToString() => IsInitialized() && _value.Length == 0 ? "<empty>" : String.Join("x", _value);

        public bool Equals(Size other) =>
            (this._value == null && other._value == null)
            || (this._value != null && other._value != null && this._value.SequenceEqual(other._value));
    }
}