using System;
using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.MapFilters;
using PlayersWorlds.Maps.Maze;
using PlayersWorlds.Maps.Renderers;
using PlayersWorlds.Maps.Serializer;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

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
        private readonly List<Area> _childAreas;
        private List<HashSet<Vector>> _hallCaveAreasCells =
            new List<HashSet<Vector>>();
        private HashSet<Vector> _fillAreasCells = new HashSet<Vector>();

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
        internal bool IsPositionEmpty => _position.IsEmpty;


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
            new Area(position,
                     /*isPositionFixed=*/true, size, areaType,
                     /*childAreas=*/null, tags);

        public static Area CreateUnpositioned(Vector size,
                                              AreaType areaType,
                                              params string[] tags) =>
            new Area(Vector.Empty,
                     /*isPositionFixed=*/false, size, areaType,
                     /*childAreas=*/null, tags);

        public static Area CreateUnpositioned(Vector initialPosition,
                                              Vector size,
                                              AreaType areaType,
                                              params string[] tags) =>
            new Area(initialPosition,
                     /*isPositionFixed=*/false, size, areaType,
                     /*childAreas=*/null, tags);

        public static Area CreateEnvironment(Vector size,
                                             params string[] tags) =>
            new Area(Vector.Zero(size.Value.Count),
                     /*isPositionFixed=*/false,
                     size, AreaType.Environment,
                     /*childAreas=*/null,
                     tags);

        /// <summary>
        /// Creates a new instance of Area.
        /// </summary>
        /// <param name="position">Position of this area.</param>
        /// <param name="isPositionFixed"><c>true</c> if the position is fixed,
        /// <c>false</c> otherwise.</param>
        /// <param name="size">Size of the area.</param>
        /// <param name="areaType">Area type.</param>
        /// <param name="childAreas">Child areas.</param>
        /// <param name="tags">Tags to assign to this area.</param>
        /// <exception cref="ArgumentException"></exception>
        // TODO: Area constructors seem to be redundant.
        internal Area(Vector position, bool isPositionFixed, Vector size,
                    AreaType areaType,
                    IEnumerable<Area> childAreas,
                    IEnumerable<string> tags) {
            if (position.IsEmpty && isPositionFixed) {
                throw new ArgumentException("Position is not initialized.");
            }
            position.ThrowIfNull(nameof(position));
            _position = position;
            _areaType = areaType;
            _isPositionFixed = isPositionFixed;
            _childAreas = childAreas == null ?
                new List<Area>() :
                new List<Area>(childAreas);
            _tags = tags?.ToArray() ?? new string[0];
            _cells = new NArray<Cell>(size, xy => new Cell());
            RebuildChildAreasSnapshot();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        internal Area(Vector position, bool isPositionFixed, NArray<Cell> cells,
                    AreaType areaType,
                    IEnumerable<Area> childAreas,
                    string[] tags) {
            if (position.IsEmpty && isPositionFixed) {
                throw new ArgumentException("Position is not initialized.");
            }
            _position = position;
            _areaType = areaType;
            _isPositionFixed = isPositionFixed;
            _childAreas = childAreas == null ?
                new List<Area>() :
                new List<Area>(childAreas);
            if (tags == null) {
                _tags = new string[0];
            } else {
                _tags = new string[tags.Length];
                Array.Copy(tags, _tags, tags.Length);
            }
            _cells = new NArray<Cell>(
                cells.Size, xy => cells[xy].Clone());
            RebuildChildAreasSnapshot();
        }

        public IEnumerable<Vector> ChildAreaCells(Area area) {
            foreach (var cell in area.Cells.Positions) {
                yield return area.Position + cell;
            }
        }

        public IEnumerable<Area> ChildAreas() =>
            _childAreas.AsReadOnly();

        public IEnumerable<Area> ChildAreas(Vector xy) =>
            _childAreas.Where(a => a.Contains(xy));

        public bool CellsAreLinked(Vector one, Vector another) {
            return Contains(one) && Contains(another) &&
                   !_fillAreasCells.Contains(one) &&
                   !_fillAreasCells.Contains(another) &&
                   (_cells[one].HardLinks.Contains(another) ||
                   _hallCaveAreasCells
                        .Any(a => a.Contains(one) && a.Contains(another)));
        }

        public bool CellHasLinks(Vector cell) {
            return Contains(cell) &&
                   !_fillAreasCells.Contains(cell) &&
                   (_cells[cell].HardLinks.Count > 0 ||
                   _hallCaveAreasCells
                        .Any(a => a.Contains(cell)));
        }

        public IList<Vector> CellLinks(Vector cell) {
            return _cells[cell].HardLinks
                .Concat(NeighborsOf(cell)
                           .Where(n =>
                               !_fillAreasCells.Contains(n) &&
                               _hallCaveAreasCells.Any(
                             area => area.Contains(n) && area.Contains(cell))))
                  .Distinct().ToList();
        }

        public Area ShallowCopy() => new Area(
            _position,
            _isPositionFixed,
            _cells,
            _areaType,
            _childAreas,
            _tags);

        public void AddChildArea(Area area) {
            _childAreas.Add(area);
            RebuildChildAreasSnapshot();
        }

        public void RebuildChildAreasSnapshot() {
            if (_childAreas.Count == 0) return;
            // check if the cell belongs to a Hall or Cave.
            // check if two cells belong to the same Hall or Cave area.
            // make sure the cell does not belong to a filled area.
            var interconnectedAreasSnapshot = new List<HashSet<Vector>>();
            var filledAreasSnapshot = new HashSet<Vector>();
            foreach (var area in _childAreas.Where(a => a.Type == AreaType.Fill)) {
                if (area.IsPositionEmpty) continue;
                foreach (var cell in area._cells.Positions) {
                    filledAreasSnapshot.Add(cell + area.Position);
                }
            }
            foreach (var area in _childAreas.Where(a => a.Type == AreaType.Hall || a.Type == AreaType.Cave)) {
                if (area.IsPositionEmpty) continue;
                var areaCells = new HashSet<Vector>(
                    area._cells.Positions.Select(cell => cell + area.Position));
                areaCells.ExceptWith(filledAreasSnapshot);
                if (areaCells.Count > 0) {
                    interconnectedAreasSnapshot.Add(areaCells);
                }
            }
            _hallCaveAreasCells = interconnectedAreasSnapshot;
            _fillAreasCells = filledAreasSnapshot;
        }

        public void Reposition(Vector newPosition) {
            if (_isPositionFixed)
                throw new InvalidOperationException("Position is fixed");
            _position = newPosition;
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
            return LowX <= point.X && HighX > point.X
                && LowY <= point.Y && HighY > point.Y;
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

        public IEnumerable<Vector> NeighborsOf(Vector cell) => new[] {
                cell + Vector.South2D,
                cell + Vector.East2D,
                cell + Vector.North2D,
                cell + Vector.West2D
            }.Where(p => Contains(p) && !_fillAreasCells.Contains(p));

        public bool AreNeighbors(Vector one, Vector another) {
            return NeighborsOf(one).Contains(another);
        }

        public void Link(Vector one, Vector another) {
            // check if the cell is a neighbor.
            if (!NeighborsOf(one).Contains(another)) {
                throw new InvalidOperationException(
                    "Linking with non-adjacent cells is not supported yet (" +
                    $"Trying to link {one} to {another}). Neighbors: {string.Join(", ", NeighborsOf(one))}");
            }
            // this should never happen.
            if (!NeighborsOf(another).Contains(one)) {
                throw new InvalidProgramException(
                    "Non-mirroring neighborhood (" +
                    $"Trying to link {one} to {another}). Neighbors of one: {string.Join(", ", NeighborsOf(one))}, neighbors of another: {string.Join(", ", NeighborsOf(another))}");
            }
            this[one].HardLinks.Add(another);
            this[another].HardLinks.Add(one);
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
            // TODO: No coverage
            if (newSize.Value.Zip(Size.Value,
                    (a, b) => a % b != 0 || a < b).Any()) {
                throw new ArgumentException(
                    "The specified size must be a greater multiple of the " +
                    $"current map size ({Size}). Provided {newSize}",
                    nameof(newSize));
            }

            var scaleFactor = newSize.Value.Zip(Size.Value,
                    (a, b) => a / b).ToArray();
            var childAreas = _childAreas.Select(
                childArea => childArea.Scale(
                    new Vector(childArea.Size.Value.Zip(
                        scaleFactor, (a, b) => a * b))));
            var position =
                new Vector(_position.Value.Zip(scaleFactor, (a, b) => a * b));
            var cells = _cells.ScaleUp(newSize);

            return new Area(position, _isPositionFixed, cells,
                            _areaType, childAreas, _tags);
        }

        /// <summary>
        /// Renders this maze to a <see cref="Area" /> with the given options.
        /// </summary>
        /// <param name="options"><see cref="MazeToMapOptions" /></param>
        /// <returns></returns>
        // TODO: Factor out
        public Area ToMap(MazeToMapOptions options) {
            options.ThrowIfWrong(this.Size);
            var map = Maze2DRenderer.CreateMapForMaze(this, options);
            new Maze2DRenderer(this, options)
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail }, Cell.CellTag.MazeWall, options.WallWidths.Min(), options.WallHeights.Min()))
                .With(new Map2DSmoothCorners(Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner, options.WallWidths.Min(), options.WallHeights.Min()))
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner }, Cell.CellTag.MazeWall, options.WallWidths.Min(), options.WallHeights.Min()))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeVoid }, true, Cell.CellTag.MazeWall, 5, 5))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeWall, Cell.CellTag.MazeWallCorner }, false, Cell.CellTag.MazeTrail, 3, 3))
                .Render(map);
            return map;
        }

        /// <summary>
        /// Produces a string representation of this Area.
        /// </summary>
        /// <returns>A <see cref="string" /> of the form
        /// "P{Position};S{Size};{Type}".</returns>
        public override string ToString() =>
            new AreaSerializer().Serialize(this);

        /// <summary>
        /// Renders the map to a string using a
        /// <see cref="Map2DStringRenderer" />.
        /// </summary>
        /// <returns>A string containing a rendered map.</returns>
        // TODO: Factor out or rename
        public string RenderToString() {
            return new Map2DStringRenderer().Render(this);
        }

        /// <summary>
        /// Renders this maze to a string using
        /// <see cref="Maze2DStringBoxRenderer" />.
        /// </summary>
        public string MazeToString() {
            return new Maze2DStringBoxRenderer(this).WithTrail();
        }
    }
}