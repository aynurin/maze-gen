using System;
using System.Collections;
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
    public class Area : ExtensibleObject, IReadOnlyCollection<Cell> {
        private readonly bool _isPositionFixed;
        private readonly List<Cell> _cells;
        private readonly Grid _grid;
        private readonly AreaType _areaType;
        private readonly string[] _tags;
        private readonly List<Area> _childAreas;

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
        public Grid Grid => _grid;

        public int Count => _cells.Count;

        public bool IsHollow =>
            _areaType == AreaType.Hall || _areaType == AreaType.Cave;

        /// <summary>
        /// Gets the value of a cell at the specified <see cref="Vector"/>
        /// position.
        /// </summary>
        /// <param name="xy">The position of the cell as a <see cref="Vector"/>.
        /// </param>
        /// <returns>The value of the cell at the specified position.</returns>
        /// <exception cref="IndexOutOfRangeException">The position is outside
        /// the map bounds.</exception>
        public Cell this[Vector xy] => _cells[(_grid.Position.IsEmpty ? xy : (xy - _grid.Position)).ToIndex(_grid.Size)];

        public static Area Create(Vector position,
                                  Vector size,
                                  AreaType areaType,
                                  params string[] tags) =>
            new Area(position, size,
                     /*isPositionFixed=*/true, areaType,
                     /*childAreas=*/null, tags);

        public static Area CreateUnpositioned(Vector size,
                                              AreaType areaType,
                                              params string[] tags) =>
            new Area(Vector.Empty, size,
                     /*isPositionFixed=*/false, areaType,
                     /*childAreas=*/null, tags);

        public static Area CreateUnpositioned(Vector initialPosition,
                                              Vector size,
                                              AreaType areaType,
                                              params string[] tags) =>
            new Area(initialPosition, size,
                     /*isPositionFixed=*/false, areaType,
                     /*childAreas=*/null, tags);

        public static Area CreateEnvironment(Vector size,
                                             params string[] tags) =>
            new Area(Vector.Zero(size.Value.Count), size,
                     /*isPositionFixed=*/false,
                     AreaType.Environment,
                     /*childAreas=*/null,
                     tags);

        /// <summary>
        /// Creates a new instance of Area.
        /// </summary>
        /// <param name="position">Position of this area.</param>
        /// <param name="size">Size of the area.</param>
        /// <param name="isPositionFixed"><c>true</c> if the position is fixed,
        /// <c>false</c> otherwise.</param>
        /// <param name="areaType">Area type.</param>
        /// <param name="childAreas">Child areas.</param>
        /// <param name="tags">Tags to assign to this area.</param>
        /// <exception cref="ArgumentException"></exception>
        // TODO: Area constructors seem to be redundant.
        internal Area(Vector position, Vector size, bool isPositionFixed,
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
            _grid = new Grid(position, size);
            _cells = Enumerable.Range(0, size.Area)
                               .Select(_ => new Cell(areaType))
                .ToList();
            BakeChildAreas();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        internal Area(Grid grid, IEnumerable<Cell> cells, bool isPositionFixed,
                    AreaType areaType,
                    IEnumerable<Area> childAreas,
                    string[] tags) {
            if (grid.Position.IsEmpty && isPositionFixed) {
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
            _grid = new Grid(grid.Position, grid.Size);
            _cells = new List<Cell>(cells);
            BakeChildAreas();
        }

        public void BakeChildAreas() {
            // check if the cell belongs to a Hall or Cave.
            // check if two cells belong to the same Hall or Cave area.
            // make sure the cell is available; unavailable cells cannot be
            //      visited.
            var childAreas = _childAreas.Where(a => !a.IsPositionEmpty)
                                        .ToList();

            // Add all None-Areas to unavailable cells;
            // Remove all non-None-Areas from unavailable cells.
            // Force all Fill areas to be unavailable.
            var unavailableCells = new HashSet<Vector>(
                _areaType == AreaType.None ? _grid : Enumerable.Empty<Vector>());
            // the reason we do this in three steps is because areas can be
            // overlapping. I.e. if there is a hall area over none area, and
            // then covered by a fill area, the fill area cells have to be
            // marked unavailable no matter the order in the collection.
            foreach (var area in childAreas.Where(a => a.Type == AreaType.None)) {
                foreach (var cell in area._grid) {
                    unavailableCells.Add(cell);
                }
            }
            foreach (var area in childAreas.Where(a => a.Type != AreaType.None)) {
                unavailableCells.ExceptWith(area._grid.Select(cell => cell));
            }
            foreach (var area in childAreas.Where(a => a.Type == AreaType.Fill)) {
                foreach (var cell in area._grid) {
                    unavailableCells.Add(cell);
                }
            }

            foreach (var cell in _grid) {
                if (unavailableCells.Contains(cell)) continue;
                var relatedAreas = childAreas
                    .Where(a => a.Contains(cell)).ToList();
                var envNeighbors = _grid.AdjacentRegion(cell)
                    // don't include diagonal neighbors.
                    .Where(nbr => nbr.Value.Where(
                        (x, i) => cell.Value[i] == x).Any())
                    // don't include unavailable neighbors.
                    .Where(nbr => !unavailableCells.Contains(nbr))
                    .ToList();
                var envLinks = envNeighbors
                    // both cell and neighbor belong to the same hollow area.
                    .Where(nbr => relatedAreas.Any(a => a.IsHollow && a.Contains(nbr)));
                var relatedCells = relatedAreas
                    .Select(a => a[cell]);
                this[cell].Bake(relatedCells, envNeighbors, envLinks);
            }
        }

        public void Reposition(Vector newPosition) {
            if (_isPositionFixed)
                throw new InvalidOperationException("Position is fixed");
            _grid.Reposition(newPosition);
        }

        public IEnumerable<Area> ChildAreas() =>
            _childAreas.AsReadOnly();

        public IEnumerable<Area> ChildAreas(Vector xy) =>
            _childAreas.Where(a => a.Contains(xy));

        public bool CellsAreLinked(Vector one, Vector another) {
            return this[one].HardLinks.Contains(another) ||
                   this[one].BakedLinks.Contains(another);
        }

        public bool CellHasLinks(Vector cell) {
            return this[cell].HardLinks.Count > 0 ||
                   this[cell].BakedLinks.Count > 0;
        }

        public ICollection<Vector> CellLinks(Vector cell) {
            return this[cell].HardLinks
                    .Concat(this[cell].BakedLinks)
                    .Distinct().ToList();
        }

        public Area ShallowCopy() => new Area(
            _grid,
            _cells,
            _isPositionFixed,
            _areaType,
            _childAreas,
            _tags);

        public void AddChildArea(Area area) {
            _childAreas.Add(area);
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
            this[cell].BakedNeighbors;

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

        public IEnumerator<Cell> GetEnumerator() {
            return _cells.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _cells.GetEnumerator();
        }
    }
}