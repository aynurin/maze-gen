using System;
using System.Collections.Generic;

namespace PlayersWorlds.Maps.Areas {
    // ? The issue is that Halls can be entered, so they should act as cells,
    // ? and Fills can't be entered, so they are invisible to all algorithms. 
    // ? For Dijkstra, Halls should act as regular cells, but with increased a
    // ? passing complexity taking into account its cells.
    // Area size definition: Given that the mazes can be autogenerated and of
    // different sizes, we can't easily predefine areas counts and sizes, so we
    // might want to calculate areas based on some kind of heuristics. E.g., see
    // RandomZoneGenerator.
    public class MapArea {
        private Vector _position;

        public AreaType Type { get; private set; }
        public string[] Tags { get; private set; }
        public Vector Size { get; private set; }
        /// <summary>
        /// 
        /// </summary>
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
        public List<Cell> Cells { get; private set; }

        public double LowX => Position.X;
        public double HighX => Position.X + Size.X;
        public double LowY => Position.Y;
        public double HighY => Position.Y + Size.Y;

        public MapArea(AreaType type, Vector size, params string[] tags)
            : this(type, size, Vector.Empty, tags) { }

        public MapArea(AreaType type, Vector size, Vector position, params string[] tags) {
            Cells = new List<Cell>(size.Area);
            Type = type;
            Tags = tags;
            Size = size;
            _position = position;
        }

        public bool Overlaps(MapArea other) {
            if (this == other)
                throw new InvalidOperationException("Can't compare with self");
            var noOverlap = this.HighX <= other.LowX || this.LowX >= other.HighX;
            noOverlap |= this.HighY <= other.LowY || this.LowY >= other.HighY;
            return !noOverlap;
        }

        internal bool Fits(Vector position, Vector size) {
            // Check if the inner rectangle is completely within the outer rectangle.
            return this.LowX >= position.X &&
                this.HighX <= position.X + size.X &&
                this.LowY >= position.Y &&
                this.HighY <= position.Y + size.Y;
        }

        internal static MapArea Parse(string v) {
            var parts = v.Split(';');
            var type = (AreaType)Enum.Parse(typeof(AreaType), parts[2]);
            var size = VectorD.Parse(parts[1]).RoundToInt();
            var position = VectorD.Parse(parts[0]).RoundToInt();
            return new MapArea(type, size, position);
        }

        public override string ToString() {
            return $"P{Position};S{Size};{Type}";
        }
    }
}