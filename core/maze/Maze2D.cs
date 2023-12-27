using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Nour.Play.Areas;
using Nour.Play.Maze.PostProcessing;
using Nour.Play.Renderers;

namespace Nour.Play.Maze {
    public class Maze2D {
        private readonly Vector _size;
        private readonly List<MazeCell> _cells;
        private readonly List<MazeCell> _visitableCells;
        public Dictionary<string, List<MazeCell>> Attributes { get; } =
            new Dictionary<string, List<MazeCell>>();
        public IList<MazeCell> AllCells => _cells.AsReadOnly();
        public IList<MazeCell> VisitableCells => _visitableCells.AsReadOnly();
        public IEnumerable<MazeCell> VisitedCells =>
            _cells.Where(cell => cell.IsVisited);

        public int XWidthColumns { get => _size.X; }

        public int YHeightRows { get => _size.Y; }

        public Vector Size { get => _size; }

        public int Area { get => _size.Area; }

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
        /// <param name="size">Map two-dimensional size,
        ///     where X - width, number of columns,
        ///     and Y - height, number of rows</param>
        public Maze2D(Vector size) {
            size.ThrowIfNotAValidSize();
            _size = size;
            var cells = new MazeCell[_size.Area];
            // ? P'haps the direction is a property of the gate, not it's identity.
            for (var i = 0; i < cells.Length; i++) {
                var xy = Vector.FromIndex(i, _size.X);
                var northI = i - _size.X;
                var westI = xy.X > 0 ? (i - 1) : -1;
                var cell = new MazeCell(xy);
                if (northI >= 0) {
                    cell.Neighbors().Add(cells[northI]);
                    cells[northI].Neighbors().Add(cell);
                }
                if (westI >= 0) {
                    cell.Neighbors().Add(cells[westI]);
                    cells[westI].Neighbors().Add(cell);
                }
                cells[i] = cell;
            }
            _cells = new List<MazeCell>(cells);
            _visitableCells = new List<MazeCell>(cells);
        }

        public List<MapArea> Areas { get; private set; } = new List<MapArea>();

        internal void AddArea(MapArea area) {
            Areas.Add(area);
            var areaCells = new List<MazeCell>();
            for (var x = 0; x < area.Size.X; x++) {
                for (var y = 0; y < area.Size.Y; y++) {
                    areaCells.Add(
                        _cells[(new Vector(x, y) + area.Position)
                            .ToIndex(this.Size.X)]);
                }
            }
            foreach (var cell in areaCells) {
                cell.AssignMapArea(area, areaCells);
                if (area.Type == AreaType.Fill) {
                    _visitableCells.Remove(cell);
                }
            }
        }

        public Map2D ToMap(Maze2DRenderer.MazeToMapOptions options) {
            var map = Maze2DRenderer.CreateMapForMaze(options);
            new Maze2DRenderer(this, options).Render(map);
            return map;
        }

        public override string ToString() {
            return new Maze2DAsciiBoxRenderer(this).WithTrail();
        }

        public static Maze2D Parse(string serialized) {
            var parts = serialized.Split(';', '\n');
            var maze = new Maze2D(new Vector(parts[0].Split('x').Select(int.Parse)));
            for (var i = 1; i < parts.Length; i++) {
                var part = parts[i].Split(':', ',').Select(int.Parse).ToArray();
                for (var j = 1; j < part.Length; j++)
                    maze.AllCells[part[0]].Link(maze.AllCells[part[j]]);
            }
            return maze;
        }
    }
}