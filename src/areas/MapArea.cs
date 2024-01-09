using System;
using System.Collections.Generic;

namespace PlayersWorlds.Maps.Areas {
    /// <summary>
    /// An area is anything that spans one or more cells and apply additional
    /// properties to the cells it spans. E.g., it can be a differently styled
    /// set of regular cells, or a hall, lake, void, etc. Some areas can be
    /// entered, others cannot. So areas impact not only a visual style, but
    /// also generating algorithms and play-time events. For example, a hall can
    /// be entered, a lake cannot, a desert can have a different style, etc.
    /// </summary>
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
                        "Position is not initialized");
                }
                return _position;
            }
            set => _position = value;
        }
        /// <summary>
        /// All cells of this area. Each cell can have its own tags that will be
        /// propagated to the target map.
        /// </summary>
        public List<Cell> Cells { get; private set; }

        internal double LowX => Position.X;
        internal double HighX => Position.X + Size.X;
        internal double LowY => Position.Y;
        internal double HighY => Position.Y + Size.Y;

        /// <summary>
        /// Creates an unpositioned <see cref="MapArea"/> instance. This
        /// instance has to be positioned before being applied to a map.
        /// </summary>
        /// <param name="type">The type of area.</param>
        /// <param name="size">The size of the area.</param>
        /// <param name="tags">The tags for the area.</param>
        public MapArea(AreaType type, Vector size, params string[] tags)
            : this(type, size, Vector.Empty, tags) { }

        /// <summary>
        /// Creates a positioned <see cref="MapArea"/> instance.
        /// </summary>
        /// <param name="type">The type of area.</param>
        /// <param name="size">The size of the area.</param>
        /// <param name="position">The position of the area.</param>
        /// <param name="tags">The tags for the area.</param>
        public MapArea(AreaType type, Vector size, Vector position, params string[] tags) {
            Cells = new List<Cell>(size.Area);
            Type = type;
            Tags = tags;
            Size = size;
            _position = position;
        }

        /// <summary>
        /// Checks if this MapArea overlaps with another MapArea or touches
        /// another MapArea.
        /// </summary>
        /// <param name="other">The other MapArea to check</param>
        /// <returns><c>true</c> if the two MapAreas overlap; otherwise, <c>
        /// false</c>.</returns>
        public bool Overlaps(MapArea other) {
            if (this == other)
                throw new InvalidOperationException("Can't compare with self");
            var noOverlap = this.HighX <= other.LowX || this.LowX >= other.HighX;
            noOverlap |= this.HighY <= other.LowY || this.LowY >= other.HighY;
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
        internal bool Fits(Vector position, Vector size) {
            return this.LowX >= position.X &&
                this.HighX <= position.X + size.X &&
                this.LowY >= position.Y &&
                this.HighY <= position.Y + size.Y;
        }

        /// <summary>
        /// Parses a string representation of a MapArea, similar to one produced
        /// by the <see cref="ToString"/> method.
        /// </summary>
        /// <remarks>
        /// Note the rounding that occurs in <see cref="VectorD.ToString()" />.
        /// </remarks>
        /// <param name="v">A <see cref="string" /> of the form
        /// "P{Position};S{Size};{Type}".</param>
        /// <returns></returns>
        internal static MapArea Parse(string v) {
            var parts = v.Split(';');
            var type = (AreaType)Enum.Parse(typeof(AreaType), parts[2]);
            var size = VectorD.Parse(parts[1]).RoundToInt();
            var position = VectorD.Parse(parts[0]).RoundToInt();
            return new MapArea(type, size, position);
        }

        /// <summary>
        /// Produces a string representation of this MapArea.
        /// </summary>
        /// <returns>A <see cref="string" /> of the form
        /// "P{Position};S{Size};{Type}".</returns>
        public override string ToString() {
            return $"P{Position};S{Size};{Type}";
        }
    }
}