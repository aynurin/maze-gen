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
        private readonly bool _isPositionFixed;
        private readonly Grid<Cell> _grid;
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
                if (_grid.Position.IsEmpty) {
                    throw new InvalidOperationException(
                        "Position is not initialized. " +
                        "Check if IsPositionFixed == true.");
                }
                return _grid.Position;
            }
        }

        internal bool IsPositionFixed => _isPositionFixed;
        internal bool IsPositionEmpty => _grid.Position.IsEmpty;


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
        public Vector Size => _grid.Size;

        /// <summary>
        /// A readonly access to the map cells.
        /// </summary>
        public Grid<Cell> Cells => _grid;

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
            get => _grid[xy];
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
            _areaType = areaType;
            _isPositionFixed = isPositionFixed;
            _childAreas = childAreas == null ?
                new List<Area>() :
                new List<Area>(childAreas);
            _tags = tags?.ToArray() ?? new string[0];
            _grid = new Grid<Cell>(position, size, xy => new Cell());
            RebuildChildAreasSnapshot();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        internal Area(Grid<Cell> cells, bool isPositionFixed,
                    AreaType areaType,
                    IEnumerable<Area> childAreas,
                    string[] tags) {
            if (cells.Position.IsEmpty && isPositionFixed) {
                throw new ArgumentException("Position is not initialized.");
            }
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
            _grid = new Grid<Cell>(cells.Position,
                cells.Size, xy => cells[xy].Clone());
            RebuildChildAreasSnapshot();
        }

        public IEnumerable<Vector> ChildAreaCells(Area area) {
            foreach (var cell in area.Cells) {
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
                   (_grid[one].HardLinks.Contains(another) ||
                   _hallCaveAreasCells
                        .Any(a => a.Contains(one) && a.Contains(another)));
        }

        public bool CellHasLinks(Vector cell) {
            return Contains(cell) &&
                   !_fillAreasCells.Contains(cell) &&
                   (_grid[cell].HardLinks.Count > 0 ||
                   _hallCaveAreasCells
                        .Any(a => a.Contains(cell)));
        }

        public IList<Vector> CellLinks(Vector cell) {
            return _grid[cell].HardLinks
                .Concat(NeighborsOf(cell)
                           .Where(n =>
                               !_fillAreasCells.Contains(n) &&
                               _hallCaveAreasCells.Any(
                             area => area.Contains(n) && area.Contains(cell))))
                  .Distinct().ToList();
        }

        public Area ShallowCopy() => new Area(
            _grid,
            _isPositionFixed,
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
                foreach (var cell in area._grid) {
                    filledAreasSnapshot.Add(cell + area.Position);
                }
            }
            foreach (var area in _childAreas.Where(a => a.Type == AreaType.Hall || a.Type == AreaType.Cave)) {
                if (area.IsPositionEmpty) continue;
                var areaCells = new HashSet<Vector>(
                    area._grid.Select(cell => cell + area.Position));
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
            _grid.Reposition(newPosition);
        }

        /// <summary>
        /// Checks if this Area overlaps with another Area.
        /// </summary>
        /// <param name="other">The other Area to check</param>
        /// <returns><c>true</c> if the two Areas overlap; otherwise, <c>
        /// false</c>.</returns>
        public bool Overlaps(Area other) => _grid.Overlaps(other._grid);

        /// <summary>
        /// Checks if this Area contains or touches the
        /// <paramref name="point" />.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns><c>true</c> if the given point is within this area.
        /// </returns>
        public bool Contains(Vector point) => _grid.Contains(point);

        /// <summary>
        /// Checks if this Area is completely within the 
        /// <paramref name="other" /> area.
        /// </summary>
        /// <param name="other">The outer area.</param>
        /// <returns><c>true</c> if the inner area is completely within the
        /// outer area, <c>false</c> otherwise.</returns>
        public bool FitsInto(Area other) =>
            _grid.FitsInto(other._grid.Position, other._grid.Size);

        public IEnumerable<Vector> NeighborsOf(Vector cell) =>
            _grid.AdjacentRegion(cell)
                 .Where(p => p.Value.Where((x, i) => cell.Value[i] == x).Any())
                 .Where(p => !_fillAreasCells.Contains(p));

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
        /// Scales current map to the specified size.
        /// </summary>
        /// <remarks>The size has to be a multiple of the current map size.
        /// </remarks>
        /// <param name="newSize">The size of the saled map.</param>
        /// <returns>A new instance of <see cref="Area" /></returns>
        public Area Scale(Vector newSize) {
            throw new NotImplementedException();
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