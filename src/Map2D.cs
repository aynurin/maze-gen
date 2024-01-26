using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PlayersWorlds.Maps.Renderers;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// Represents a 2D map with cells that map 1-1 to game engine cells.
    /// </summary>
    public class Map2D {
        private readonly Cell[] _cells;

        /// <summary>
        /// Size of the map in cells.
        /// </summary>
        public Vector Size { get; private set; }

        /// <summary>
        /// A readonly access to the map cells.
        /// </summary>
        public IList<Cell> Cells => Array.AsReadOnly(_cells);

        /// <summary>
        /// Creates a new 2D map with the specified size.
        /// </summary>
        /// <param name="size">The size of the map as a <see cref="Vector"/> of
        /// dimensions.</param>
        public Map2D(Vector size) {
            Size = size;
            _cells = new Cell[size.Area];
            for (var i = 0; i < size.Area; i++) {
                _cells[i] = new Cell();
            }
        }

        /// <summary>
        /// Gets the value of a cell at the specified <see cref="Vector"/>
        /// position.
        /// </summary>
        /// <param name="xy">The position of the cell as a <see cref="Vector"/>.
        /// </param>
        /// <returns>The value of the cell at the specified position.</returns>
        /// <exception cref="IndexOutOfRangeException">The position is outside
        /// the map bounds.</exception>
        public Cell this[Vector xy] {
            get => _cells[xy.ToIndex(Size.X)];
        }

        /// <summary>
        /// Iterates over all cells in the map in row-major order, returning
        /// each cell's position and value as a tuple.
        /// </summary>
        /// <returns>An enumerable collection of tuples containing the cell
        /// position as a <see cref="Vector"/> and its value.</returns>
        public IEnumerable<(Vector xy, Cell cell)> Iterate() {
            for (var y = 0; y < Size.Y; y++) {
                for (var x = 0; x < Size.X; x++) {
                    var xy = new Vector(x, y);
                    yield return (xy, this[xy]);
                }
            }
        }

        /// <summary>
        /// Retrieve all cells of a rectangular region on the map.
        /// </summary>
        /// <param name="xy">Lowest XY position of the region.</param>
        /// <param name="areaSize">Size of the region.</param>
        /// <returns>An enumerable collection of tuples of the specified region
        /// containing the cell position as a <see cref="Vector"/> and its
        /// value.</returns>
        /// <exception cref="IndexOutOfRangeException">The coordinates are out
        /// of the bounds of the map.
        /// <see cref="IterateIntersection(Vector,Vector)" /> is an alternative
        /// that doesn't throw.</exception>
        public IEnumerable<(Vector xy, Cell cell)>
        IterateArea(Vector xy, Vector areaSize) {
            areaSize.ThrowIfNotAValidSize();
            if (xy.X < 0 ||
                xy.X + areaSize.X > Size.X ||
                xy.Y < 0 ||
                xy.Y + areaSize.Y > Size.Y) {
                throw new IndexOutOfRangeException($"Can't retrieve area of size {areaSize} at {xy} in map of size {Size}");
            }
            return IterateIntersection(xy, areaSize);
        }

        /// <summary>
        /// Retrieve all cells of a rectangular region on the map.
        /// </summary>
        /// <remarks>Retrieves any cells on the map that belong to the requested
        /// region.</remarks>
        /// <param name="xy">Lowest XY position of the region.</param>
        /// <param name="areaSize">Size of the region.</param>
        /// <returns><see cref="Cell" />s of the specified region.</returns>
        public IEnumerable<(Vector xy, Cell cell)>
        IterateIntersection(Vector xy, Vector areaSize) {
            areaSize.ThrowIfNotAValidSize();
            for (var x = Math.Max(0, xy.X); x < Math.Min(xy.X + areaSize.X, Size.X); x++) {
                for (var y = Math.Max(0, xy.Y); y < Math.Min(xy.Y + areaSize.Y, Size.Y); y++) {
                    var position = new Vector(x, y);
                    yield return (position, this[position]);
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
        public IEnumerable<(Vector xy, Cell cell)>
        IterateAdjacentCells(Vector xy) {
            return IterateIntersection(
                xy + Vector.SouthWest2D,
                new Vector(3, 3)).Where(cell => cell.xy != xy);
        }

        /// <summary>
        /// Renders the map to a string using a
        /// <see cref="Map2DStringRenderer" />.
        /// </summary>
        /// <returns>A string containing a rendered map.</returns>
        public override string ToString() {
            return new Map2DStringRenderer().Render(this);
        }
    }
}