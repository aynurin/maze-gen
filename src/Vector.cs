using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// A coordinate on a map, or a size of an object on the map.
    /// </summary>
    /// <remarks>
    /// This class is not supposed to be used for vector maths.
    /// See <a href="https://www.nuget.org/packages/System.Numerics.Vectors/"
    /// title="System.Numerics.Vectors on NuGet">System.Numerics.Vectors</a>
    /// for that.
    /// </remarks>
    public readonly struct Vector : IEquatable<Vector> {
        /// <summary>
        /// Empty instance of <see cref="Vector" />.
        /// </summary>
        public static readonly Vector Empty = new Vector();
        /// <summary>
        /// Represents a 2-dimensional zero vector.
        /// </summary>
        public static readonly Vector Zero2D = new Vector(0, 0);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the top-left corner on the
        /// euclidean plane (-1, 1).
        /// </summary>
        public static readonly Vector NorthWest2D = new Vector(-1, 1);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the top on the
        /// euclidean plane (0, 1).
        /// </summary>
        public static readonly Vector North2D = new Vector(0, 1);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the top-right on the
        /// euclidean plane (1, 1).
        /// </summary>
        public static readonly Vector NorthEast2D = new Vector(1, 1);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the left on the
        /// euclidean plane (-1, 0).
        /// </summary>
        public static readonly Vector West2D = new Vector(-1, 0);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the right on the
        /// euclidean plane (1, 0).
        /// </summary>
        public static readonly Vector East2D = new Vector(1, 0);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the bottom-left on the
        /// euclidean plane (-1, -1).
        /// </summary>
        public static readonly Vector SouthWest2D = new Vector(-1, -1);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the bottom on the
        /// euclidean plane (0, -1).
        /// </summary>
        public static readonly Vector South2D = new Vector(0, -1);
        /// <summary>
        /// A <see cref="Vector" /> pointing to the bottom-right on the
        /// euclidean plane (1, -1).
        /// </summary>
        public static readonly Vector SouthEast2D = new Vector(1, -1);

        private readonly int[] _value;
        private readonly bool _isInitialized; // false on initialization

        /// <summary>
        /// Components of this vector.
        /// </summary>
        internal int[] Value => _value;
        /// <summary>
        /// The vector is not empty only if it was initialized with components.
        /// </summary>
        public bool IsEmpty => !_isInitialized;
        /// <summary>
        /// Returns a first component of a non-empty vector.
        /// </summary>
        public int X => !IsEmpty && _value.Length > 0 ? _value[0] :
            throw new InvalidOperationException("X is only supported in non-empty vectors");
        /// <summary>
        /// Returns a second component of a non-empty vector.
        /// </summary>
        public int Y => !IsEmpty && _value.Length > 1 ? _value[1] :
            throw new InvalidOperationException("Y is only supported in two- or more dimensional space");
        /// <summary>
        /// A product of the components of this vector.
        /// </summary>
        public int Area => _value.Aggregate((a, b) => a * b);
        /// <summary>
        /// Squared magnitude of this vector.
        /// </summary>
        public int MagnitudeSq => _value.Sum(a => a * a);

        /// <param name="x">X coordinate (rows)</param>
        /// <param name="y">Y coordinate (columns)</param>
        public Vector(int x, int y) : this(new int[] { x, y }) { }

        /// <summary>
        /// Creates a new vector with the specified components.
        /// </summary>
        /// <param name="dimensions">Components of the vector</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dimensions" /> is null.</exception>
        public Vector(IEnumerable<int> dimensions) {
            dimensions.ThrowIfNull(nameof(dimensions));
            _value = dimensions.ToArray();
            _isInitialized = _value.Length > 0;
        }

        /// <summary>
        /// Checks if this vector can be used as a size, i.e. it has components,
        /// and all components are greater than zero.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void ThrowIfNotAValidSize() {
            if (_value.Length == 0 || _value.Any(i => i <= 0))
                throw new ArgumentException($"This Vector is not a valid size: {this}");
        }

        private static T ThrowIfEmptyOrApply<T>(Vector one, Vector another, Func<Vector, Vector, T> apply) {
            if (one.IsEmpty || another.IsEmpty)
                throw new InvalidOperationException("Cannot operate on an empty vector");
            return apply(one, another);
        }

        /// <summary>
        /// <see cref="Equals(Vector)"/>.
        /// </summary>
        public static bool operator ==(Vector one, Vector another) =>
            one.Equals(another);

        /// <summary>
        /// <see cref="Equals(Vector)"/>.
        /// </summary>
        public static bool operator !=(Vector one, Vector another) =>
            !one.Equals(another);

        /// <summary>
        /// Add two Vectors together.
        /// </summary>
        /// <param name="one">The first Vector</param>
        /// <param name="another">The second Vector</param>
        /// <returns>A new Vector that is the sum of the two input Vectors
        /// </returns>
        public static Vector operator +(Vector one, Vector another) =>
            ThrowIfEmptyOrApply(one, another,
                (a, b) => new Vector(one._value.Zip(another._value, (x1, x2) => x1 + x2)));

        /// <summary>
        /// Subtracts one vector from another by subtracting each component of
        /// the right-hand-side vector from the left-hand-side vector.
        /// </summary>
        /// <param name="one">The vector to subtract from</param>
        /// <param name="another">The vector to subtract</param>
        /// <returns>A new vector with the difference</returns>
        public static Vector operator -(Vector one, Vector another) =>
            ThrowIfEmptyOrApply(one, another, (a, b) => new Vector(one._value.Zip(another._value, (x1, x2) => x1 - x2)));

        /// <summary>
        /// Checks if a region of size <paramref name="container"/> fits a region
        /// of the size of this vector.
        /// </summary>
        public bool FitsInto(Vector container) => _value.Zip(container._value, (a, b) => a <= b).All(b => b);

        /// <summary>
        /// <see cref="Equals(Vector)"/>.
        /// </summary>
        public override bool Equals(object obj) => this.Equals((Vector)obj);

        /// <summary>
        /// Returns a hash code of this vector based on its components. If no
        /// components, returns the hash code of the empty int[].
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() =>
            _value == null ? base.GetHashCode() :
            IsEmpty ? _value.GetHashCode() :
            ((IStructuralEquatable)_value).GetHashCode(EqualityComparer<int>.Default);

        /// <summary>
        /// Returns a string representation of this vector of the form
        /// <c>x,y</c>.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => IsEmpty ? "<empty>" : _value.Length == 0 ? "00" : string.Join("x", _value);

        /// <summary>
        /// Checks if two vectors have the same components.
        /// </summary>
        public bool Equals(Vector another) =>
            (this.IsEmpty && another.IsEmpty)
            || (!this.IsEmpty && !another.IsEmpty && this._value.SequenceEqual(another._value));

        internal int ToIndex(int maxX) {
            if (X > maxX) {
                throw new IndexOutOfRangeException($"Can't get index of vector {this} in a space that's limited by X(max) = {maxX}");
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