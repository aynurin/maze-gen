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
        private readonly List<Cell> _cells;

        /// <summary>
        /// Size of the map in cells.
        /// </summary>
        public Vector Size { get; private set; }

        /// <summary>
        /// A readonly access to the map cells.
        /// </summary>
        public IList<Cell> Cells => _cells.AsReadOnly();

        /// <summary />
        public Map2D(Vector size) {
            Size = size;
            _cells = new List<Cell>(size.Area);
            for (var i = 0; i < size.Area; i++) {
                _cells.Add(new Cell());
            }
        }

        /// <summary>
        /// Get a cell at the specified position.
        /// </summary>
        /// <param name="xy">Position of the cell on the map</param>
        /// <returns></returns>
        public Cell CellAt(Vector xy) => _cells[xy.ToIndex(Size.X)];

        /// <summary>
        /// Retrieve all cells of a rectangular region on the map.
        /// </summary>
        /// <param name="xy">Lowest XY position of the region.</param>
        /// <param name="areaSize">Size of the region.</param>
        /// <returns><see cref="Cell" />s of the specified region.</returns>
        /// <exception cref="IndexOutOfRangeException">The coordinates are out
        /// of the bounds of the map. <see cref="AnyCellsAt(Vector,Vector)" />
        /// is an alternative that doesn't throw.</exception>
        public IEnumerable<Cell> CellsAt(Vector xy, Vector areaSize) {
            if (xy.X < 0 || xy.X + areaSize.X > Size.X || xy.Y < 0 || xy.Y + areaSize.Y > Size.Y) {
                throw new IndexOutOfRangeException($"Can't retrieve area of size {areaSize} at {xy} in map of size {Size}");
            }
            return AnyCellsAt(xy, areaSize);
        }

        /// <summary>
        /// Retrieve all cells of a rectangular region on the map.
        /// </summary>
        /// <remarks>Retrieves any cells on the map that belong to the requested
        /// region.</remarks>
        /// <param name="xy">Lowest XY position of the region.</param>
        /// <param name="areaSize">Size of the region.</param>
        /// <returns><see cref="Cell" />s of the specified region.</returns>
        public IEnumerable<Cell> AnyCellsAt(Vector xy, Vector areaSize) {
            for (var x = Math.Max(0, xy.X); x < Math.Min(xy.X + areaSize.X, Size.X); x++) {
                for (var y = Math.Max(0, xy.Y); y < Math.Min(xy.Y + areaSize.Y, Size.Y); y++) {
                    yield return _cells[new Vector(x, y).ToIndex(Size.X)];
                }
            }
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