using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayersWorlds.Maps.MapFilters;

namespace PlayersWorlds.Maps.Maze {

    /// <summary>
    /// Renders a <see cref="Maze2D" /> to a string.
    /// </summary>
    public class Maze2DRenderer {
        private readonly Area _maze;
        private readonly MazeToMapOptions _options;
        private readonly List<Map2DFilter> _filters = new List<Map2DFilter>();

        internal static Area CreateMapForMaze(
            Area maze, MazeToMapOptions options) =>
                Area.CreateEnvironment(options.RenderedSize(maze.Size));

        /// <summary />
        public Maze2DRenderer(Area maze, MazeToMapOptions options) {
            maze.ThrowIfNull("maze");
            options.ThrowIfNull("options");
            options.ThrowIfWrong(maze.Size);

            _maze = maze;
            _options = options;
        }

        /// <summary>
        /// Applies the specified <see cref="Map2DFilter" />s while rendering.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Maze2DRenderer With(Map2DFilter filter) {
            _filters.Add(filter);
            return this;
        }

        /// <summary>
        /// Renders a <see cref="Area" /> to a string.
        /// </summary>
        public void Render(Area map) {
            if (!_options.RenderedSize(_maze.Size).FitsInto(map.Size)) {
                throw new ArgumentException("Map does not fit the maze.");
            }
            foreach (var cell in _maze.Cells) {
                var mapping = new CellsMapping(map, cell, _options);
                if (cell.Position == new Vector(1, 1)) {
                    System.Diagnostics.Debugger.Break();
                }
                if (cell.IsConnected) {
                    mapping.CenterCells.ForEach(c => c.Tags.Add(Cell.CellTag.MazeTrail));
                }
                if (_maze.CellsAreLinked(cell.Position, cell.Position + Vector.North2D)) {
                    mapping.NCells.ForEach(c => c.Tags.Add(Cell.CellTag.MazeTrail));
                }
                if (_maze.CellsAreLinked(cell.Position, cell.Position + Vector.East2D)) {
                    mapping.ECells.ForEach(c => c.Tags.Add(Cell.CellTag.MazeTrail));
                }
                if (_maze.CellsAreLinked(cell.Position, cell.Position + Vector.South2D)) {
                    mapping.SCells.ForEach(c => c.Tags.Add(Cell.CellTag.MazeTrail));
                }
                if (_maze.CellsAreLinked(cell.Position, cell.Position + Vector.West2D)) {
                    mapping.WCells.ForEach(c => c.Tags.Add(Cell.CellTag.MazeTrail));
                }
            }
            foreach (var filter in _filters) {
                filter.Render(map);
            }
        }

        internal class CellsMapping {
            private readonly Area _map;
            private readonly Cell _mazeCell;
            private readonly MazeToMapOptions _options;
            private readonly Vector[] _size = new Vector[9];
            private readonly Vector[] _position = new Vector[9];
            private const int NW = 0, N = 1, NE = 2, W = 3, CENTER = 4, E = 5, SW = 6, S = 7, SE = 8;

            public CellsMapping(Area map, Cell mazeCell, MazeToMapOptions options) {
                map.ThrowIfNull(nameof(map));
                mazeCell.ThrowIfNull(nameof(mazeCell));
                options.ThrowIfNull(nameof(options));

                _map = map;
                _mazeCell = mazeCell;
                _options = options;

                _size[SW] = _options.WallSize(_mazeCell.Position);
                _position[SW] = _options.SWPosition(_mazeCell.Position);
                _size[CENTER] = _options.TrailSize(_mazeCell.Position);
                _position[CENTER] = _position[SW] + _size[SW];
                _size[NE] = _options.WallSize(
                    _mazeCell.Position + Vector.NorthEast2D);
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
                _map.Cells.Iterate(_position[CENTER], _size[CENTER])
                    .Select(c => c.cell);

            public IEnumerable<Cell> NWCells =>
                _map.Cells.Iterate(_position[NW], _size[NW])
                    .Select(c => c.cell);

            public IEnumerable<Cell> NCells =>
                _map.Cells.Iterate(_position[N], _size[N])
                    .Select(c => c.cell);

            public IEnumerable<Cell> NECells =>
                _map.Cells.Iterate(_position[NE], _size[NE])
                    .Select(c => c.cell);

            public IEnumerable<Cell> WCells =>
                _map.Cells.Iterate(_position[W], _size[W])
                    .Select(c => c.cell);

            public IEnumerable<Cell> ECells =>
                _map.Cells.Iterate(_position[E], _size[E])
                    .Select(c => c.cell);

            public IEnumerable<Cell> SWCells =>
                _map.Cells.Iterate(_position[SW], _size[SW])
                    .Select(c => c.cell);

            public IEnumerable<Cell> SCells =>
                _map.Cells.Iterate(_position[S], _size[S])
                    .Select(c => c.cell);

            public IEnumerable<Cell> SECells =>
                _map.Cells.Iterate(_position[SE], _size[SE])
                    .Select(c => c.cell);


        }

        /// <summary>
        /// Maze rendering options.
        /// </summary>
        public class MazeToMapOptions {
            internal int[] TrailWidths { get; }
            internal int[] TrailHeights { get; }
            internal int[] WallWidths { get; }
            internal int[] WallHeights { get; }

            internal Vector WallSize(Vector mazeCellPosition) =>
                new Vector(
                    WallWidths.Length == 1 ?
                        WallWidths[0] : WallWidths[mazeCellPosition.X],
                    WallHeights.Length == 1 ?
                        WallHeights[0] : WallHeights[mazeCellPosition.Y]);

            internal Vector TrailSize(Vector mazeCellPosition) =>
                new Vector(
                    TrailWidths.Length == 1 ?
                        TrailWidths[0] : TrailWidths[mazeCellPosition.X],
                    TrailHeights.Length == 1 ?
                        TrailHeights[0] : TrailHeights[mazeCellPosition.Y]);

            internal Vector SWPosition(Vector mazeCellPosition) {
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

            internal Vector RenderedSize(Vector mazeSize) {
                ThrowIfWrong(mazeSize);
                return SWPosition(mazeSize) +
                    new Vector(
                        WallWidths.Length == 1 ?
                            WallWidths[0] : WallWidths[mazeSize.X],
                        WallHeights.Length == 1 ?
                            WallHeights[0] : WallHeights[mazeSize.Y]);
            }

            /// <summary>
            /// Creates new maze rendering options with the specified walls and
            /// trails sizes.
            /// </summary>
            /// <param name="trailWidths">Widths of all trail cells.</param>
            /// <param name="trailHeights">Heights of all trail cells.</param>
            /// <param name="wallWidths">Widths of all wall cells.</param>
            /// <param name="wallHeights">Heights of all wall cells.</param>
            /// <exception cref="ArgumentException"></exception>
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

            /// <summary>
            /// Creates an instance of <see cref="MazeToMapOptions" /> with
            /// square wall and trail cell sizes.
            /// </summary>
            public static MazeToMapOptions SquareCells(
                int trailCellSize,
                int wallCellSize)
                => new MazeToMapOptions(
                    new int[] { trailCellSize },
                    new int[] { trailCellSize },
                    new int[] { wallCellSize },
                    new int[] { wallCellSize }
                );

            /// <summary>
            /// Creates an instance of <see cref="MazeToMapOptions" /> with
            /// rectangular wall and trail cell sizes.
            /// </summary>
            public static MazeToMapOptions RectCells(
                Vector trailCellSize,
                Vector wallCellSize)
                => new MazeToMapOptions(
                    new int[] { trailCellSize.X },
                    new int[] { trailCellSize.Y },
                    new int[] { wallCellSize.X },
                    new int[] { wallCellSize.Y }
                );

            /// <summary>
            /// Creates an instance of <see cref="MazeToMapOptions" /> with
            /// rectangular wall and trail cell sizes.
            /// </summary>
            public static MazeToMapOptions RectCells(
                int cellWidth,
                int cellHeight)
                => new MazeToMapOptions(
                    new int[] { cellWidth },
                    new int[] { cellHeight },
                    new int[] { cellWidth },
                    new int[] { cellHeight }
                );

            internal void ThrowIfWrong(Vector mazeSize) {
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