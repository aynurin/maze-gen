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
    /// (icebergs, volcanos), scaled sub spaces (Hermione's bag), or simply
    /// items in the cell.
    /// </remarks>
    /// <typeparam name="T">The type of data stored in each cell of the map.
    /// </typeparam>
    public class NArray<T> : IReadOnlyCollection<Vector> {
        private readonly T[] _cellData;

        /// <inheritdoc />
        public int Count => _cellData.Length;

        /// <summary>
        /// Gets the size of the N-dimensional array.
        /// </summary>
        public Vector Size { get; }

        /// <summary>
        /// Gets the value of a cell at the specified <see cref="Vector"/>
        /// position.
        /// </summary>
        /// <param name="position">The location of the cell as a
        /// <see cref="Vector"/>.</param>
        /// <returns>The value of the cell at the specified position.</returns>
        /// <exception cref="IndexOutOfRangeException">The position is outside
        /// the map bounds.</exception>
        public T this[Vector position] => _cellData[position.ToIndex(Size)];

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

            _cellData = new T[size.Area];

            for (var i = 0; i < _cellData.Length; i++) {
                _cellData[i] = initialValue == null ? default :
                    initialValue(Vector.FromIndex(i, size));
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
        public IEnumerable<Vector> AdjacentRegion(Vector xy) {
            return SafeRegion(
                    xy + new Vector(Enumerable.Repeat(-1, Size.Dimensions)),
                    new Vector(Enumerable.Repeat(3, Size.Dimensions)))
                .Where(cell => cell != xy);
        }

        /// <summary>
        /// Retrieve all cells of a rectangular region on the map.
        /// </summary>
        /// <param name="xy">Lowest XY position of the region.</param>
        /// <param name="size">Size of the region.</param>
        /// <returns>An enumerable collection of tuples of the specified region
        /// containing the cell position as a <see cref="Vector"/> and its
        /// value.</returns>
        public IEnumerable<Vector> SafeRegion(
            Vector xy, Vector size) {
            size.ThrowIfNotAValidSize();
            if (xy.Dimensions != size.Dimensions || xy.Dimensions != Size.Dimensions) {
                throw new ArgumentException(
                    $"The position and size of the region ({size}) must have " +
                    $"the same number of dimensions as the size of the map " +
                    $"(xy={xy}, size={size}, map size={Size}).");
            }

            var xyValue = new List<int>(xy.Value);
            var sizeValue = new List<int>(size.Value);
            for (var i = 0; i < Size.Dimensions; i++) {
                if (xyValue[i] < 0) {
                    sizeValue[i] = sizeValue[i] + xyValue[i];
                    xyValue[i] = 0;
                }
                if (xyValue[i] + sizeValue[i] > Size.Value[i]) {
                    sizeValue[i] = Math.Max(0, Size.Value[i] - xyValue[i]);
                }
            }

            return Region(new Vector(xyValue), new Vector(sizeValue));
        }

        /// <summary>
        /// Retrieve all cells of a rectangular region on the map.
        /// </summary>
        /// <remarks>Retrieves any cells on the map that belong to the requested
        /// region.</remarks>
        /// <param name="xy">Lowest XY position of the region.</param>
        /// <param name="size">Size of the region.</param>
        /// <returns><see cref="Cell" />s of the specified region.</returns>
        /// <exception cref="IndexOutOfRangeException">The coordinates are out
        /// of the bounds of the map.
        /// <see cref="SafeRegion(Vector,Vector)" /> is an alternative
        /// that doesn't throw.</exception>
        public IEnumerable<Vector> Region(Vector xy, Vector size) {
            // Validate dimensions
            size.ThrowIfNotAValidSize();
            if (size.Area == 0) {
                yield break;
            }
            if (size.Dimensions != Size.Dimensions ||
                xy.Dimensions != Size.Dimensions) {
                throw new ArgumentException(
                    "Vector dimensions must match map dimensions " +
                    $"(NArray({Size}).SafeRegion({xy}, {size}))");
            }
            if (xy.Value.Any(x => x < 0) ||
                xy.Value.Select(
                    (x, i) => (xy.Value[i] + size.Value[i]) > Size.Value[i])
                        .Any(_ => _)) {
                throw new IndexOutOfRangeException(
                    $"Can't retrieve area of size {size} at {xy} " +
                    $"in map of size {Size}");
            }

            var xyValue = new List<int>(xy.Value);
            var sizeValue = new List<int>(size.Value);

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

        #region IReadOnlyCollection<Vector> 
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerator<Vector> GetEnumerator() {
            for (var i = 0; i < _cellData.Length; i++) {
                yield return Vector.FromIndex(i, Size);
            }
        }
        #endregion
    }
}