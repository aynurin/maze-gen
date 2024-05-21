using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    /// <p><see cref="Cells" /> are all the cells in the field. Each cell has
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
    public class Maze2D : ExtensibleObject {
        private readonly Vector _size;
        private readonly NArray<MazeCell> _cells;
        /// <summary>
        /// A read-only access to all cells in this maze field.
        /// </summary>
        /// <remarks>
        /// Contains the full field of cells even if the cells are not a part
        /// of the maze.
        /// </remarks>
        public NArray<MazeCell> Cells => _cells;
        /// <summary>
        /// Maze cells, i.e. cells that belong to the maze and have links to 
        /// other maze cells.
        /// </summary>
        public IEnumerable<MazeCell> MazeCells =>
            _cells.Where(cell => cell.IsConnected);

        /// <summary />
        public Vector Size { get => _cells.Size; }

        /// <summary>
        /// If the longest path was set, returns the longest path. See
        /// <see cref="DijkstraDistance"/>.
        /// </summary>
        public Optional<DijkstraDistance.LongestTrailExtension> LongestPath {
            get => X<DijkstraDistance.LongestTrailExtension>() ??
                   Optional<DijkstraDistance.LongestTrailExtension>.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">columns</param>
        /// <param name="y">rows</param>
        [Obsolete]
        public Maze2D(int x, int y) :
            this(Area.CreateEnvironment(new Vector(x, y), xy => new Cell(xy))) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseArea">Area on which the maze is generated</param>
        public Maze2D(Area baseArea) {
            if (baseArea.Size.Dimensions != 2) {
                throw new NotImplementedException(
                    "At the moment, only 2D mazes are supported.");
            }
            _size = baseArea.Size;
            _cells = new NArray<MazeCell>(
                _size, xy => new MazeCell(baseArea.Cells[xy]));
            foreach (var cell in _cells.Iterate()) {
                var north = cell.xy + Vector.North2D;
                if (north.Y < baseArea.Size.Y) {
                    cell.cell.Neighbors().Add(_cells[north]);
                    _cells[north].Neighbors().Add(cell.cell);
                }

                var west = cell.xy + Vector.West2D;
                if (west.X >= 0) {
                    cell.cell.Neighbors().Add(_cells[west]);
                    _cells[west].Neighbors().Add(cell.cell);
                }
            }
        }

        private readonly Dictionary<MapArea, ICollection<MazeCell>>
            _mapAreas = new Dictionary<MapArea, ICollection<MazeCell>>();

        /// <summary>
        /// Areas assigned to this maze. See <see cref="Maps.Areas"/>.
        /// </summary>
        public Dictionary<MapArea, ICollection<MazeCell>> MapAreas => _mapAreas;

        internal void AddArea(MapArea area) {
            var areaCells = _cells.IterateIntersection(area.Position, area.Size)
                .Select(cell => cell.cell)
                .ToList();
            _mapAreas.Add(area, areaCells);
            foreach (var cell in areaCells) {
                cell.AddMapArea(area, areaCells);
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
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail }, Cell.CellTag.MazeWall, options.WallWidths.Min(), options.WallHeights.Min()))
                .With(new Map2DSmoothCorners(Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner, options.WallWidths.Min(), options.WallHeights.Min()))
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner }, Cell.CellTag.MazeWall, options.WallWidths.Min(), options.WallHeights.Min()))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeVoid }, true, Cell.CellTag.MazeWall, 5, 5))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeWall, Cell.CellTag.MazeWallCorner }, false, Cell.CellTag.MazeTrail, 3, 3))
                .Render(map);
            return map;
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
            var areas = string.Join(",", _mapAreas.Select(area => area.Key.ToString()));
            var cells = string.Join(",", _cells.Select((cell, index) => {
                if (_cells[index].Links().Count > 0) {
                    var links = _cells[index].Links()
                        .Select(link => link.Position.ToIndex(_size))
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
        /// Renders this maze to a string using
        /// <see cref="Maze2DStringBoxRenderer" />.
        /// </summary>
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
        // TODO: Add Areas
        public static Maze2D Parse(string serialized) {
            if (serialized.IndexOf('|') == -1) {
                // TODO: Migrate all serialization to the other format.
                var parts = serialized.Split(';', '\n');
                var size = new Vector(parts[0].Split('x').Select(int.Parse));
                var maze = new Maze2D(size.X, size.Y);
                for (var i = 1; i < parts.Length; i++) {
                    var part = parts[i].Split(':', ',').Select(int.Parse).ToArray();
                    for (var j = 1; j < part.Length; j++) {
                        maze._cells[part[0]].Link(maze._cells[part[j]]);
                    }
                }
                return maze;
            } else {
                var linksAdded = new HashSet<string>();
                var parts = serialized.Split('|');
                var size = Vector.Parse(parts[0]);
                var maze = new Maze2D(size.X, size.Y);
                parts[1].Split(',')
                    .ForEach(areaStr => maze.AddArea(MapArea.Parse(areaStr)));
                parts[2].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ForEach(cellStr => {
                    var part = cellStr.Split(':', ' ').Select(int.Parse).ToArray();
                    for (var j = 1; j < part.Length; j++) {
                        if (linksAdded.Contains($"{part[0]}|{part[j]}")) continue;
                        maze._cells[part[0]].Link(maze._cells[part[j]]);
                        linksAdded.Add($"{part[0]}|{part[j]}");
                        linksAdded.Add($"{part[j]}|{part[0]}");
                    }
                });
                return maze;
            }
        }
    }
}