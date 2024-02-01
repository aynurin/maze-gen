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
        public IEnumerable<(Vector xy, T cell)> Iterate() {
            return IterateIntersection(Vector.Zero(Size.Value.Length), Size);
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
        public IEnumerable<(Vector xy, T cell)> Iterate(
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
        public IEnumerable<(Vector xy, T cell)>
        IterateIntersection(Vector xy, Vector size) {
            // Validate dimensions
            size.ThrowIfNotAValidSize();
            if (size.Value.Length != Size.Value.Length ||
                xy.Value.Length != Size.Value.Length) {
                throw new ArgumentException(
                    "Vector dimensions must match map dimensions " +
                    $"(NArray({Size}).IterateIntersection({xy}, {size}))");
            }

            var xyValue = xy.Value.Clone() as int[];
            var sizeValue = size.Value.Clone() as int[];
            for (var i = 0; i < Size.Value.Length; i++) {
                if (xyValue[i] < 0) {
                    sizeValue[i] = sizeValue[i] + xyValue[i];
                    xyValue[i] = 0;
                }
                if (xyValue[i] + sizeValue[i] > Size.Value[i]) {
                    sizeValue[i] = Size.Value[i] - xyValue[i];
                }
            }

            var current = xyValue.Clone() as int[];

            while (true) {
                var position = new Vector(current);
                yield return (position, this[position]);

                // Increment coordinates, wrapping back to 0 when reaching the
                // end of a dimension
                var i = current.Length - 1;
                while (i >= 0) {
                    current[i]++;
                    if (current[i] < (xyValue[i] + sizeValue[i])) {
                        // Complete if we haven't exceeded the end of the
                        // dimension
                        break;
                    }
                    // Move to the next dimension if we haven't reached the end
                    current[i] = xyValue[i]; // Wrap back to 0
                    i--;
                }

                // Iteration complete when all coordinates are back to 0
                if (i < 0) {
                    break;
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
        public IEnumerable<(Vector xy, T cell)>
        IterateAdjacentCells(Vector xy) {
            return IterateIntersection(
                    xy + new Vector(Enumerable.Repeat(-1, Size.Value.Length)),
                    new Vector(Enumerable.Repeat(3, Size.Value.Length)))
                .Where(cell => cell.xy != xy);
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