using System;
using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Areas.Evolving;
using PlayersWorlds.Maps.MapFilters;
using PlayersWorlds.Maps.Maze;
using PlayersWorlds.Maps.Renderers;
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
        private readonly Area _parent;

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

        public Area Parent => _parent;

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

        public static Area CreateUnpositioned(Vector position,
                                              Vector size,
                                              AreaType areaType,
                                              params string[] tags) =>
            new Area(position,
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
        private Area(Vector position, bool isPositionFixed, Vector size,
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

            _cells = new NArray<Cell>(size, xy => new Cell(xy, this));
            foreach (var cell in _cells.Iterate()) {
                var north = cell.xy + Vector.North2D;
                if (north.Y < _cells.Size.Y) {
                    cell.cell.Neighbors().Add(north);
                    _cells[north].Neighbors().Add(cell.xy);
                }

                var west = cell.xy + Vector.West2D;
                if (west.X >= 0) {
                    cell.cell.Neighbors().Add(west);
                    _cells[west].Neighbors().Add(cell.xy);
                }
            }
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        private Area(Vector position, bool isPositionFixed, NArray<Cell> cells,
                    AreaType areaType, Area parent,
                    IEnumerable<Area> childAreas,
                    string[] tags) {
            _position = position;
            _areaType = areaType;
            _isPositionFixed = isPositionFixed;
            _parent = parent;
            _childAreas = new List<Area>(childAreas);
            _tags = new string[tags.Length];
            Array.Copy(tags, _tags, tags.Length);
            _cells = new NArray<Cell>(
                cells.Size, xy => cells[xy].CloneWithParent(this));
        }

        public IEnumerable<Cell> ChildCellsAt(Vector xy) {
            // yield return _cells[xy];
            foreach (var area in _childAreas) {
                if (!area.Contains(xy)) continue;
                var positionInArea = xy - area.Position;
                yield return area[positionInArea];
                foreach (var cell in area.ChildCellsAt(positionInArea)) {
                    yield return cell;
                }
            }
        }

        public Area ShallowCopy() => new Area(
            _position,
            _isPositionFixed,
            _cells,
            _areaType,
            _parent,
            _childAreas,
            _tags);

        public Area CreateChildArea(Area template) {
            var childArea = new Area(template._position,
                            template._isPositionFixed,
                            template._cells,
                            template._areaType,
                            this,
                            template._childAreas,
                            template._tags);
            _childAreas.Add(childArea);
            return childArea;
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
                            _areaType, _parent, childAreas, _tags);
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
            return new Area(position, isPositionFixed,
                            size, type,
                            /*childAreas=*/null,
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
        // TODO: Factor out or rename
        public string RenderToString() {
            return new Map2DStringRenderer().Render(this);
        }

        /// <summary>
        /// Returns a serialized representation of this maze.
        /// </summary>
        // !! BUG: Currently the serialized representation is not the same as 
        //  the one used by Parse.
        // TODO: Create a common serialization approach.
        public string Serialize() {
            var linksAdded = new HashSet<int[]>();
            var size = Size.ToString();
            var areas = string.Join(",", _childAreas.Select(area => area.ToString()));
            var cells = string.Join(",", _cells.Select((cell, index) => {
                if (_cells[index].HasLinks()) {
                    var links = _cells[index].Links()
                        .Select(link => link.ToIndex(Size))
                        .Where(link => !linksAdded.Contains(new int[] { index, link }));

                    linksAdded.UnionWith(links.Select(link => new int[] { link, index }));

                    return $"{index}:{string.Join(" ", links)}";
                } else {
                    return null;
                }
            }).Where(s => s != null));
            return $"{size}|{areas}|{cells}";
        }

        /// <summary>
        /// Parses a string into a <see cref="Area" />.
        /// </summary>
        /// <param name="serialized">A string of the form
        /// <c>Vector; cell:link,link,...; cell:link,link,...; ...</c>, where
        /// <c>Vector</c> is a string representation of a 2D
        /// <see cref="Vector" /> defining the size of the maze, <c>cell</c> is
        /// the index of the cell in the maze, and <c>link</c> is the index of 
        /// a cell linked to this cell.</param>
        /// <returns></returns>
        // TODO: Add Areas
        public static Area ParseAsMaze(string serialized) {
            if (serialized.IndexOf('|') == -1) {
                // TODO: Migrate all serialization to the other format.
                var parts = serialized.Split(';', '\n');
                var size = new Vector(parts[0].Split('x').Select(int.Parse));
                var maze = Area.CreateEnvironment(size);
                for (var i = 1; i < parts.Length; i++) {
                    var part = parts[i].Split(':', ',').Select(int.Parse).ToArray();
                    for (var j = 1; j < part.Length; j++) {
                        maze._cells[part[0]].Link(Vector.FromIndex(part[j], maze.Size));
                    }
                }
                return maze;
            } else {
                var linksAdded = new HashSet<string>();
                var parts = serialized.Split('|');
                var size = Vector.Parse(parts[0]);
                var maze = Area.CreateEnvironment(size);
                parts[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ForEach(
                    areaStr => maze.CreateChildArea(Area.Parse(areaStr)));
                parts[2].Split(
                    new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .ForEach(cellStr => {
                        var part = cellStr.Split(':', ' ')
                                          .Select(int.Parse).ToArray();
                        for (var j = 1; j < part.Length; j++) {
                            if (linksAdded.Contains($"{part[0]}|{part[j]}")) {
                                continue;
                            }
                            maze._cells[part[0]].Link(Vector.FromIndex(part[j], maze.Size));
                            linksAdded.Add($"{part[0]}|{part[j]}");
                            linksAdded.Add($"{part[j]}|{part[0]}");
                        }
                    });
                return maze;
            }
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