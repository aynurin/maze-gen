using System;
using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Serializer;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// An <see cref="Area"/> cell.
    /// </summary>
    /// <remarks>
    /// For now it's only a set of tags assigned to the cell.
    /// </remarks>
    public class Cell : ExtensibleObject {
        private readonly List<CellTag> _tags = new List<CellTag>();
        private readonly HashSet<Vector> _hardLinks = new HashSet<Vector>();
        private readonly AreaType _areaType;
        private AreaType? _bakedAreaType;
        private readonly HashSet<Vector> _bakedLinks = new HashSet<Vector>();
        private readonly HashSet<Vector> _bakedNeighbors = new HashSet<Vector>();
        private readonly HashSet<Cell> _bakedCells = new HashSet<Cell>();

        /// <summary>
        /// Tags assigned to cell.
        /// </summary>
        public List<CellTag> Tags => _tags;

        public HashSet<Vector> HardLinks => _hardLinks;

        public HashSet<Vector> BakedLinks => _bakedLinks;

        public HashSet<Vector> BakedNeighbors => _bakedNeighbors;

        public HashSet<Cell> BakedCells => _bakedCells;

        public AreaType AreaType => _bakedAreaType ?? _areaType;

        /// <summary>
        /// Creates an instance of cell at the specified position.
        /// </summary>
        /// <remarks>Supposed for internal use only.</remarks>
        internal Cell(AreaType areaType) {
            _areaType = areaType;
        }

        internal Cell Clone() {
            var newCell = new Cell(_areaType);
            newCell._tags.AddRange(_tags);
            foreach (var link in _hardLinks) {
                newCell._hardLinks.Add(link);
            }
            return newCell;
        }

        internal void Bake(IEnumerable<Cell> relatedCells,
                           IEnumerable<Vector> envNeighbors,
                           IEnumerable<Vector> envLinks) {
            // bake in neighbors, links, and other computed properties.
            _bakedLinks.Clear();
            _bakedLinks.UnionWith(envLinks);
            _bakedNeighbors.Clear();
            _bakedNeighbors.UnionWith(envNeighbors);
            _bakedCells.Clear();
            _bakedCells.UnionWith(relatedCells);

            var bakedType = relatedCells.Select(c => c.AreaType)
                                        .OrderByDescending(t => t)
                                        .FirstOrDefault();
            if (bakedType > _areaType) {
                _bakedAreaType = bakedType;
            }
        }

        override public string ToString() =>
            new CellSerializer().Serialize(this);

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
                return _tag.Equals((obj as CellTag)?._tag);
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
                return _tag;
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
