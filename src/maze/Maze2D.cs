using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.MapFilters;
using PlayersWorlds.Maps.Maze.PostProcessing;
using PlayersWorlds.Maps.Renderers;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// A 2D maze map that can be used by <see cref="MazeGenerator"/> to
    /// generate mazes.
    /// </summary>
    /// <remarks>
    /// <p><see cref="AllCells" /> are all the cells in the field. Each cell has
    /// neighbors on four of its sides. When the maze is generated, cells are 
    /// linked to each other to constitute passages.</p>
    /// <p>When choosing the next cell to move to, a maze generator picks 
    /// cells from the ones that haven't been linked yet.</p>
    /// <p>When the linkage happens, all cells belonging to <see
    /// cref="AreaType.Hall" /> <see cref="Areas" /> assigned to this cell are
    /// considered linked because the whole hall is now linked to some external
    /// cell. Which means that the maze generator cannot pick the cells of that
    /// hall in the further iterations. Which is not a valid behavior.</p>
    /// <p>To resolve this, we will use the following approach. There is a pool
    /// of cells to pick from when picking a random cell in the field. And
    /// another approach to pick a random neighbor.</p>
    /// </remarks>
    public class Maze2D {
        private readonly Vector _size;
        private readonly NArray<MazeCell> _cells;
        private readonly List<MazeCell> _unlinkedCells;
        /// <summary>
        /// Post-processing attributes assigned to this maze. See
        /// <see cref="PostProcessing"/>.
        /// </summary>
        public Dictionary<string, List<MazeCell>> Attributes { get; } =
            new Dictionary<string, List<MazeCell>>();
        /// <summary>
        /// A read-only access to the cells in this maze.
        /// </summary>
        public NArray<MazeCell> AllCells => _cells;
        /// <summary>
        /// A read-only access to the visitable cells in this maze.
        /// </summary>
        public IList<MazeCell> UnlinkedCells => _unlinkedCells.AsReadOnly();
        /// <summary />
        public IEnumerable<MazeCell> VisitedCells =>
            _cells.Where(cell => cell.IsVisited);

        /// <summary />
        public Vector Size { get => _cells.Size; }

        /// <summary>
        /// Number of cells in this maze.
        /// </summary>
        public int Area { get => _size.Area; }

        /// <summary>
        /// If the longest path was set, returns the longest path. See
        /// <see cref="PostProcessing.DijkstraDistance"/>.
        /// </summary>
        public Optional<List<MazeCell>> LongestPath {
            get =>
                Attributes.ContainsKey(DijkstraDistance.LongestTrailAttribute) ?
                Attributes[DijkstraDistance.LongestTrailAttribute] :
                    Optional<List<MazeCell>>.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">columns</param>
        /// <param name="y">rows</param>
        public Maze2D(int x, int y) : this(new Vector(x, y)) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">Map2D size,
        ///     where X is the width, number of columns,
        ///     and Y is the height, number of rows</param>
        public Maze2D(Vector size) {
            size.ThrowIfNotAValidSize();
            if (size.Value.Length != 2) {
                throw new NotImplementedException(
                    "At the moment, only 2D mazes are supported.");
            }
            _size = size;
            _cells = new NArray<MazeCell>(_size, xy => new MazeCell(xy));
            foreach (var cell in _cells.Iterate()) {
                var north = cell.xy + Vector.North2D;
                if (north.Y < size.Y) {
                    cell.cell.Neighbors().Add(_cells[north]);
                    _cells[north].Neighbors().Add(cell.cell);
                }

                var west = cell.xy + Vector.West2D;
                if (west.X >= 0) {
                    cell.cell.Neighbors().Add(_cells[west]);
                    _cells[west].Neighbors().Add(cell.cell);
                }
            }
            _unlinkedCells = new List<MazeCell>(_cells);
        }

        private readonly Dictionary<MapArea, ICollection<MazeCell>>
            _mapAreas = new Dictionary<MapArea, ICollection<MazeCell>>();

        /// <summary>
        /// Areas assigned to this maze. See <see cref="Maps.Areas"/>.
        /// </summary>
        public Dictionary<MapArea, ICollection<MazeCell>> MapAreas => _mapAreas;

        internal void AddArea(MapArea area) {
            var areaCells = _cells.Iterate(area.Position, area.Size)
                .Select(cell => cell.cell)
                .ToList();
            _mapAreas.Add(area, areaCells);
            foreach (var cell in areaCells) {
                cell.AddMapArea(area, areaCells);
            }
        }

        /// <summary>
        /// Links all cells in <see cref="AreaType.Hall" /> and
        /// <see cref="AreaType.Cave" /> areas, and removes
        /// <see cref="AreaType.Fill" /> area cells from the neighbors and
        /// links. This should be called in MazeGenerator.Generate() after the
        /// generator algorithm completes.
        /// </summary>
        public void ApplyAreas() {
            foreach (var area in _mapAreas) {
                if (area.Key.Type == AreaType.Fill) {
                    // if it's a filled area it cannot be visited, so we remove
                    // all mentions of its cells:
                    // 1. remove all of its cells from their neighbors
                    // 2. remove all neighbors of its cells
                    // 3. remove all links that involve its cells
                    foreach (var cell in area.Value) {
                        cell.Neighbors().ForEach(
                            neighbor => neighbor.Neighbors().Remove(cell));
                        cell.Neighbors().Clear();
                        var links = cell.Links().ToArray();
                        links.ForEach(link => cell.Unlink(link));
                        _unlinkedCells.Remove(cell);
                    }
                } else if (area.Key.Type == AreaType.Hall || area.Key.Type == AreaType.Cave) {
                    // if it's a hall, we need to link all its cells together
                    area.Value.ForEach(cell => cell.LinkAllNeighborsInArea(area.Key));
                }
            }
        }

        /// <summary>
        /// Renders this maze to a <see cref="Map2D" /> with the given options.
        /// </summary>
        /// <param name="options"><see cref="MazeToMapOptions" /></param>
        /// <returns></returns>
        public Map2D ToMap(MazeToMapOptions options) {
            options.ThrowIfWrong(this.Size);
            var map = Maze2DRenderer.CreateMapForMaze(this, options);
            new Maze2DRenderer(this, options)
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail }, Cell.CellTag.MazeWall, 1, 1))
                .With(new Map2DSmoothCorners(Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner, 1, 1))
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner }, Cell.CellTag.MazeWall, 1, 1))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeVoid }, true, Cell.CellTag.MazeWall, 5, 5))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeWall, Cell.CellTag.MazeWallCorner }, false, Cell.CellTag.MazeTrail, 3, 3))
                .Render(map);
            return map;
        }

        /// <summary>
        /// Renders this maze to a string using
        /// <see cref="Maze2DStringBoxRenderer" />.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return new Maze2DStringBoxRenderer(this).WithTrail();
        }

        /// <summary>
        /// Parses a string into a <see cref="Maze2D" />.
        /// </summary>
        /// <param name="serialized">A string of the form
        /// <c>Vector; cell:link,link,...; cell:link,link,...; ...</c>, where
        /// <c>Vector</c> is a string representation of a 2D
        /// <see cref="Vector" /> defining the size of the maze, <c>cell</c> is
        /// the index of the cell in the maze, and <c>link</c> is the index of 
        /// a cell linked to this cell.</param>
        /// <returns></returns>
        public static Maze2D Parse(string serialized) {
            var parts = serialized.Split(';', '\n');
            var maze = new Maze2D(new Vector(parts[0].Split('x').Select(int.Parse)));
            for (var i = 1; i < parts.Length; i++) {
                var part = parts[i].Split(':', ',').Select(int.Parse).ToArray();
                if (part.Length > 1) {
                    maze._unlinkedCells.Remove(maze.AllCells[part[0]]);
                }
                for (var j = 1; j < part.Length; j++) {
                    maze.AllCells[part[0]].Link(maze.AllCells[part[j]]);
                    maze._unlinkedCells.Remove(maze.AllCells[part[0]]);
                }
            }
            return maze;
        }
    }
}