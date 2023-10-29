using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nour.Play {
    // ? Maybe find a better name
    public struct Vector : IEquatable<Vector> {
        public static readonly Vector Empty = new Vector();
        public static readonly Vector Zero2D = new Vector(0, 0);
        public static readonly Vector NorthWest2D = new Vector(-1, -1);
        public static readonly Vector North2D = new Vector(-1, 0);
        public static readonly Vector NorthEast2D = new Vector(-1, 1);
        public static readonly Vector West2D = new Vector(0, -1);
        public static readonly Vector East2D = new Vector(0, 1);
        public static readonly Vector SouthEast2D = new Vector(1, -1);
        public static readonly Vector South2D = new Vector(1, 0);
        public static readonly Vector SouthWest2D = new Vector(1, 1);

        private int[] _value;
        private bool _isInitialized; // false on initialization

        public int[] Value => _value;
        public bool IsEmpty => !_isInitialized;

        public bool IsTwoDimensional => !IsEmpty && _value.Length == 2;
        public bool IsThreeDimensional => !IsEmpty && _value.Length == 3;

        public int X => IsTwoDimensional || IsThreeDimensional ? _value[0] : throw new InvalidOperationException("X and Y are only supported in two- or three-dimensional space");
        public int Y => IsTwoDimensional || IsThreeDimensional ? _value[1] : throw new InvalidOperationException("X and Y are only supported in two- or three-dimensional space");
        public int Z => IsThreeDimensional ? _value[2] : throw new InvalidOperationException("Z iz only supported in a three-dimensional space");
        public int Area => _value.Aggregate((a, b) => a * b);

        /// <summary>
        /// 
        /// </summary>
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

        public void ThrowIfNot2D() {
            if (_value.Length != 2)
                throw new ArgumentException($"This Vector is not a 2D vector: {this}");
        }

        private static T ThrowIfEmptyOrApply<T>(Vector one, Vector another, Func<T> apply) {
            if (one.IsEmpty || another.IsEmpty)
                throw new InvalidOperationException("Cannot operate on an empty vector");
            return apply();
        }

        public static bool operator ==(Vector one, Vector another) =>
            one.Equals(another);
        public static bool operator !=(Vector one, Vector another) =>
            !one.Equals(another);
        public static bool operator >(Vector one, Vector another) =>
            one.IsEmpty && another.IsEmpty ? false :
            ThrowIfEmptyOrApply(one, another, () => one._value.Zip(another._value, (a, b) => a > b).All(_ => _));
        public static bool operator >=(Vector one, Vector another) =>
            one.IsEmpty && another.IsEmpty ? false :
            ThrowIfEmptyOrApply(one, another, () => one._value.Zip(another._value, (a, b) => a >= b).All(_ => _));
        public static bool operator <(Vector one, Vector another) =>
            one.IsEmpty && another.IsEmpty ? false :
            ThrowIfEmptyOrApply(one, another, () => one._value.Zip(another._value, (a, b) => a < b).All(_ => _));
        public static bool operator <=(Vector one, Vector another) =>
            one.IsEmpty && another.IsEmpty ? false :
            ThrowIfEmptyOrApply(one, another, () => one._value.Zip(another._value, (a, b) => a <= b).All(_ => _));
        public static Vector operator +(Vector one, Vector another) =>
            ThrowIfEmptyOrApply(one, another, () => new Vector(one._value.Zip(another._value, (a, b) => a + b)));
        public static Vector operator +(Vector one, int delta) =>
            ThrowIfEmptyOrApply(one, Vector.Zero2D, () => new Vector(one._value.Select(e => e + delta)));
        public static Vector operator +(int delta, Vector one) => one + delta;
        public static Vector operator -(Vector one, Vector another) =>
            ThrowIfEmptyOrApply(one, another, () => new Vector(one._value.Zip(another._value, (a, b) => a - b)));
        public static Vector operator -(Vector one, int delta) =>
            ThrowIfEmptyOrApply(one, Vector.Zero2D, () => new Vector(one._value.Select(e => e - delta)));
        public static Vector operator -(int delta, Vector one) =>
            ThrowIfEmptyOrApply(one, Vector.Zero2D, () => new Vector(one._value.Select(e => delta - e)));
        public static Vector operator /(Vector dividend, Vector divisor) =>
            ThrowIfEmptyOrApply(dividend, divisor, () => new Vector(dividend._value.Zip(divisor._value, (a, b) => a / b)));
        public static Vector operator /(Vector dividend, int divisor) =>
            ThrowIfEmptyOrApply(dividend, Vector.Zero2D,
            () => new Vector(dividend._value.Select(e => e % divisor != 0 ?
                throw new InvalidOperationException($"Can't divide with rounding (${e}/${divisor})") : e / divisor)));
        public static Vector operator *(Vector one, Vector another) =>
            ThrowIfEmptyOrApply(one, another, () => new Vector(one._value.Zip(another._value, (a, b) => a * b)));
        public static Vector operator *(Vector one, int another) =>
            ThrowIfEmptyOrApply(one, Vector.Zero2D, () => new Vector(one._value.Select(e => e * another)));
        public static Vector operator *(int one, Vector another) => another * one;

        public override bool Equals(object obj) => this.Equals((Vector)obj);
        public override int GetHashCode() =>
            IsEmpty ? throw new InvalidOperationException("Cannot get hash code of an empty vector") :
            ((IStructuralEquatable)_value).GetHashCode(EqualityComparer<int>.Default);
        public override string ToString() => IsEmpty ? "<empty>" : _value.Length == 0 ? "00" : String.Join("x", _value);
        public bool Equals(Vector another) =>
            (this.IsEmpty && another.IsEmpty)
            || (!this.IsEmpty && !another.IsEmpty && this._value.SequenceEqual(another._value));
    }
}