using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nour.Play.MapFilters;

namespace Nour.Play.Maze {

    public class Maze2DRenderer {
        private readonly Maze2D _maze;
        private readonly MazeToMapOptions _options;

        internal static Map2D CreateMapForMaze(MazeToMapOptions options) {
            var mapSize = new Vector(options.TrailXWidths.Sum(),
                                     options.TrailYHeights.Sum()) +
                          new Vector(options.WallXWidths.Sum(),
                                     options.WallYHeights.Sum());
            return new Map2D(mapSize);
        }

        public Maze2DRenderer(Maze2D maze, MazeToMapOptions options) {
            maze.ThrowIfNull("maze");
            options.ThrowIfNull("options");
            options.ThrowIfWrong(maze);

            _maze = maze;
            _options = options;
        }

        public void Render(Map2D map) {
            if (!Fits(map, _options)) {
                throw new ArgumentException("Map does not fit the maze.");
            }
            foreach (var cell in _maze.AllCells) {
                var mapping = new CellsMapping(map, cell, _options);
                if (cell.IsVisited) {
                    mapping.CenterCells.ForEach(c => c.Tags.Add(MapCellType.Trail));
                }
                if (cell.Links(Vector.North2D).HasValue) {
                    mapping.NCells.ForEach(c => c.Tags.Add(MapCellType.Trail));
                }
                if (cell.Links(Vector.East2D).HasValue) {
                    mapping.ECells.ForEach(c => c.Tags.Add(MapCellType.Trail));
                }
                if (cell.Links(Vector.South2D).HasValue) {
                    mapping.SCells.ForEach(c => c.Tags.Add(MapCellType.Trail));
                }
                if (cell.Links(Vector.West2D).HasValue) {
                    mapping.WCells.ForEach(c => c.Tags.Add(MapCellType.Trail));
                }
            }
            new Map2DOutline(new[] { MapCellType.Trail }, MapCellType.Wall, 1, 1).Render(map);
            new Map2DAntialias(MapCellType.Trail, MapCellType.Edge, 1, 1).Render(map);
            new Map2DOutline(new[] { MapCellType.Trail, MapCellType.Edge }, MapCellType.Wall, 1, 1).Render(map);
            new Map2DFillGaps(new[] { MapCellType.Void }, true, MapCellType.Wall, 5, 5).Render(map);
            new Map2DFillGaps(new[] { MapCellType.Wall, MapCellType.Edge }, false, MapCellType.Trail, 3, 3).Render(map);
        }

        public bool Fits(Map2D map, MazeToMapOptions options) {
            return map.Size.X >=
                    options.TrailXWidths.Sum() + options.WallXWidths.Sum()
                && map.Size.Y >=
                    options.TrailYHeights.Sum() + options.WallYHeights.Sum();
        }

        public CellsMapping CreateCellsMapping(Map2D map, MazeCell mazeCell)
            => new CellsMapping(map, mazeCell, _options);

        public static class MapCellType {
            public const string Wall = "MAZE2D_WALL";
            public const string Trail = "MAZE2D_TRAIL";
            public const string Edge = "MAZE2D_EDGE";
            public const string Void = "MAZE2D_VOID";
        }

        public class CellsMapping {
            private readonly Map2D _map;
            private readonly MazeCell _mazeCell;
            private readonly MazeToMapOptions _options;
            private readonly Vector[] _size = new Vector[9];
            private readonly Vector[] _position = new Vector[9];
            private const int NW = 0, N = 1, NE = 2, W = 3, CENTER = 4, E = 5, SW = 6, S = 7, SE = 8;

            public CellsMapping(Map2D map, MazeCell mazeCell, MazeToMapOptions options) {
                _map = map;
                _mazeCell = mazeCell;
                _options = options;

                _size[SW] = new Vector(
                    _options.WallXWidths[_mazeCell.X],
                    _options.WallYHeights[_mazeCell.Y]);
                _position[SW] = new Vector(
                    _options.TrailXWidths
                        .Where((a, ai) => ai < mazeCell.X).Sum() +
                    _options.WallXWidths
                        .Where((a, ai) => ai < mazeCell.X).Sum(),
                    _options.TrailYHeights
                        .Where((a, ai) => ai < mazeCell.Y).Sum() +
                    _options.WallYHeights
                        .Where((a, ai) => ai < mazeCell.Y).Sum()
                );
                _size[CENTER] = new Vector(
                    _options.TrailXWidths[_mazeCell.X],
                    _options.TrailYHeights[_mazeCell.Y]);
                _position[CENTER] = _position[SW] + _size[SW];
                _size[NE] = new Vector(
                    _options.WallXWidths[_mazeCell.X + 1],
                    _options.WallYHeights[_mazeCell.Y + 1]);
                _position[NE] = _position[CENTER] + _size[CENTER];

                _size[NW] = new Vector(_size[SW].X, _size[NE].Y);
                _position[NW] = new Vector(_position[SW].X, _position[NE].Y);
                _size[N] = new Vector(_size[CENTER].X, _size[NE].Y);
                _position[N] = new Vector(_position[CENTER].X, _position[NE].Y);
                _size[W] = new Vector(_size[SW].X, _size[CENTER].Y);
                _position[W] = new Vector(_position[SW].X, _position[CENTER].Y);
                _size[E] = new Vector(_size[NE].X, _size[CENTER].Y);
                _position[E] = new Vector(_position[NE].X, _position[CENTER].Y);
                _size[S] = new Vector(_size[CENTER].X, _size[SW].Y);
                _position[S] = new Vector(_position[CENTER].X, _position[SW].Y);
                _size[SE] = new Vector(_size[NE].X, _size[SW].Y);
                _position[SE] = new Vector(_position[NE].X, _position[SW].Y);
            }

            // y │    N
            //   │  W C E
            //   │    S
            //   └─────────
            //           x

            public Vector CenterPosition => _position[CENTER];
            public Vector CenterSize => _size[CENTER];
            public Vector NWPosition => _position[NW];
            public Vector NWSize => _size[NW];
            public Vector NPosition => _position[N];
            public Vector NSize => _size[N];
            public Vector NEPosition => _position[NE];
            public Vector NESize => _size[NE];
            public Vector WPosition => _position[W];
            public Vector WSize => _size[W];
            public Vector EPosition => _position[E];
            public Vector ESize => _size[E];
            public Vector SWPosition => _position[SW];
            public Vector SWSize => _size[SW];
            public Vector SPosition => _position[S];
            public Vector SSize => _size[S];
            public Vector SEPosition => _position[SE];
            public Vector SESize => _size[SE];

            public IEnumerable<Cell> CenterCells =>
                _map.CellsAt(_position[CENTER], _size[CENTER]);

            public IEnumerable<Cell> NWCells =>
                _map.CellsAt(_position[NW], _size[NW]);

            public IEnumerable<Cell> NCells =>
                _map.CellsAt(_position[N], _size[N]);

            public IEnumerable<Cell> NECells =>
                _map.CellsAt(_position[NE], _size[NE]);

            public IEnumerable<Cell> WCells =>
                _map.CellsAt(_position[W], _size[W]);

            public IEnumerable<Cell> ECells =>
                _map.CellsAt(_position[E], _size[E]);

            public IEnumerable<Cell> SWCells =>
                _map.CellsAt(_position[SW], _size[SW]);

            public IEnumerable<Cell> SCells =>
                _map.CellsAt(_position[S], _size[S]);

            public IEnumerable<Cell> SECells =>
                _map.CellsAt(_position[SE], _size[SE]);


        }

        public class MazeToMapOptions {
            public int[] TrailXWidths { get; private set; }
            public int[] TrailYHeights { get; private set; }
            public int[] WallXWidths { get; private set; }
            public int[] WallYHeights { get; private set; }

            public MazeToMapOptions(
                int[] trailXWidths,
                int[] trailYHeights,
                int[] wallXWidths,
                int[] wallYHeights) {
                trailXWidths.ThrowIfNull("trailXWidths");
                trailYHeights.ThrowIfNull("trailYHeights");
                wallXWidths.ThrowIfNull("wallXWidths");
                wallYHeights.ThrowIfNull("wallYHeights");
                if (trailXWidths.Any(i => i <= 0) ||
                    trailYHeights.Any(i => i <= 0) ||
                    wallXWidths.Any(i => i <= 0) ||
                    wallYHeights.Any(i => i <= 0)) {
                    throw new ArgumentException("Zero and negative wall and " +
                        "trail widths are not supported.");
                }
                TrailXWidths = trailXWidths;
                TrailYHeights = trailYHeights;
                WallXWidths = wallXWidths;
                WallYHeights = wallYHeights;
            }

            public static MazeToMapOptions Create(
                int trailSize,
                int wallSize,
                Vector mazeSize)
                => new MazeToMapOptions(
                    /* trailXWidths = */ Enumerable.Repeat(trailSize, mazeSize.X).ToArray(),
                    /* trailYHeights = */ Enumerable.Repeat(trailSize, mazeSize.Y).ToArray(),
                    /* wallXWidths = */ Enumerable.Repeat(wallSize, Math.Max(mazeSize.X + 1, 0)).ToArray(),
                    /* wallYHeights = */ Enumerable.Repeat(wallSize, Math.Max(mazeSize.Y + 1, 0)).ToArray());

            public static MazeToMapOptions Custom(
                int trailXWidths,
                int trailYHeights,
                int wallXWidths,
                int wallYHeights,
                Vector mazeSize)
                => new MazeToMapOptions(
                    /* trailXWidths = */ Enumerable.Repeat(trailXWidths, mazeSize.X).ToArray(),
                    /* trailYHeights = */ Enumerable.Repeat(trailYHeights, mazeSize.Y).ToArray(),
                    /* wallXWidths = */ Enumerable.Repeat(wallXWidths, Math.Max(mazeSize.X + 1, 0)).ToArray(),
                    /* wallYHeights = */ Enumerable.Repeat(wallYHeights, Math.Max(mazeSize.Y + 1, 0)).ToArray());

            public void ThrowIfWrong(Maze2D maze) {
                if (TrailXWidths.Length != maze.XWidthColumns ||
                    TrailYHeights.Length != maze.YHeightRows ||
                    WallXWidths.Length != maze.XWidthColumns + 1 ||
                    WallYHeights.Length != maze.YHeightRows + 1) {
                    throw new ArgumentException("The provided Walls and " +
                        "trails counts need to match maze size");
                }
            }
        }
    }
}