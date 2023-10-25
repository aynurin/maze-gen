using System;
using System.Collections.Generic;
using System.Linq;

namespace Nour.Play.Maze {
    public class Maze2D {
        private readonly Vector _size;
        private readonly List<MazeCell> _cells;
        public Dictionary<string, List<MazeCell>> Attributes { get; } =
            new Dictionary<string, List<MazeCell>>();

        public IList<MazeCell> Cells => _cells.AsReadOnly();

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

        public Map2D ToMap(MazeToMapOptions options) {
            options.ThrowIfNull("options");
            options.ThrowIfWrong(this);

            var newSize = new Vector(options.TrailXHeights.Sum(),
                                     options.TrailYWidths.Sum()) +
                          new Vector(options.WallXHeights.Sum(),
                                     options.WallYWidths.Sum());
            var newMap = new Map2D(newSize);
            for (int i = 0; i < this.Cells.Count; i++) {
                var mazeCell = this.Cells[i];
                var scaledX = options.TrailXHeights.Where(
                                    (a, ai) => ai < mazeCell.X).Sum() +
                              options.WallXHeights.Where(
                                    (a, ai) => ai < mazeCell.X).Sum();
                var scaledY = options.TrailYWidths.Where(
                                    (a, ai) => ai < mazeCell.Y).Sum() +
                              options.WallYWidths.Where(
                                    (a, ai) => ai < mazeCell.Y).Sum();
                newMap.CellsAt(new Vector(scaledX, scaledY),
                               new Vector(
                                    options.TrailXHeights[mazeCell.X],
                                    options.TrailYWidths[mazeCell.Y]))
                      .ForEach(c => c.Type = Cell.CellType.Trail);
                if (mazeCell.Neighbors(Vector.East2D).HasValue) {
                    newMap.CellsAt(new Vector(scaledX,
                                              scaledY + options.TrailYWidths[mazeCell.Y]),
                                   new Vector(options.TrailXHeights[mazeCell.X],
                                              options.WallYWidths[mazeCell.Y]))
                          .ForEach(c => c.Type =
                                   mazeCell.Links(Vector.East2D).HasValue ?
                                        Cell.CellType.Trail :
                                        Cell.CellType.Wall);
                }
                if (mazeCell.Neighbors(Vector.South2D).HasValue) {
                    newMap.CellsAt(new Vector(scaledX + options.TrailXHeights[mazeCell.X],
                                              scaledY),
                                   new Vector(options.WallXHeights[mazeCell.X],
                                              options.TrailYWidths[mazeCell.Y]))
                          .ForEach(c => c.Type =
                                   mazeCell.Links(Vector.South2D).HasValue ?
                                        Cell.CellType.Trail :
                                        Cell.CellType.Wall);
                }
                if (mazeCell.Neighbors(Vector.East2D).HasValue &&
                    mazeCell.Neighbors(Vector.South2D).HasValue) {
                    newMap.CellsAt(new Vector(scaledX + options.TrailXHeights[mazeCell.X],
                                              scaledY + options.TrailYWidths[mazeCell.Y]),
                                   new Vector(options.WallXHeights[mazeCell.X],
                                              options.WallYWidths[mazeCell.Y]))
                          .ForEach(c => c.Type =
                                   mazeCell.Links(Vector.East2D).HasValue &&
                                   mazeCell.Links(Vector.South2D).HasValue &&
                                   mazeCell.Links(Vector.East2D).Value.Links(Vector.South2D).HasValue &&
                                   mazeCell.Links(Vector.South2D).Value.Links(Vector.East2D).HasValue ?
                                        Cell.CellType.Trail :
                                        Cell.CellType.Edge);
                }
            }
            return newMap;
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

        public class MazeToMapOptions {
            public int[] TrailXHeights { get; private set; }
            public int[] TrailYWidths { get; private set; }
            public int[] WallXHeights { get; private set; }
            public int[] WallYWidths { get; private set; }

            // option 2: custom sizes
            public static MazeToMapOptions Custom(
                int[] trailXHeights,
                int[] trailYWidths,
                int[] wallXHeights,
                int[] wallYWidths)
                => new MazeToMapOptions(
                    trailXHeights,
                    trailYWidths,
                    wallXHeights,
                    wallYWidths);

            public MazeToMapOptions(
                int[] trailXHeights,
                int[] trailYWidths,
                int[] wallXHeights,
                int[] wallYWidths) {
                trailXHeights.ThrowIfNull("trailXHeights");
                trailYWidths.ThrowIfNull("trailYWidths");
                wallXHeights.ThrowIfNull("wallXHeights");
                wallYWidths.ThrowIfNull("wallYWidths");
                TrailXHeights = trailXHeights;
                TrailYWidths = trailYWidths;
                WallXHeights = wallXHeights;
                WallYWidths = wallYWidths;
            }

            public void ThrowIfWrong(Maze2D maze) {
                if (TrailXHeights.Length != maze.XHeightRows ||
                    TrailYWidths.Length != maze.YWidthColumns ||
                    WallXHeights.Length != maze.XHeightRows - 1 ||
                    WallYWidths.Length != maze.YWidthColumns - 1) {
                    throw new ArgumentException("The provided Walls and " +
                        "trails counts need to match maze size");
                }
                if (TrailXHeights.Any(i => i <= 0) ||
                    TrailYWidths.Any(i => i <= 0) ||
                    WallXHeights.Any(i => i <= 0) ||
                    WallYWidths.Any(i => i <= 0)) {
                    throw new ArgumentException("Zero and negative wall and " +
                        "trail widths are not supported.");
                }
            }
        }
    }
}