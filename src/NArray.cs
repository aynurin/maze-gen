using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// Represents an n-dimensional readonly array stored efficiently in a 1D
    /// array with indexed access using <see cref="Vector"/> coordinates.
    /// </summary>
    /// <remarks>
    /// There wasn't a need to make it n-dimensional at this stage, but I want
    /// to explore scenarios of storing more information in a map. E.g. every
    /// cell could contain data for more space, like space-associated data 
    /// (icebergs, volcanos), scaled sub spaces (Yennefer's tent,
    /// https://youtu.be/TAf8Z4aTOTM, or Hermione's bag), or even items present
    /// in the cell.
    /// </remarks>
    /// <typeparam name="T">The type of data stored in each cell of the map.
    /// </typeparam>
    public class NArray<T> : IReadOnlyList<T> {
        private readonly T[] _cells;

        /// <summary>
        /// Gets the size of the N-dimensional array.
        /// </summary>
        public Vector Size { get; }

        /// <inheritdoc />
        public int Count => _cells.Length;

        /// <summary>
        /// Gets the value of a cell at the specified <see cref="Vector"/>
        /// position.
        /// </summary>
        /// <param name="position">The location of the cell as a
        /// <see cref="Vector"/>.</param>
        /// <returns>The value of the cell at the specified position.</returns>
        /// <exception cref="IndexOutOfRangeException">The position is outside
        /// the map bounds.</exception>
        public T this[Vector position] => _cells[position.ToIndex(Size)];

        /// <inheritdoc />
        public T this[int index] => _cells[index];

        public IEnumerable<Vector> Positions =>
            _cells.Select((cell, index) => Vector.FromIndex(index, Size));

        /// <summary>
        /// Creates a new array with the specified size and optional initial
        /// value for each cell.
        /// </summary>
        /// <param name="size">The size of the map as a <see cref="Vector"/> of
        /// dimensions (rows, columns).</param>
        /// <param name="initialValue">The initial value for each cell.</param>
        public NArray(Vector size, Func<Vector, T> initialValue = null) {
            size.ThrowIfNotAValidSize();

            Size = size;

            _cells = new T[size.Area];

            for (var i = 0; i < _cells.Length; i++) {
                _cells[i] = initialValue == null ? default :
                    initialValue(Vector.FromIndex(i, size));
            }
        }

        /// <summary>
        /// Creates a copy of the provided <see cref="NArray{T}"/>.
        /// </summary>
        /// <param name="other">The source <see cref="NArray{T}"/>.</param>
        public NArray(NArray<T> other) {
            Size = other.Size;
            _cells = new T[Size.Area];
            Array.Copy(other._cells, _cells, _cells.Length);
        }

        /// <summary>
        /// Returns the position of the first occurrence of a value in the map.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Vector IndexOf(T value) =>
            Vector.FromIndex(
                    Array.FindIndex(_cells, x => x.Equals(value)), Size);

        /// <summary>
        /// Iterates over all cells in the map in row-major order, returning
        /// each cell's position and value as a tuple.
        /// </summary>
        /// <returns>An enumerable collection of tuples containing the item's
        /// position as a <see cref="Vector"/> and its value.</returns>
        public IEnumerable<Vector> Iterate() {
            return IterateIntersection(Vector.Zero(Size.Dimensions), Size);
        }

        /// <summary>
        /// Retrieve all cells of a rectangular region on the map.
        /// </summary>
        /// <param name="xy">Lowest XY position of the region.</param>
        /// <param name="size">Size of the region.</param>
        /// <returns>An enumerable collection of tuples of the specified region
        /// containing the cell position as a <see cref="Vector"/> and its
        /// value.</returns>
        /// <exception cref="IndexOutOfRangeException">The coordinates are out
        /// of the bounds of the map.
        /// <see cref="IterateIntersection(Vector,Vector)" /> is an alternative
        /// that doesn't throw.</exception>
        public IEnumerable<Vector> Iterate(
            Vector xy, Vector size) {
            size.ThrowIfNotAValidSize();
            if (xy.X < 0 ||
                xy.X + size.X > Size.X ||
                xy.Y < 0 ||
                xy.Y + size.Y > Size.Y) {
                throw new IndexOutOfRangeException(
                    $"Can't retrieve area of size {size} at {xy} " +
                    $"in map of size {Size}");
            }
            return IterateIntersection(xy, size);
        }

        // TODO: Test all iterators
        // TODO: Make all iterators use IterateIntersection to reduce code duplication

        /// <summary>
        /// Retrieve all cells of a rectangular region on the map.
        /// </summary>
        /// <remarks>Retrieves any cells on the map that belong to the requested
        /// region.</remarks>
        /// <param name="xy">Lowest XY position of the region.</param>
        /// <param name="size">Size of the region.</param>
        /// <returns><see cref="Cell" />s of the specified region.</returns>
        public IEnumerable<Vector>
        IterateIntersection(Vector xy, Vector size) {
            // Validate dimensions
            size.ThrowIfNotAValidSize();
            if (size.Dimensions != Size.Dimensions ||
                xy.Dimensions != Size.Dimensions) {
                throw new ArgumentException(
                    "Vector dimensions must match map dimensions " +
                    $"(NArray({Size}).IterateIntersection({xy}, {size}))");
            }

            var xyValue = new List<int>(xy.Value);
            var sizeValue = new List<int>(size.Value);
            for (var i = 0; i < Size.Dimensions; i++) {
                if (xyValue[i] < 0) {
                    sizeValue[i] = sizeValue[i] + xyValue[i];
                    xyValue[i] = 0;
                }
                if (xyValue[i] + sizeValue[i] > Size.Value[i]) {
                    sizeValue[i] = Size.Value[i] - xyValue[i];
                }
            }

            var current = new List<int>(xyValue);

            var dimension = 0;
            while (dimension < current.Count) {
                var position = new Vector(current);
                yield return position;

                // Increment coordinates, wrapping back to 0 when reaching the
                // end of a dimension
                for (dimension = 0; dimension < current.Count; dimension++) {
                    current[dimension]++;
                    if (current[dimension] <
                        (xyValue[dimension] + sizeValue[dimension])) {
                        // Complete if we haven't exceeded the end of the
                        // dimension
                        break;
                    }
                    // Move to the next dimension if we haven't reached the end
                    current[dimension] = xyValue[dimension]; // Wrap back to 0
                }
            }
        }

        /// <summary>
        /// Gets a list of <see cref="Vector"/> positions of all adjacent cells
        /// to the specified position, excluding cells outside the map bounds.
        /// </summary>
        /// <param name="xy">The position of the cell to get adjacent
        /// cells for as a <see cref="Vector"/>.</param>
        /// <returns>An enumerable collection of <see cref="Vector"/> positions
        /// of adjacent cells.</returns>
        public IEnumerable<Vector>
        IterateAdjacentCells(Vector xy) {
            return IterateIntersection(
                    xy + new Vector(Enumerable.Repeat(-1, Size.Dimensions)),
                    new Vector(Enumerable.Repeat(3, Size.Dimensions)))
                .Where(cell => cell != xy);
        }

        /// <summary>
        /// Scales the NArray to a larger size based on the provided vector.
        /// </summary>
        /// <remarks>This method scales to a larger size without interpolation.
        /// So the target size must be a multiple of the current size.</remarks>
        /// <param name="vector">The vector representing the new size of the
        /// NArray.</param>
        /// <returns>A new <see cref="NArray{T}"/> instance that is a scaled 
        /// version of the current NArray.</returns>
        public NArray<T> ScaleUp(Vector vector) {
            if (Size.Area == 0 || vector.Area == 0) {
                return new NArray<T>(vector);
            }
            var scale = vector.Value.Zip(Size.Value, (a, b) => a / b).ToArray();
            if (scale.Any(a => a <= 0)) {
                throw new ArgumentException(
                    "Can't scale up a NArray to a smaller size");
            }
            var scaledMap = new NArray<T>(vector, (xy) =>
                this[new Vector(xy.Value.Zip(scale, (a, b) => a / b))]);
            return scaledMap;
        }

        #region IReadOnlyList<T> 
        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return ((IEnumerable<T>)_cells).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _cells.GetEnumerator();
        }
        #endregion
    }
}