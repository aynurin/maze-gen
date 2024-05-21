using System;
using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Areas.Evolving;
using PlayersWorlds.Maps.Renderers;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// Represents an area of arbitrary cells in an N-space.
    /// </summary>
    public class Area : ExtensibleObject {
        private Vector _position;
        private readonly bool _isPositionFixed;
        private readonly NArray<Cell> _cells;
        private readonly AreaType _areaType;
        private readonly string[] _tags;
        private readonly List<Area> _childAreas = new List<Area>();

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
        }

        internal bool IsPositionFixed => _isPositionFixed;


        /// <summary>
        /// Used by other algorithms to identify if the area can be entered
        /// by the player or not.
        /// </summary>
        public AreaType Type => _areaType;

        /// <summary>
        /// Allows to specify any data significant for further use. These tags
        /// can be populated by the user and will be retained in the resulting
        /// map.
        /// </summary>
        public string[] Tags => _tags;

        public IReadOnlyCollection<Area> ChildAreas => _childAreas.AsReadOnly();

        /// <summary>
        /// Size of the map in cells.
        /// </summary>

        public Vector Size => _cells.Size;

        /// <summary>
        /// A readonly access to the map cells.
        /// </summary>
        public NArray<Cell> Cells => _cells;

        internal double LowX => _position.X;
        internal double HighX => _position.X + Size.X;
        internal double LowY => _position.Y;
        internal double HighY => _position.Y + Size.Y;

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

        public static Area Create(Vector position,
                                  Vector size,
                                  AreaType areaType,
                                  params string[] tags) =>
            new Area(position, true, areaType,
                     new NArray<Cell>(size, xy => new Cell(xy)), tags);

        public static Area CreateUnpositioned(Vector size,
                                              AreaType areaType,
                                              params string[] tags) =>
            new Area(Vector.Empty, false, areaType,
                     new NArray<Cell>(size, xy => new Cell(xy)), tags);

        public static Area CreateUnpositioned(Vector position,
                                              Vector size,
                                              AreaType areaType,
                                              params string[] tags) =>
            new Area(position, false, areaType,
                     new NArray<Cell>(size, xy => new Cell(xy)), tags);

        public static Area CreateEnvironment(Vector size,
                                             params string[] tags) =>
            CreateEnvironment(size, xy => new Cell(xy), tags);

        public static Area CreateEnvironment(Vector size,
                                             Func<Vector, Cell> initialValue,
                                             params string[] tags) =>
            new Area(Vector.Zero(size.Value.Count),
                     /*isPositionFixed=*/false,
                     AreaType.Environment,
                     new NArray<Cell>(size, initialValue),
                     tags);

        private Area(Vector position, bool isPositionFixed,
                    AreaType areaType, NArray<Cell> mapdata,
                    params string[] tags) {
            if (position.IsEmpty && isPositionFixed) {
                throw new ArgumentException("Position is not initialized.");
            }
            _position = position;
            _cells = mapdata;
            _areaType = areaType;
            _isPositionFixed = isPositionFixed;
            _tags = tags;
        }

        public void Reposition(Vector newPosition) {
            if (_isPositionFixed)
                throw new InvalidOperationException("Position is fixed");
            _position = newPosition;
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
        /// <returns><c>true</c> if the two Areas overlap; otherwise, <c>
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

        /// <summary>
        /// Scales current map to the specified size.
        /// </summary>
        /// <remarks>The size has to be a multiple of the current map size.
        /// </remarks>
        /// <param name="newSize">The size of the saled map.</param>
        /// <returns>A new instance of <see cref="Area" /></returns>
        public Area Scale(Vector newSize) {
            if (newSize.Value.Zip(Size.Value,
                    (a, b) => a % b != 0 || a < b).Any()) {
                throw new ArgumentException(
                    "The specified size must be a greater multiple of the " +
                    $"current map size ({Size}). Provided {newSize}",
                    nameof(newSize));
            }

            return new Area(_position, _isPositionFixed,
                            _areaType, _cells.ScaleUp(newSize));
        }

        public Area ShallowCopy() =>
            new Area(_position, _isPositionFixed,
                     _areaType, new NArray<Cell>(Cells));

        /// <summary>
        /// Parses a string representation of a Area, similar to one produced
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
        internal static Area Parse(string v, bool isPositionFixed = false) {
            var parts = v.Split(';');
            var type = parts.Length > 2 ?
                (AreaType)Enum.Parse(typeof(AreaType), parts[2]) :
                AreaType.None;
            var size = VectorD.Parse(parts[1]).RoundToInt();
            var position = VectorD.Parse(parts[0]).RoundToInt();
            return new Area(position,
                            isPositionFixed,
                            type,
                            new NArray<Cell>(size, xy => new Cell(xy)),
                            parts.Skip(3).ToArray());
        }

        /// <summary>
        /// Produces a string representation of this Area.
        /// </summary>
        /// <returns>A <see cref="string" /> of the form
        /// "P{Position};S{Size};{Type}".</returns>
        public override string ToString() =>
            $"P{(_position.IsEmpty ? "<empty>" : _position.ToString())};" +
            $"S{Size};{Type};" + string.Join(";", Tags);

        /// <summary>
        /// Renders the map to a string using a
        /// <see cref="Map2DStringRenderer" />.
        /// </summary>
        /// <returns>A string containing a rendered map.</returns>
        public string RenderToString() {
            return new Map2DStringRenderer().Render(this);
        }
    }
}