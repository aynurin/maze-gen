using System;
using System.Linq;
using PlayersWorlds.Maps.Renderers;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// Represents a 2D map with cells that map 1-1 to game engine cells.
    /// </summary>
    public class Map2D {
        private readonly NArray<Cell> _cells;

        /// <summary>
        /// Size of the map in cells.
        /// </summary>
        public Vector Size { get; private set; }

        /// <summary>
        /// A readonly access to the map cells.
        /// </summary>
        public NArray<Cell> Cells => _cells;

        /// <summary>
        /// Creates a new 2D map with the specified size.
        /// </summary>
        /// <param name="size">The size of the map as a <see cref="Vector"/> of
        /// dimensions.</param>
        public Map2D(Vector size) {
            Size = size;
            _cells = new NArray<Cell>(size, xy => new Cell(xy));
        }

        /// <summary>
        /// Creates a new 2D map with the specified data.
        /// </summary>
        /// <param name="mapdata">The <see cref="NArray{T}"/> of map data.
        /// </param>
        public Map2D(NArray<Cell> mapdata) {
            Size = mapdata.Size;
            _cells = mapdata;
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
            get => _cells[xy];
        }

        /// <summary>
        /// Creates a deep copy of the current map.
        /// </summary>
        /// <returns>A new <see cref="Map2D"/> instance that is a deep copy of this map.</returns>
        public Map2D Clone() => new Map2D(new NArray<Cell>(Cells));

        /// <summary>
        /// Renders the map to a string using a
        /// <see cref="Map2DStringRenderer" />.
        /// </summary>
        /// <returns>A string containing a rendered map.</returns>
        public override string ToString() {
            return new Map2DStringRenderer().Render(this);
        }

        /// <summary>
        /// Scales current map to the specified size.
        /// </summary>
        /// <remarks>The size has to be a multiple of the current map size.
        /// </remarks>
        /// <param name="vector">The size of the saled map.</param>
        /// <returns>A new instance of <see cref="Map2D" /></returns>
        public Map2D Scale(Vector vector) {
            if (vector.Value.Zip(Size.Value,
                    (a, b) => a % b != 0 || a < b).Any()) {
                throw new ArgumentException(
                    "The specified size must be a greater multiple of the " +
                    $"current map size ({Size}). Provided {vector}",
                    nameof(vector));
            }

            return new Map2D(_cells.ScaleUp(vector));
        }
    }
}