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
            _cells = new NArray<Cell>(size, xy => new Cell());
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
        /// Renders the map to a string using a
        /// <see cref="Map2DStringRenderer" />.
        /// </summary>
        /// <returns>A string containing a rendered map.</returns>
        public override string ToString() {
            return new Map2DStringRenderer().Render(this);
        }
    }
}