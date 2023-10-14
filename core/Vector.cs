using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nour.Play {
    // ? Maybe find a better name
    public struct Vector : IEquatable<Vector> {
        private static readonly Vector _empty = new Vector();

        public static Vector Empty => _empty;

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

        public static bool operator ==(Vector one, Vector other) =>
            one.Equals(other);
        public static bool operator !=(Vector one, Vector other) =>
            !one.Equals(other);
        public static bool operator >(Vector one, Vector other) =>
            one.IsEmpty && other.IsEmpty ? false :
            one.IsEmpty || other.IsEmpty ? throw new InvalidOperationException("Cannot compare with an empty vector") :
            one._value.Zip(other._value, (a, b) => a > b).All(_ => _);
        public static bool operator >=(Vector one, Vector other) =>
            one.IsEmpty && other.IsEmpty ? false :
            one.IsEmpty || other.IsEmpty ? throw new InvalidOperationException("Cannot compare with an empty vector") :
            one._value.Zip(other._value, (a, b) => a >= b).All(_ => _);
        public static bool operator <(Vector one, Vector other) =>
            one.IsEmpty && other.IsEmpty ? false :
            one.IsEmpty || other.IsEmpty ? throw new InvalidOperationException("Cannot compare with an empty vector") :
            one._value.Zip(other._value, (a, b) => a < b).All(_ => _);
        public static bool operator <=(Vector one, Vector other) =>
            one.IsEmpty && other.IsEmpty ? false :
            one.IsEmpty || other.IsEmpty ? throw new InvalidOperationException("Cannot compare with an empty vector") :
            one._value.Zip(other._value, (a, b) => a <= b).All(_ => _);
        public static Vector operator +(Vector one, Vector other) =>
            one.IsEmpty || other.IsEmpty ? throw new InvalidOperationException("Cannot operate on an empty vector") :
            new Vector(one._value.Zip(other._value, (a, b) => a + b));
        public static Vector operator -(Vector one, Vector other) =>
            one.IsEmpty || other.IsEmpty ? throw new InvalidOperationException("Cannot operate on an empty vector") :
            new Vector(one._value.Zip(other._value, (a, b) => a - b));

        public override bool Equals(object obj) => this.Equals((Vector)obj);
        public override int GetHashCode() =>
            IsEmpty ? throw new InvalidOperationException("Cannot get hash code of an empty vector") :
            ((IStructuralEquatable)_value).GetHashCode(EqualityComparer<int>.Default);
        public override string ToString() => IsEmpty ? "<empty>" : _value.Length == 0 ? "00" : String.Join("x", _value);

        public bool Equals(Vector other) =>
            (this.IsEmpty && other.IsEmpty)
            || (!this.IsEmpty && !other.IsEmpty && this._value.SequenceEqual(other._value));
    }
}