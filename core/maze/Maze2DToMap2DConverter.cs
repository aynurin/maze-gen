using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using Nour.Play.Renderers;

namespace Nour.Play.Maze {
    public class Maze2DToMap2DConverter {
        public const string MAP2D_CELL_TYPE_WALL = "MAZE2D_WALL";
        public const string MAP2D_CELL_TYPE_TRAIL = "MAZE2D_TRAIL";
        public const string MAP2D_CELL_TYPE_EDGE = "MAZE2D_EDGE";
        public const string MAP2D_CELL_TYPE_VOID = "MAZE2D_VOID";

        public Map2D Convert(Maze2D maze, MazeToMapOptions options) {
            options.ThrowIfNull("maze");
            options.ThrowIfNull("options");
            options.ThrowIfWrong(maze);

            var mapSize = new Vector(options.TrailXHeights.Sum(),
                                     options.TrailYWidths.Sum()) +
                          new Vector(options.WallXHeights.Sum(),
                                     options.WallYWidths.Sum());
            var map = new Map2D(mapSize);
            for (int i = 0; i < maze.Cells.Count; i++) {
                var mazeCell = maze.Cells[i];
                var scaledX = options.TrailXHeights.Where(
                                    (a, ai) => ai < mazeCell.X).Sum() +
                              options.WallXHeights.Where(
                                    (a, ai) => ai <= mazeCell.X).Sum();
                var scaledY = options.TrailYWidths.Where(
                                    (a, ai) => ai < mazeCell.Y).Sum() +
                              options.WallYWidths.Where(
                                    (a, ai) => ai <= mazeCell.Y).Sum();
                if (mazeCell.IsVisited) {
                    map.CellsAt(new Vector(scaledX, scaledY),
                                   new Vector(
                                        options.TrailXHeights[mazeCell.X],
                                        options.TrailYWidths[mazeCell.Y]))
                          .ForEach(c => c.Tags.Add(MAP2D_CELL_TYPE_TRAIL));

                    if (!mazeCell.Neighbors(Vector.East2D).HasValue) {
                        map.CellsAt(new Vector(scaledX - options.WallXHeights[mazeCell.X],
                                                scaledY + options.TrailYWidths[mazeCell.Y]),
                                    new Vector(options.TrailXHeights[mazeCell.X] + options.WallXHeights[mazeCell.X + 1] + options.WallXHeights[mazeCell.X],
                                                options.WallYWidths[mazeCell.Y + 1]))
                            .ForEach(c => c.Tags.Add(MAP2D_CELL_TYPE_WALL));
                    }
                    if (!mazeCell.Neighbors(Vector.West2D).HasValue) {
                        map.CellsAt(new Vector(scaledX - options.WallXHeights[mazeCell.X],
                                                scaledY - options.WallYWidths[mazeCell.Y]),
                                    new Vector(options.TrailXHeights[mazeCell.X] + options.WallXHeights[mazeCell.X + 1] + options.WallXHeights[mazeCell.X],
                                                options.WallYWidths[mazeCell.Y]))
                            .ForEach(c => c.Tags.Add(MAP2D_CELL_TYPE_WALL));
                    }
                    if (!mazeCell.Neighbors(Vector.South2D).HasValue) {
                        map.CellsAt(new Vector(scaledX + options.TrailXHeights[mazeCell.X],
                                                scaledY - options.WallYWidths[mazeCell.Y]),
                                    new Vector(options.WallXHeights[mazeCell.X + 1],
                                                options.TrailYWidths[mazeCell.Y] + options.WallYWidths[mazeCell.Y + 1] + options.WallYWidths[mazeCell.Y]))
                            .ForEach(c => c.Tags.Add(MAP2D_CELL_TYPE_WALL));
                    }
                    if (!mazeCell.Neighbors(Vector.North2D).HasValue) {
                        map.CellsAt(new Vector(scaledX - options.WallXHeights[mazeCell.X],
                                                scaledY - options.WallYWidths[mazeCell.Y]),
                                    new Vector(options.WallXHeights[mazeCell.X],
                                                options.TrailYWidths[mazeCell.Y] + options.WallYWidths[mazeCell.Y + 1] + options.WallYWidths[mazeCell.Y]))
                            .ForEach(c => c.Tags.Add(MAP2D_CELL_TYPE_WALL));
                    }
                } else {
                    var cellType = MAP2D_CELL_TYPE_VOID;
                    if (mazeCell.Attributes.ContainsKey("MAP2D_CELL_TYPE")) {
                        cellType = mazeCell.Attributes["MAP2D_CELL_TYPE"];
                    }
                    map.CellsAt(new Vector(scaledX, scaledY),
                                   new Vector(
                                        options.TrailXHeights[mazeCell.X],
                                        options.TrailYWidths[mazeCell.Y]))
                          .ForEach(c => c.Tags.Add(cellType));
                    if (!mazeCell.Neighbors(Vector.East2D).HasValue) {
                        map.CellsAt(new Vector(scaledX - options.WallXHeights[mazeCell.X],
                                                scaledY + options.TrailYWidths[mazeCell.Y]),
                                    new Vector(options.TrailXHeights[mazeCell.X] + options.WallXHeights[mazeCell.X + 1] + options.WallXHeights[mazeCell.X],
                                                options.WallYWidths[mazeCell.Y + 1]))
                            .ForEach(c => c.Tags.Add(MAP2D_CELL_TYPE_VOID));
                    }
                    if (!mazeCell.Neighbors(Vector.West2D).HasValue) {
                        map.CellsAt(new Vector(scaledX - options.WallXHeights[mazeCell.X],
                                                scaledY - options.WallYWidths[mazeCell.Y]),
                                    new Vector(options.TrailXHeights[mazeCell.X] + options.WallXHeights[mazeCell.X + 1] + options.WallXHeights[mazeCell.X],
                                                options.WallYWidths[mazeCell.Y]))
                            .ForEach(c => c.Tags.Add(MAP2D_CELL_TYPE_VOID));
                    }
                    if (!mazeCell.Neighbors(Vector.South2D).HasValue) {
                        map.CellsAt(new Vector(scaledX + options.TrailXHeights[mazeCell.X],
                                                scaledY - options.WallYWidths[mazeCell.Y]),
                                    new Vector(options.WallXHeights[mazeCell.X + 1],
                                                options.TrailYWidths[mazeCell.Y] + options.WallYWidths[mazeCell.Y + 1] + options.WallYWidths[mazeCell.Y]))
                            .ForEach(c => c.Tags.Add(MAP2D_CELL_TYPE_VOID));
                    }
                    if (!mazeCell.Neighbors(Vector.North2D).HasValue) {
                        map.CellsAt(new Vector(scaledX - options.WallXHeights[mazeCell.X],
                                                scaledY - options.WallYWidths[mazeCell.Y]),
                                    new Vector(options.WallXHeights[mazeCell.X],
                                                options.TrailYWidths[mazeCell.Y] + options.WallYWidths[mazeCell.Y + 1] + options.WallYWidths[mazeCell.Y]))
                            .ForEach(c => c.Tags.Add(MAP2D_CELL_TYPE_VOID));
                    }
                }
                if (mazeCell.Neighbors(Vector.East2D).HasValue) {
                    map.CellsAt(new Vector(scaledX,
                                              scaledY + options.TrailYWidths[mazeCell.Y]),
                                   new Vector(options.TrailXHeights[mazeCell.X],
                                              options.WallYWidths[mazeCell.Y + 1]))
                          .ForEach(c => c.Tags.Add(
                                   mazeCell.Links(Vector.East2D).HasValue ?
                                        MAP2D_CELL_TYPE_TRAIL :
                                   mazeCell.IsVisited || (mazeCell.Neighbors(Vector.East2D).HasValue && mazeCell.Neighbors(Vector.East2D).Value.IsVisited) ?
                                        MAP2D_CELL_TYPE_WALL :
                                        MAP2D_CELL_TYPE_VOID));
                }
                if (mazeCell.Neighbors(Vector.South2D).HasValue) {
                    map.CellsAt(new Vector(scaledX + options.TrailXHeights[mazeCell.X],
                                              scaledY),
                                   new Vector(options.WallXHeights[mazeCell.X + 1],
                                              options.TrailYWidths[mazeCell.Y]))
                          .ForEach(c => c.Tags.Add(
                                   mazeCell.Links(Vector.South2D).HasValue ?
                                        MAP2D_CELL_TYPE_TRAIL :
                                   mazeCell.IsVisited || (mazeCell.Neighbors(Vector.South2D).HasValue && mazeCell.Neighbors(Vector.South2D).Value.IsVisited) ?
                                        MAP2D_CELL_TYPE_WALL :
                                        MAP2D_CELL_TYPE_VOID));
                }
                if (mazeCell.Neighbors(Vector.East2D).HasValue &&
                    mazeCell.Neighbors(Vector.South2D).HasValue) {
                    var diagonalCell = mazeCell.Neighbors(Vector.East2D).Value.Neighbors(Vector.South2D);
                    var cellType = MAP2D_CELL_TYPE_WALL;
                    if (mazeCell.Links(Vector.East2D).HasValue &&
                        mazeCell.Links(Vector.South2D).HasValue &&
                        mazeCell.Links(Vector.East2D).Value.Links(Vector.South2D).HasValue &&
                        mazeCell.Links(Vector.South2D).Value.Links(Vector.East2D).HasValue) {
                        cellType = MAP2D_CELL_TYPE_TRAIL;
                    } else if (new bool[] {
                                    mazeCell.Links(Vector.East2D).HasValue,
                                    mazeCell.Links(Vector.East2D).HasValue && mazeCell.Links(Vector.East2D).Value.Links(Vector.South2D).HasValue,
                                    mazeCell.Links(Vector.South2D).HasValue,
                                    mazeCell.Links(Vector.South2D).HasValue && mazeCell.Links(Vector.South2D).Value.Links(Vector.East2D).HasValue,
                               }.Count(b => b) >= 2) {
                        cellType = MAP2D_CELL_TYPE_EDGE;
                    } else if (mazeCell.Neighbors(Vector.East2D).Value.Links(Vector.South2D).HasValue &&
                               mazeCell.Neighbors(Vector.South2D).Value.Links(Vector.East2D).HasValue) {
                        cellType = MAP2D_CELL_TYPE_EDGE;
                    } else if (!mazeCell.Neighbors(Vector.East2D).Value.IsVisited &&
                               !mazeCell.Neighbors(Vector.South2D).Value.IsVisited &&
                               !mazeCell.Neighbors(Vector.East2D).Value.Neighbors(Vector.South2D).Value.IsVisited) {
                        cellType = mazeCell.IsVisited ? MAP2D_CELL_TYPE_WALL : MAP2D_CELL_TYPE_VOID;
                    }
                    if (cellType == MAP2D_CELL_TYPE_EDGE) {
                        if (!mazeCell.IsVisited) {
                            map.CellsAt(new Vector(scaledX, scaledY),
                                           new Vector(
                                                options.TrailXHeights[mazeCell.X],
                                                options.TrailYWidths[mazeCell.Y]))
                                  .ForEach(c => c.Tags.Add(MAP2D_CELL_TYPE_WALL));
                        }
                        if (mazeCell.Neighbors(Vector.East2D).HasValue &&
                           !mazeCell.Neighbors(Vector.East2D).Value.IsVisited) {
                            mazeCell.Neighbors(Vector.East2D).Value.Attributes.Set("MAP2D_CELL_TYPE", MAP2D_CELL_TYPE_WALL);
                        }
                        if (mazeCell.Neighbors(Vector.South2D).HasValue &&
                           !mazeCell.Neighbors(Vector.South2D).Value.IsVisited) {
                            mazeCell.Neighbors(Vector.South2D).Value.Attributes.Set("MAP2D_CELL_TYPE", MAP2D_CELL_TYPE_WALL);
                        }
                        if (mazeCell.Neighbors(Vector.South2D).HasValue &&
                            mazeCell.Neighbors(Vector.South2D).Value.Neighbors(Vector.East2D).HasValue &&
                           !mazeCell.Neighbors(Vector.South2D).Value.Neighbors(Vector.East2D).Value.IsVisited) {
                            mazeCell.Neighbors(Vector.South2D).Value.Neighbors(Vector.East2D).Value.Attributes.Set("MAP2D_CELL_TYPE", MAP2D_CELL_TYPE_WALL);
                        }
                    }
                    map.CellsAt(new Vector(scaledX + options.TrailXHeights[mazeCell.X],
                                              scaledY + options.TrailYWidths[mazeCell.Y]),
                                   new Vector(options.WallXHeights[mazeCell.X + 1],
                                              options.WallYWidths[mazeCell.Y + 1]))
                          .ForEach(c => c.Tags.Add(cellType));
                }
            }
            return map;
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

            // option 2: custom sizes
            public static MazeToMapOptions Custom(
                int allTrailWidths,
                int allWallWidths,
                Vector mazeSize)
                => new MazeToMapOptions(
                    /* trailXHeights = */ Enumerable.Repeat(allTrailWidths, mazeSize.X).ToArray(),
                    /* trailYWidths = */ Enumerable.Repeat(allTrailWidths, mazeSize.Y).ToArray(),
                    /* wallXHeights = */ Enumerable.Repeat(allTrailWidths, Math.Max(mazeSize.X + 1, 0)).ToArray(),
                    /* wallYWidths = */ Enumerable.Repeat(allTrailWidths, Math.Max(mazeSize.Y + 1, 0)).ToArray());

            // option 2: custom sizes
            public static MazeToMapOptions Custom(
                int allTrailXHeights,
                int allTrailYWidths,
                int allWallXHeights,
                int allWallYWidths,
                Vector mazeSize)
                => new MazeToMapOptions(
                    /* trailXHeights = */ Enumerable.Repeat(allTrailXHeights, mazeSize.X).ToArray(),
                    /* trailYWidths = */ Enumerable.Repeat(allTrailYWidths, mazeSize.Y).ToArray(),
                    /* wallXHeights = */ Enumerable.Repeat(allWallXHeights, Math.Max(mazeSize.X + 1, 0)).ToArray(),
                    /* wallYWidths = */ Enumerable.Repeat(allWallYWidths, Math.Max(mazeSize.Y + 1, 0)).ToArray());

            public MazeToMapOptions(
                int[] trailXHeights,
                int[] trailYWidths,
                int[] wallXHeights,
                int[] wallYWidths) {
                trailXHeights.ThrowIfNull("trailXHeights");
                trailYWidths.ThrowIfNull("trailYWidths");
                wallXHeights.ThrowIfNull("wallXHeights");
                wallYWidths.ThrowIfNull("wallYWidths");
                if (trailXHeights.Any(i => i <= 0) ||
                    trailYWidths.Any(i => i <= 0) ||
                    wallXHeights.Any(i => i <= 0) ||
                    wallYWidths.Any(i => i <= 0)) {
                    throw new ArgumentException("Zero and negative wall and " +
                        "trail widths are not supported.");
                }
                TrailXHeights = trailXHeights;
                TrailYWidths = trailYWidths;
                WallXHeights = wallXHeights;
                WallYWidths = wallYWidths;
            }

            public void ThrowIfWrong(Maze2D maze) {
                if (TrailXHeights.Length != maze.XHeightRows ||
                    TrailYWidths.Length != maze.YWidthColumns ||
                    WallXHeights.Length != maze.XHeightRows + 1 ||
                    WallYWidths.Length != maze.YWidthColumns + 1) {
                    throw new ArgumentException("The provided Walls and " +
                        "trails counts need to match maze size");
                }
            }
        }
    }
}