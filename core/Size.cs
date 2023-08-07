using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Nour.Play.Maze {
    public struct Size {
        private static readonly Point _empty = new Point();

        public static Point Empty => _empty;

        private int[] _value;

        public int[] Value => _value;

        public int Rows => _value.Length == 2 ? _value[0] : throw new InvalidOperationException("Rows and columns are only supported in two-dimensional structures");
        public int Columns => _value.Length == 2 ? _value[1] : throw new InvalidOperationException("Rows and columns are only supported in two-dimensional structures");
        public int Area => Rows * Columns;

        public Size(int rows, int columns) : this(new int[] { rows, columns }) { }

        public Size(IEnumerable<int> dimensions) {
            _value = dimensions.ToArray();
        }

        public Size Rotate2d() {
            return new Size(Columns, Rows);
        }

        public static bool operator ==(Size one, Size another) =>
            one._value.SequenceEqual(another._value);
        public static bool operator !=(Size one, Size another) =>
            !one._value.SequenceEqual(another._value);
        public static bool operator >(Size one, Size another) =>
            one._value.Zip(another._value, (a, b) => a > b).All(_ => _);
        public static bool operator >=(Size one, Size another) =>
            one._value.Zip(another._value, (a, b) => a >= b).All(_ => _);
        public static bool operator <(Size one, Size another) =>
            one._value.Zip(another._value, (a, b) => a < b).All(_ => _);
        public static bool operator <=(Size one, Size another) =>
            one._value.Zip(another._value, (a, b) => a <= b).All(_ => _);
        public static Size operator +(Size one, Size another) =>
            new Size(one._value.Zip(another._value, (a, b) => a + b));
        public static Size operator -(Size one, Size another) =>
            new Size(one._value.Zip(another._value, (a, b) => a - b));

        public override bool Equals(object obj) =>
            this._value.SequenceEqual(((Size)obj)._value);
        public override int GetHashCode() => this.Value.GetHashCode();
        public override string ToString() => _value.Length == 0 ? "<empty>" : String.Join("x", _value);
    }
}