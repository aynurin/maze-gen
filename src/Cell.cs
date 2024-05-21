using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// An <see cref="Area"/> cell.
    /// </summary>
    /// <remarks>
    /// For now it's only a set of tags assigned to the cell.
    /// </remarks>
    public class Cell : ExtensibleObject {
        private readonly List<Cell> _links = new List<Cell>();
        private readonly List<Cell> _neighbors = new List<Cell>();
        /// <summary>
        /// Tags assigned to cell.
        /// </summary>
        public List<CellTag> Tags { get; } = new List<CellTag>();
        private readonly Vector _position;
        private readonly List<Area> _childAreas = new List<Area>();

        /// <summary>
        /// Absolute position of this cell in the world.
        /// </summary>
        public Vector Position => _position;

        /// <summary>
        /// <c>true</c> if this cell has been visited by the maze generation
        /// algorithm.
        /// </summary>
        public bool IsConnected => _links.Count > 0;

        public List<Area> ChildAreas => _childAreas;
        /// <summary>
        /// Assign this cell to a <see cref="Area" />.
        /// </summary>
        // If the map area is not visitable, then we can't visit this cell, thus
        // it can't have neighbors and links.
        // This method will not propagate the Area to the neighbors.
        public void AddMapArea(Area area) {
            if (_childAreas.Contains(area)) {
                throw new InvalidOperationException(
                    "This area is already assigned to this cell");
            }
            _childAreas.Add(area);
        }

        /// <summary>
        /// Creates an instance of cell at the specified position.
        /// </summary>
        /// <param name="position">The position of the cell in its area.</param>
        /// <remarks>Supposed for internal use only.</remarks>
        internal Cell(Vector position) {
            _position = position;
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
        public void Link(Cell cell) {
            // check if the cell is a neighbor.
            if (!_neighbors.Contains(cell))
                throw new NotImplementedException(
                    "Linking with non-adjacent cells is not supported yet");
            // fool-proof to avoid double linking.
            if (_links.Contains(cell))
                throw new InvalidOperationException($"This link already exists ({this}->{cell})");

            _links.Add(cell);
            cell._links.Add(this);
        }

        // !! smellz?
        internal void LinkAllNeighborsInArea(Area area) {
            foreach (var neighbor in _neighbors) {
                if (_links.Contains(neighbor)) {
                    continue;
                }
                if (area.Cells.Any(c => (area.Position + c.Position) == neighbor.Position)) {
                    Link(neighbor);
                }
            }
        }

        /// <summary>
        /// Breaks a link between the cells.
        /// </summary>
        /// <param name="cell"></param>
        public void Unlink(Cell cell) {
            _links.Remove(cell);
            cell._links.Remove(this);
        }

        /// <summary>
        /// The neighbors of this cell.
        /// </summary>
        public IList<Cell> Neighbors() => _neighbors;

        /// <summary>
        /// Get a neighbor of this cell in the specified direction.
        /// </summary>
        /// <param name="unitVector">A vector that points to the neighbor. See
        /// the directional vectors predefined in the <see cref="Vector" />
        /// class.</param>
        public Optional<Cell> Neighbors(Vector unitVector) =>
            new Optional<Cell>(_neighbors.Find(
                cell => cell.Position ==
                        this.Position + unitVector));

        /// <summary>
        /// The links between this cell and other cells.
        /// </summary>
        public IList<Cell> Links() => _links.AsReadOnly();

        /// <summary>
        /// Get a linked cell in the specified direction.
        /// </summary>
        /// <param name="unitVector">A vector that points to the neighbor. See
        /// the directional vectors predefined in the <see cref="Vector" />
        /// class.</param>
        public Optional<Cell> Links(Vector unitVector) =>
            new Optional<Cell>(_links.Find(
                cell => cell.Position ==
                        this.Position + unitVector));

        override public string ToString() =>
            $"Cell({Position}{(IsConnected ? "V" : "")} " +
            $"[{string.Join(", ", Tags)}])";

        /// <summary>
        /// A debug string describing this cell.
        /// </summary>
        /// <returns></returns>
        public string ToLongString() =>
            $"Cell({Position}{(IsConnected ? "V" : "")}({GatesString()}) " +
            $"[{string.Join(", ", Tags)}])";

        private string GatesString() => string.Concat(
            Links(Vector.North2D).HasValue ? "N" : "-",
            Links(Vector.East2D).HasValue ? "E" : "-",
            Links(Vector.South2D).HasValue ? "S" : "-",
            Links(Vector.West2D).HasValue ? "W" : "-");

        /// <summary>
        /// Cell tags can be used in the game engine to choose objects, visual
        /// style, or behaviors associated with the generated cell. See
        /// <see cref="Cell.Tags"/>.
        /// </summary>
        /// <remarks>
        /// <p>Ideally we would want to define a more clear, strongly typed
        /// structure for the "tags" idea, but there is no clear understanding
        /// of the requirements for now so we will keep this simple.</p>
        /// </remarks>
        public class CellTag {
            private readonly string _tag;

            /// <summary />
            public CellTag(string tag) {
                tag.ThrowIfNullOrEmpty(nameof(tag));
                _tag = tag;
            }

            /// <summary>
            /// Compares this CellTag with another CellTag.
            /// </summary>
            /// <param name="obj">A CellTag to compare with.</param>
            /// <returns>
            /// <c>true</c> if the current CellTag is equal to the <paramref
            /// name="obj"/>; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals(object obj) {
                if (obj is string v) return _tag.Equals(v);
                return _tag.Equals((obj as CellTag)._tag);
            }

            /// <summary>
            /// Serves as the default hash function.
            /// </summary>
            /// <returns>A hash code for the current CellTag.</returns>
            public override int GetHashCode() {
                return _tag.GetHashCode();
            }

            /// <summary>
            /// Returns a string that represents this CellTag.
            /// </summary>
            /// <returns>A string that represents this CellTag.</returns>
            public override string ToString() {
                return "CellTag('" + _tag + "')";
            }

            /// <summary>
            /// CellTag denoting a maze wall.
            /// </summary>
            public static readonly CellTag MazeWall =
                new CellTag("MAZE2D_WALL");
            /// <summary>
            /// CellTag denoting a maze trail.
            /// </summary>
            public static readonly CellTag MazeTrail =
                new CellTag("MAZE2D_TRAIL");
            /// <summary>
            /// CellTag denoting a maze wall corner.
            /// </summary>
            public static readonly CellTag MazeWallCorner =
                new CellTag("MAZE2D_CORNER");
            /// <summary>
            /// CellTag denoting a void space in the maze.
            /// </summary>
            public static readonly CellTag MazeVoid =
                new CellTag("MAZE2D_VOID");
        }
    }
}