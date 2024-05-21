using System;
using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// Represents an area of arbitrary cells in an N-space.
    /// </summary>
    public class Area : ExtensibleObject {
        private readonly Vector _position;
        private readonly bool _isPositionFixed;

        private readonly NArray<Cell> _cells;

        private readonly AreaType _areaType;

        private readonly List<Area> _childAreas = new List<Area>();

        public AreaType Type => _areaType;

        public IReadOnlyCollection<Area> ChildAreas => _childAreas.AsReadOnly();

        public Vector Size => _cells.Size;

        public NArray<Cell> Cells => _cells;

        internal double LowX => _position.X;
        internal double HighX => _position.X + Size.X;
        internal double LowY => _position.Y;
        internal double HighY => _position.Y + Size.Y;

        public static Area Create(
            Vector position, Vector size, AreaType areaType) =>
            new Area(position, true, areaType,
                     new NArray<Cell>(size, xy => new Cell(xy)));

        public static Area CreateUnpositioned(
            Vector size, AreaType areaType) =>
            new Area(Vector.Empty, false, areaType,
                     new NArray<Cell>(size, xy => new Cell(xy)));

        public static Area CreateEnvironment(Vector size, Func<Vector, Cell> initialValue = null) =>
            new Area(Vector.Empty, false, AreaType.Environment,
                     new NArray<Cell>(size, initialValue));

        private Area(Vector position, bool isPositionFixed,
                    AreaType areaType, NArray<Cell> mapdata) {
            _position = position;
            _cells = mapdata;
            _areaType = areaType;
            _isPositionFixed = isPositionFixed;
        }

        internal void AddAreas(IEnumerable<Area> areas) {
            _childAreas.AddRange(areas);
            // // add the new areas to all cells owned by this area.
            // foreach (var area in areas) {
            //     foreach (var cell in area.Cells) {
            //         this.Cells[cell.Position].AddArea(area);
            //     }
            // }
        }

        /// <summary>
        /// Checks if this Area overlaps with another Area.
        /// </summary>
        /// <param name="other">The other Area to check</param>
        /// <returns><c>true</c> if the two MapAreas overlap; otherwise, <c>
        /// false</c>.</returns>
        public bool Overlaps(Area other) => OverlapArea(other) != Vector.Zero2D;

        /// <summary>
        /// Calculates the size of the overlap area between this Area and 
        /// another Area.
        /// </summary>
        /// <param name="other">The other Area to check</param>
        /// <returns><see cref="Vector" /> of the size of the overlap, or 
        /// <see cref="Vector.Zero2D" /> if there is no overlap.</returns>
        public Vector OverlapArea(Area other) {
            if (this == other)
                throw new InvalidOperationException("Can't compare with self");

            // Calculate the overlap rectangle coordinates
            var lowX = (int)Math.Max(this.LowX, other.LowX);
            var highX = (int)Math.Min(this.HighX, other.HighX);
            var lowY = (int)Math.Max(this.LowY, other.LowY);
            var highY = (int)Math.Min(this.HighY, other.HighY);

            // Check if there is no overlap
            if (lowX >= highX || lowY >= highY) {
                return Vector.Zero2D;
            }

            // Calculate the size of the overlap area
            return new Vector(highX - lowX, highY - lowY);
        }

        /// <summary>
        /// Checks if this Area contains or touches the
        /// <paramref name="point" />.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns><c>true</c> if the given point is within this area.
        /// </returns>
        public bool Contains(Vector point) {
            return LowX <= point.X && HighX >= point.X
                && LowY <= point.Y && HighY >= point.Y;
        }

        /// <summary>
        /// Checks if this Area is completely within an area defined by
        /// <paramref name="position" /> and <paramref name="size" />.
        /// </summary>
        /// <param name="position">The position of the outer area.</param>
        /// <param name="size">The size of the outer rectangle.</param>
        /// <returns><c>true</c> if the inner rectangle is completely within the
        /// outer area, <c>false</c> otherwise.</returns>
        public bool FitsInto(Vector position, Vector size) {
            return LowX >= position.X &&
                HighX <= position.X + size.X &&
                LowY >= position.Y &&
                HighY <= position.Y + size.Y;
        }

        /// <summary>
        /// Checks if this Area is completely within the 
        /// <paramref name="other" /> area.
        /// </summary>
        /// <param name="other">The outer area.</param>
        /// <returns><c>true</c> if the inner area is completely within the
        /// outer area, <c>false</c> otherwise.</returns>
        public bool FitsInto(Area other) {
            // TODO: How does this work with non-rectangle areas? 
            return LowX >= other.LowX &&
                HighX <= other.HighX &&
                LowY >= other.LowY &&
                HighY <= other.HighY;
        }

        public Area Scale(Vector vector) {
            if (vector.Value.Zip(Size.Value,
                    (a, b) => a % b != 0 || a < b).Any()) {
                throw new ArgumentException(
                    "The specified size must be a greater multiple of the " +
                    $"current map size ({Size}). Provided {vector}",
                    nameof(vector));
            }

            return new Area(_position, _isPositionFixed,
                            _areaType, _cells.ScaleUp(vector));
        }

        public Area ShallowCopy() =>
            new Area(_position, _isPositionFixed,
                     _areaType, new NArray<Cell>(Cells));
    }
}