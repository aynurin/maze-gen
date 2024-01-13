using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Represents a maze cell.
    /// </summary>
    public class MazeCell {
        private readonly List<MazeCell> _links = new List<MazeCell>();
        private readonly List<MazeCell> _neighbors = new List<MazeCell>();
        private ReadOnlyCollection<MazeCell> _mapAreaCells;
        private MapArea _mapArea;

        /// <summary>
        /// <see cref="PostProcessing" /> attributes of this cell.
        /// </summary>
        public Dictionary<string, string> Attributes { get; }
            = new Dictionary<string, string>();

        /// <summary />
        public Vector Coordinates { get; private set; }

        /// <summary>
        /// <c>true</c> if this cell has been visited by the maze generation
        /// algorithm.
        /// </summary>
        public bool IsVisited { get; private set; }

        /// <summary />
        public int X => Coordinates.X;
        /// <summary />
        public int Y => Coordinates.Y;

        /// <summary>
        /// If this cell is part of a <see cref="MapArea" />, this property
        /// returns the associated <see cref="MapArea" />.
        /// </summary>
        public MapArea MapArea => _mapArea;

        /// <summary>
        /// Assign this cell to a <see cref="MapArea" />.
        /// </summary>
        // If the map area is not visitable, then we can't visit this cell, thus
        // it can't have neighbors and links.
        // This method will not propagate the MapArea to the neighbors.
        public void AssignMapArea(MapArea area, IList<MazeCell> mapAreaCells) {
            if (_mapArea != null) {
                throw new InvalidOperationException(
                    $"Map area already assigned to this cell {this}");
            }
            _mapArea = area;
            _mapAreaCells = new ReadOnlyCollection<MazeCell>(mapAreaCells);
            if (_mapArea.Type == AreaType.Fill) {
                foreach (var neighbor in _neighbors) {
                    neighbor._neighbors.Remove(this);
                    neighbor._links.Remove(this);
                    if (neighbor._links.Count == 0) {
                        neighbor.IsVisited = false;
                    }
                }
                _neighbors.Clear();
                _links.Clear();
                this.IsVisited = false;
            } else if (_mapArea.Type == AreaType.Hall) {
                // We don't add back references because they will be added by
                // upstream.
                _links.AddRange(_neighbors.Where(n => _mapAreaCells.Contains(n)));
                IsVisited = true;
            }
        }

        internal MazeCell(int x, int y) : this(new Vector(x, y)) { }

        /// <summary>
        /// Creates a new instance of the <see cref="MazeCell" /> class using 
        /// a vector as cell coordinates.
        /// </summary>
        public MazeCell(Vector coordinates) {
            Coordinates = coordinates;
        }

        /// <summary>
        /// Creates a link between this cell and the specified cell making a
        /// path that can be used by the player to travel between the two cells.
        /// </summary>
        /// <exception cref="NotImplementedException">The cells are not adjacent
        /// and traveling between non-adjacent cells is not yet implemented.
        /// </exception>
        /// <exception cref="InvalidOperationException">The link already exists.
        /// </exception>
        public void Link(MazeCell cell) {
            // skip the cells in the same map area as they are already linked.
            if (_mapAreaCells?.Contains(cell) == true) return;
            // check if the cell is a neighbor.
            if (!_neighbors.Contains(cell))
                throw new NotImplementedException(
                    "Linking with non-adjacent cells is not supported yet");
            // fool-proof to avoid double linking.
            if (_links.Contains(cell))
                throw new InvalidOperationException($"This link already exists ({this}->{cell})");

            IsVisited = true;
            cell.IsVisited = true;
            _links.Add(cell);
            cell._links.Add(this);
        }

        /// <summary>
        /// Breaks a link between the cells.
        /// </summary>
        /// <param name="cell"></param>
        public void Unlink(MazeCell cell) {
            _links.Remove(cell);
            cell._links.Remove(this);
        }

        /// <summary>
        /// The neighbors of this cell.
        /// </summary>
        // TODO: this should be read-only.
        public IList<MazeCell> Neighbors() => _neighbors;

        /// <summary>
        /// Get a neighbor of this cell in the specified direction.
        /// </summary>
        /// <param name="unitVector">A vector that points to the neighbor. See
        /// the directional vectors predefined in the <see cref="Vector" />
        /// class.</param>
        public Optional<MazeCell> Neighbors(Vector unitVector) =>
            new Optional<MazeCell>(_neighbors.Find(cell => cell.Coordinates == this.Coordinates + unitVector));

        /// <summary>
        /// The links between this cell and other cells.
        /// </summary>
        public List<MazeCell> Links() => _links;

        /// <summary>
        /// Get a linked cell in the specified direction.
        /// </summary>
        /// <param name="unitVector">A vector that points to the neighbor. See
        /// the directional vectors predefined in the <see cref="Vector" />
        /// class.</param>
        public Optional<MazeCell> Links(Vector unitVector) =>
            new Optional<MazeCell>(_links.Find(cell => cell.Coordinates == this.Coordinates + unitVector));

        /// <summary>
        /// A debug string describing this cell.
        /// </summary>
        /// <returns></returns>
        public string ToLongString() => $"{ToString()}({GatesString()})";

        /// <summary>
        /// A string representation of this cell in the form of
        /// <c>Coordinates[V]</c>. Where V, if specified, denotes that this cell
        /// has been visited by a maze generator algorithm.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Coordinates}{(IsVisited ? "V" : "")}";

        private string GatesString() => string.Concat(
            Links(Vector.North2D).HasValue ? "N" : "-",
            Links(Vector.East2D).HasValue ? "E" : "-",
            Links(Vector.South2D).HasValue ? "S" : "-",
            Links(Vector.West2D).HasValue ? "W" : "-");
    }
}
