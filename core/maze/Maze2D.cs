using System;
using System.Collections.Generic;
using System.Linq;
using Nour.Play.Renderers;

namespace Nour.Play.Maze {
    public class Maze2D {
        private readonly Vector _size;
        private readonly List<MazeCell> _cells;
        public Dictionary<string, List<MazeCell>> Attributes { get; } =
            new Dictionary<string, List<MazeCell>>();

        public IList<MazeCell> Cells => _cells.AsReadOnly();
        public IEnumerable<MazeCell> VisitedCells =>
            _cells.Where(cell => cell.IsVisited);

        public MazeCell this[int x, int y] {
            get {
                var index = x * _size.Y + y;
                return _cells[index];
            }
        }

        public int XHeightRows { get => _size.X; }

        public int YWidthColumns { get => _size.Y; }

        public Vector Size { get => _size; }

        public int Area { get => _size.Area; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">rows</param>
        /// <param name="y">columns</param>
        public Maze2D(int x, int y) : this(new Vector(x, y)) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">Map two-dimensional size, where X - rows (height), and Y - columns (width)</param>
        public Maze2D(Vector size) {
            _size = size;
            if (_size.X <= 0 || _size.Y <= 0)
                throw new ArgumentException("Map size must be greater than 0", "size");
            _cells = new List<MazeCell>(_size.Area);
            // ? P'haps the direction is a property of the gate, not it's identity.
            for (int i = 0; i < _cells.Capacity; i++) {
                var cell = new MazeCell(i / _size.Y, i % _size.Y);
                if (cell.X > 0) {
                    cell.Neighbors().Add(this[cell.X - 1, cell.Y]);
                    this[cell.X - 1, cell.Y].Neighbors().Add(cell);
                }
                if (cell.Y > 0) {
                    cell.Neighbors().Add(this[cell.X, cell.Y - 1]);
                    this[cell.X, cell.Y - 1].Neighbors().Add(cell);
                }
                _cells.Add(cell);
            }
        }

        public Map2D ToMap(Maze2DToMap2DConverter.MazeToMapOptions options) {
            return new Maze2DToMap2DConverter().Convert(this, options);
        }

        public override string ToString() {
            return new Maze2DAsciiBoxRenderer(this).WithTrail();
        }

        public static Maze2D Parse(string serialized) {
            var parts = serialized.Split(';', '\n');
            var maze = new Maze2D(new Vector(parts[0].Split('x').Select(Int32.Parse)));
            for (int i = 1; i < parts.Length; i++) {
                var part = parts[i].Split(':', ',').Select(Int32.Parse).ToArray();
                for (int j = 1; j < part.Length; j++)
                    maze.Cells[part[0]].Link(maze.Cells[part[j]]);
            }
            return maze;
        }
    }
}