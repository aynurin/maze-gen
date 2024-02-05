using System;
using System.Linq;
using PlayersWorlds.Maps.Areas.Evolving;

namespace PlayersWorlds.Maps.Areas {
    /// <summary>
    /// An area is anything that spans one or more cells and apply additional
    /// properties to the cells it spans. E.g., it can be a differently styled
    /// set of regular cells, or a hall, lake, void, etc. Some areas can be
    /// entered, others cannot. So areas impact not only a visual style, but
    /// also generating algorithms and play-time events. For example, a hall can
    /// be entered, a lake cannot, a desert can have a different style, etc.
    /// </summary>
    /// <remarks>
    /// <li>Use one of the
    /// <see cref="CreateAutoPositioned(AreaType, Vector, string[])" /> and
    /// overloads to create an area that will be positioned automatically by
    /// <see cref="AreaDistributor" />.</li> 
    /// <li>Use <see cref="Create" /> to create an area at a given position.
    /// </li>
    /// </remarks>
    public class MapArea {
        private Vector _position;
        /// <summary>
        /// Used by other algorithms to identify if the area can be entered
        /// by the player or not.
        /// </summary>
        public AreaType Type { get; private set; }
        /// <summary>
        /// Allows to specify any data significant for further use. These tags
        /// can be populated by the user and will be retained in the resulting
        /// map.
        /// </summary>
        public string[] Tags { get; private set; }
        /// <summary>
        /// Size of this area in the target map.
        /// </summary>
        /// <remarks>
        /// If the area is applied to a maze, then it's the cells of the maze;
        /// and if the area is applied to a map, then it's the cells of the map.
        /// </remarks>
        public Vector Size { get; private set; }
        /// <summary>
        /// Position of this area in the target map.
        /// </summary>
        /// <remarks>
        /// If the area is applied to a maze, then it's the cells of the maze;
        /// and if the area is applied to a map, then it's the cells of the map.
        /// </remarks>
        /// <remarks>
        /// There should be a clear separation in code between
        /// positioned areas and non-positioned areas, so we will
        /// throw here without letting the consumer to check if
        /// the area is positioned or not.
        /// </remarks>
        public Vector Position {
            get {
                if (_position.IsEmpty) {
                    throw new InvalidOperationException(
                        "Position is not initialized. " +
                        "Check if IsPositionFixed == true.");
                }
                return _position;
            }
            set => _position = value;
        }
        internal bool IsPositionFixed { get; }

        internal double LowX => Position.X;
        internal double HighX => Position.X + Size.X;
        internal double LowY => Position.Y;
        internal double HighY => Position.Y + Size.Y;

        private MapArea(
            AreaType type,
            Vector position,
            Vector size,
            bool isPositionFixed,
            params string[] tags) {
            Type = type;
            Position = position;
            Size = size;
            IsPositionFixed = isPositionFixed;
            Tags = tags;
        }

        /// <summary>
        /// Creates an unpositioned <see cref="MapArea"/> instance. This
        /// area will be positioned by <see cref="AreaDistributor" /> before
        /// being placed to a map.
        /// </summary>
        /// <param name="type">The type of area.</param>
        /// <param name="size">The size of the area.</param>
        /// <param name="tags">The tags for the area.</param>
        public static MapArea CreateAutoPositioned(
            AreaType type, Vector size, params string[] tags) {
            return new MapArea(type, Vector.Empty, size, false, tags);
        }

        /// <summary>
        /// Creates a <see cref="MapArea"/> instance at a given position. This
        /// area can be re-positioned by <see cref="AreaDistributor" /> before
        /// being placed to a map.
        /// </summary>
        /// <param name="type">The type of area.</param>
        /// <param name="size">The size of the area.</param>
        /// <param name="position">The initial position of the area.</param>
        /// <param name="tags">The tags for the area.</param>
        public static MapArea CreateAutoPositioned(
            AreaType type, Vector size, Vector position, params string[] tags) {
            return new MapArea(type, position, size, false, tags);
        }

        /// <summary>
        /// Creates <see cref="MapArea"/> at a given position.
        /// <see cref="AreaDistributor" /> will not move this area.
        /// </summary>
        /// <param name="type">The type of area.</param>
        /// <param name="size">The size of the area.</param>
        /// <param name="position">The position of the area.</param>
        /// <param name="tags">The tags for the area.</param>
        public static MapArea Create(
            AreaType type, Vector position, Vector size, params string[] tags) {
            if (position.IsEmpty) {
                throw new ArgumentException("Position cannot be empty.");
            }
            return new MapArea(type, position, size, true, tags);
        }

        /// <summary>
        /// Checks if this MapArea overlaps with another MapArea.
        /// </summary>
        /// <param name="other">The other MapArea to check</param>
        /// <returns><c>true</c> if the two MapAreas overlap; otherwise, <c>
        /// false</c>.</returns>
        public bool Overlaps(MapArea other) {
            if (this == other)
                throw new InvalidOperationException("Can't compare with self");
            var noOverlap = HighX <= other.LowX || LowX >= other.HighX;
            noOverlap |= HighY <= other.LowY || LowY >= other.HighY;
            return !noOverlap;
        }

        /// <summary>
        /// Checks if this MapArea is completely within an area defined by
        /// <paramref name="position" /> and <paramref name="size" />.
        /// </summary>
        /// <param name="position">The position of the outer area.</param>
        /// <param name="size">The size of the outer rectangle.</param>
        /// <returns><c>true</c> if the inner rectangle is completely within the
        /// outer area, <c>false</c> otherwise.</returns>
        internal bool FitsInto(Vector position, Vector size) {
            return LowX >= position.X &&
                HighX <= position.X + size.X &&
                LowY >= position.Y &&
                HighY <= position.Y + size.Y;
        }

        /// <summary>
        /// A shortcut to <see cref="object.MemberwiseClone()" />.
        /// </summary>
        /// <returns>A shallow copy of the current <see cref="MapArea" />.
        /// </returns>
        public MapArea ShallowCopy() => this.MemberwiseClone() as MapArea;

        /// <summary>
        /// Parses a string representation of a MapArea, similar to one produced
        /// by the <see cref="ToString"/> method.
        /// </summary>
        /// <remarks>
        /// Note the rounding that occurs in <see cref="VectorD.ToString()" />.
        /// </remarks>
        /// <param name="v">A <see cref="string" /> of the form
        /// "P{Position};S{Size};{Type}[;tags]".</param>
        /// <param name="isPositionFixed"><c>true</c> to indicate that this area
        /// shouldn't be repositioned by <see cref="AreaDistributor" />,
        /// otherwise <c>false</c> (default)</param>
        /// <returns></returns>
        internal static MapArea Parse(string v, bool isPositionFixed = false) {
            var parts = v.Split(';');
            var type = parts.Length > 2 ?
                (AreaType)Enum.Parse(typeof(AreaType), parts[2]) :
                AreaType.None;
            var size = VectorD.Parse(parts[1]).RoundToInt();
            var position = VectorD.Parse(parts[0]).RoundToInt();
            return new MapArea(
                type, size, position, isPositionFixed, parts.Skip(3).ToArray());
        }

        /// <summary>
        /// Produces a string representation of this MapArea.
        /// </summary>
        /// <returns>A <see cref="string" /> of the form
        /// "P{Position};S{Size};{Type}".</returns>
        public override string ToString() {
            return $"P{Position};S{Size};{Type};" + string.Join(";", Tags);
        }
    }
}