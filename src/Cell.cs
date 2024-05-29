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
        private readonly Vector _position;
        private readonly Area _owningArea;
        private readonly List<CellTag> _tags = new List<CellTag>();
        private readonly List<Vector> _links = new List<Vector>();
        private readonly List<Vector> _neighbors = new List<Vector>();

        /// <summary>
        /// Absolute position of this cell in the world.
        /// </summary>
        public Vector Position => _position;

        /// <summary>
        /// Area that owns this cell.
        /// </summary>
        public Area OwningArea => _owningArea;

        /// <summary>
        /// Tags assigned to cell.
        /// </summary>
        public List<CellTag> Tags => _tags;

        /// <summary>
        /// <c>true</c> if this cell has been visited by the maze generation
        /// algorithm.
        /// </summary>
        public bool IsConnected => _links.Count > 0;

        public Cell Parent =>
                _owningArea.Parent[_owningArea.Position + _position];

        /// <summary>
        /// Creates an instance of cell at the specified position.
        /// </summary>
        /// <param name="position">The position of the cell in its area.</param>
        /// <param name="owningArea">Area that owns this cell.</param>
        /// <remarks>Supposed for internal use only.</remarks>
        public Cell(Vector position, Area owningArea) {
            _position = position;
            _owningArea = owningArea;
        }

        internal Cell CloneWithParent(Area area) {
            var newCell = new Cell(_position, area);
            newCell._tags.AddRange(_tags);
            newCell._links.AddRange(_links);
            newCell._neighbors.AddRange(_neighbors);
            return newCell;
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
        public void Link(Vector other) {
            // check if the cell is a neighbor.
            if (!_neighbors.Contains(other))
                throw new NotImplementedException(
                    "Linking with non-adjacent cells is not supported yet (" +
                    $"Trying to link {other} to {this}). Neighbors: {string.Join(", ", _neighbors)}");
            // fool-proof to avoid double linking.
            if (_links.Contains(other))
                throw new InvalidOperationException($"This link already exists ({this}->{other})");

            _links.Add(other);
            _owningArea[other]._links.Add(this.Position);
        }

        // !! smellz?
        internal void LinkAllNeighborsInArea(Area area) {
            foreach (var neighbor in _neighbors) {
                if (_links.Contains(neighbor)) {
                    continue;
                }
                if (area.Cells.Any(c => (area.Position + c.Position) == neighbor)) {
                    Link(neighbor);
                }
            }
        }

        /// <summary>
        /// Breaks a link between the cells.
        /// </summary>
        /// <param name="other"></param>
        public void Unlink(Vector other) {
            _links.Remove(other);
            _owningArea[other]._links.Remove(this.Position);
        }

        /// <summary>
        /// The neighbors of this cell.
        /// </summary>
        // TODO: Readonly? With vectors, we can just pre-compute?
        public IList<Vector> Neighbors() => _neighbors;

        public bool HasNeighbor(Vector positionInArea) =>
            _neighbors.Contains(positionInArea);

        /// <summary>
        /// The links between this cell and other cells.
        /// </summary>
        // TODO: Change all return types to vectors.
        public IList<Vector> Links() =>
            _links.AsReadOnly();

        public bool HasLink(Vector positionInArea) =>
            _links.Contains(positionInArea);

        public bool HasLinks() => _links.Count > 0;

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
            HasLink(_position + Vector.North2D) ? "N" : "-",
            HasLink(_position + Vector.East2D) ? "E" : "-",
            HasLink(_position + Vector.South2D) ? "S" : "-",
            HasLink(_position + Vector.West2D) ? "W" : "-");

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