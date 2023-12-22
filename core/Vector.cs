using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nour.Play {
    // ? Maybe find a better name
    public struct Vector : IEquatable<Vector> {
        public static readonly Vector Empty = new Vector();
        public static readonly Vector Zero2D = new Vector(0, 0);
        public static readonly Vector NorthWest2D = new Vector(-1, 1);
        public static readonly Vector North2D = new Vector(0, 1);
        public static readonly Vector NorthEast2D = new Vector(1, 1);
        public static readonly Vector West2D = new Vector(-1, 0);
        public static readonly Vector East2D = new Vector(1, 0);
        public static readonly Vector SouthEast2D = new Vector(-1, -1);
        public static readonly Vector South2D = new Vector(0, -1);
        public static readonly Vector SouthWest2D = new Vector(1, -1);

        private readonly int[] _value;
        private readonly bool _isInitialized; // false on initialization

        public int[] Value => _value;
        public bool IsEmpty => !_isInitialized;

        public bool IsTwoDimensional => !IsEmpty && _value.Length == 2;
        public bool IsThreeDimensional => !IsEmpty && _value.Length == 3;

        public int X => IsTwoDimensional || IsThreeDimensional ? _value[0] :
            throw new InvalidOperationException("X and Y are only supported in two- or three-dimensional space");
        public int Y => IsTwoDimensional || IsThreeDimensional ? _value[1] :
            throw new InvalidOperationException("X and Y are only supported in two- or three-dimensional space");
        public int Area => _value.Aggregate((a, b) => a * b);
        public int MagnitudeSq => _value.Sum(a => a * a);

        /// <param name="x">X coordinate (rows)</param>
        /// <param name="y">Y coordinate (columns)</param>
        public Vector(int x, int y) : this(new int[] { x, y }) { }

        public Vector(IEnumerable<int> dimensions) {
            if (dimensions == null) {
                throw new ArgumentNullException("dimensions");
            }
            _value = dimensions.ToArray();
            _isInitialized = true;
        }

        public void ThrowIfNotAValidSize() {
            if (_value.Length == 0 || _value.Any(i => i <= 0))
                throw new ArgumentException($"This Vector is not a valid size: {this}");
        }

        private static T ThrowIfEmptyOrApply<T>(Vector one, Vector another, Func<Vector, Vector, T> apply) {
            if (one.IsEmpty || another.IsEmpty)
                throw new InvalidOperationException("Cannot operate on an empty vector");
            return apply(one, another);
        }

        public static bool operator ==(Vector one, Vector another) =>
            one.Equals(another);
        public static bool operator !=(Vector one, Vector another) =>
            !one.Equals(another);
        public static Vector operator +(Vector one, Vector another) =>
            ThrowIfEmptyOrApply(one, another,
                (a, b) => new Vector(one._value.Zip(another._value, (x1, x2) => x1 + x2)));
        public static Vector operator -(Vector one, Vector another) =>
            ThrowIfEmptyOrApply(one, another, (a, b) => new Vector(one._value.Zip(another._value, (x1, x2) => x1 - x2)));

        public override bool Equals(object obj) => this.Equals((Vector)obj);
        public override int GetHashCode() =>
            _value == null ? base.GetHashCode() :
            IsEmpty ? _value.GetHashCode() :
            ((IStructuralEquatable)_value).GetHashCode(EqualityComparer<int>.Default);
        public override string ToString() => IsEmpty ? "<empty>" : _value.Length == 0 ? "00" : String.Join("x", _value);
        public bool Equals(Vector another) =>
            (this.IsEmpty && another.IsEmpty)
            || (!this.IsEmpty && !another.IsEmpty && this._value.SequenceEqual(another._value));

        internal int ToIndex(int maxX) {
            if (X > maxX) {
                throw new IndexOutOfRangeException($"Can't get index of vector {this} in a space that's limited by Xmax = {maxX}");
            }
            return Y * maxX + X;
        }

        internal static Vector FromIndex(int i, int maxX) {
            var x = i % maxX;
            var y = i / maxX;
            return new Vector(x, y);
        }
    }
}