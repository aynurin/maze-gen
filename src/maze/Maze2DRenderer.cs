using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayersWorlds.Maps.MapFilters;

namespace PlayersWorlds.Maps.Maze {

    public class Maze2DRenderer {
        private readonly Maze2D _maze;
        private readonly MazeToMapOptions _options;

        internal static Map2D CreateMapForMaze(
            Maze2D maze, MazeToMapOptions options) =>
                new Map2D(options.RenderedSize(maze.Size));

        public Maze2DRenderer(Maze2D maze, MazeToMapOptions options) {
            maze.ThrowIfNull("maze");
            options.ThrowIfNull("options");
            options.ThrowIfWrong(maze.Size);

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
            return map.Size.Fits(options.RenderedSize(_maze.Size));
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

                _size[SW] = _options.WallSize(_mazeCell.Coordinates);
                _position[SW] = _options.SWPosition(_mazeCell.Coordinates);
                _size[CENTER] = _options.TrailSize(_mazeCell.Coordinates);
                _position[CENTER] = _position[SW] + _size[SW];
                _size[NE] = _options.WallSize(
                    _mazeCell.Coordinates + Vector.NorthEast2D);
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
            public int[] TrailWidths { get; }
            public int[] TrailHeights { get; }
            public int[] WallWidths { get; }
            public int[] WallHeights { get; }

            public Vector WallSize(Vector mazeCellPosition) =>
                new Vector(
                    WallWidths.Length == 1 ?
                        WallWidths[0] : WallWidths[mazeCellPosition.X],
                    WallHeights.Length == 1 ?
                        WallHeights[0] : WallHeights[mazeCellPosition.Y]);

            public Vector TrailSize(Vector mazeCellPosition) =>
                new Vector(
                    TrailWidths.Length == 1 ?
                        TrailWidths[0] : TrailWidths[mazeCellPosition.X],
                    TrailHeights.Length == 1 ?
                        TrailHeights[0] : TrailHeights[mazeCellPosition.Y]);

            public Vector SWPosition(Vector mazeCellPosition) {
                var trailPart = new Vector(
                    TrailWidths.Length == 1 ?
                        TrailWidths[0] * mazeCellPosition.X :
                        TrailWidths
                            .Where((v, vi) => vi < mazeCellPosition.X)
                            .Sum(),
                    TrailHeights.Length == 1 ?
                        TrailHeights[0] * mazeCellPosition.Y :
                        TrailHeights
                            .Where((v, vi) => vi < mazeCellPosition.Y)
                            .Sum());
                var wallPart = new Vector(
                    WallWidths.Length == 1 ?
                        WallWidths[0] * mazeCellPosition.X :
                        WallWidths
                            .Where((v, vi) => vi < mazeCellPosition.X)
                            .Sum(),
                    WallHeights.Length == 1 ?
                        WallHeights[0] * mazeCellPosition.Y :
                        WallHeights
                            .Where((v, vi) => vi < mazeCellPosition.Y)
                            .Sum());
                return trailPart + wallPart;
            }

            public Vector RenderedSize(Vector mazeSize) {
                ThrowIfWrong(mazeSize);
                return SWPosition(mazeSize) +
                    new Vector(
                        WallWidths.Length == 1 ?
                            WallWidths[0] : WallWidths[mazeSize.X],
                        WallHeights.Length == 1 ?
                            WallHeights[0] : WallHeights[mazeSize.Y]);
            }

            public MazeToMapOptions(
                int[] trailWidths,
                int[] trailHeights,
                int[] wallWidths,
                int[] wallHeights) {
                trailWidths.ThrowIfNullOrEmpty("trailWidths");
                trailHeights.ThrowIfNullOrEmpty("trailWidths");
                wallWidths.ThrowIfNullOrEmpty("trailWidths");
                wallHeights.ThrowIfNullOrEmpty("trailYHeights");
                if (trailWidths.Any(v => v <= 0) ||
                    trailHeights.Any(v => v <= 0) ||
                    wallWidths.Any(v => v <= 0) ||
                    wallHeights.Any(v => v <= 0)) {
                    throw new ArgumentException("Zero and negative wall and " +
                        "trail sizes are not supported.");
                }
                TrailWidths = trailWidths;
                TrailHeights = trailHeights;
                WallWidths = wallWidths;
                WallHeights = wallHeights;
            }

            public static MazeToMapOptions SquareCells(
                int trailCellSize,
                int wallCellSize)
                => new MazeToMapOptions(
                    new int[] { trailCellSize },
                    new int[] { trailCellSize },
                    new int[] { wallCellSize },
                    new int[] { wallCellSize }
                );

            public static MazeToMapOptions RectCells(
                Vector trailCellSize,
                Vector wallCellSize)
                => new MazeToMapOptions(
                    new int[] { trailCellSize.X },
                    new int[] { trailCellSize.Y },
                    new int[] { wallCellSize.X },
                    new int[] { wallCellSize.Y }
                );

            public static MazeToMapOptions Custom(
                int[] trailWidths,
                int[] trailHeights,
                int[] wallWidths,
                int[] wallHeights)
                => new MazeToMapOptions(trailWidths, trailHeights, wallWidths, wallHeights);

            public void ThrowIfWrong(Vector mazeSize) {
                var msg = "Please provide {0} for all {1}. The provided maze " +
                          "({2}) should have {3} {1} {0} (or only one, same " +
                          "for all {1}), and the provided {0} are ({4})";
                if (TrailWidths.Length > 1 &&
                    TrailWidths.Length != mazeSize.X) {
                    throw new ArgumentException(
                        string.Format(
                            msg, "widths", "trails", mazeSize, mazeSize.X,
                            string.Join(", ", TrailWidths)));
                }
                if (TrailHeights.Length > 1 &&
                    TrailHeights.Length != mazeSize.Y) {
                    throw new ArgumentException(
                        string.Format(
                            msg, "heights", "trails", mazeSize, mazeSize.Y,
                            string.Join(", ", TrailHeights)));
                }
                if (WallWidths.Length > 1 &&
                    WallWidths.Length != mazeSize.X + 1) {
                    throw new ArgumentException(
                        string.Format(
                            msg, "widths", "walls", mazeSize, mazeSize.X + 1,
                            string.Join(", ", WallWidths)));
                }
                if (WallHeights.Length > 1 &&
                    WallHeights.Length != mazeSize.Y + 1) {
                    throw new ArgumentException(
                        string.Format(
                            msg, "heights", "walls", mazeSize, mazeSize.Y + 1,
                            string.Join(", ", WallHeights)));
                }
            }
        }
    }
}