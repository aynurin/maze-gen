using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Areas.Evolving;
using PlayersWorlds.Maps.Maze.PostProcessing;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Base class for all maze generator algorithms. Also contains helper
    /// methods for maze generation.
    /// </summary>
    public abstract class MazeGenerator {
        /// <summary>
        /// When implemented in a derived class, generates a new maze.
        /// </summary>
        /// <param name="builder"><see cref="Maze2DBuilder" /> instance for
        /// the maze to be generated.</param>
        public abstract void GenerateMaze(Maze2DBuilder builder);

        /// <summary>
        /// A helper method to generate a new maze.
        /// </summary>
        /// <param name="size">Size of the maze to generate.</param>
        /// <param name="options">Maze generator options.</param>
        /// <returns>A generated maze</returns>
        /// <exception cref="ArgumentNullException">Maze generation algorithm
        /// is not specified. See <see cref="GeneratorOptions.Algorithms" />.</exception>
        /// <exception cref="ArgumentException">The provided maze generator type
        /// is not inherited from <see cref="MazeGenerator" /> or does not
        /// provide a default constructor.</exception>
        public static Maze2D Generate(Vector size, GeneratorOptions options) =>
            Generate(size, options, out var _);

        internal static Maze2D Generate(Vector size, GeneratorOptions options,
            out Maze2DBuilder builder) {
            if (options.Algorithm == null) {
                throw new ArgumentNullException(
                    "Please specify maze generation algorithm using " +
                    "GeneratorOptions.Algorithm or a generic parameter.");
            }
            if (!typeof(MazeGenerator).IsAssignableFrom(options.Algorithm)) {
                throw new ArgumentException(
                    "Specified maze generation algorithm " +
                    $"({options.Algorithm.FullName}) is not a subtype of " +
                    "MazeGenerator.");
            }
            if (options.Algorithm.GetConstructor(Type.EmptyTypes) == null) {
                throw new ArgumentException(
                    "Specified maze generation algorithm " +
                    $"({options.Algorithm.FullName}) does not have a default " +
                    "constructor.");
            }

            var maze = new Maze2D(size);
            try {
                Console.WriteLine($"{options.Algorithm.Name}: Generating maze " +
                    $"{maze.Size} with {options.ShortDebugString()}");  /*  */
                GenerateMazeAreas(size, options, maze);
                builder = new Maze2DBuilder(maze, options);
                (Activator.CreateInstance(options.Algorithm) as MazeGenerator)
                    .GenerateMaze(builder);
                builder.ConnectHalls();
                maze.ApplyAreas();
                maze.Attributes.Set(DeadEnd.DeadEndAttribute, DeadEnd.Find(maze));
                maze.Attributes.Set(DijkstraDistance.LongestTrailAttribute,
                    DijkstraDistance.FindLongestTrail(maze));
                return maze;
            } catch (Exception ex) {
                throw new MazeGenerationException(maze, ex);
            }
        }

        private static void GenerateMazeAreas(Vector mazeSize,
                                              GeneratorOptions options,
                                              Maze2D maze) {
            // count existing (desired) placement errors we can ignore when
            // checking auto-generated areas.
            var existingErrors = options.MapAreas == null ? 0 :
                options.MapAreas.Count(
                    area => area.IsPositionFixed &&
                            options.MapAreas.Any(other =>
                                area != other &&
                                other.IsPositionFixed &&
                                area.Overlaps(other))) +
                options.MapAreas.Count(
                    area => area.IsPositionFixed &&
                            !area.FitsInto(Vector.Zero2D, mazeSize));
            // when we auto-generate the areas, there is a 5% chance that we
            // can't auto-distribute (see SideToSideForceProducer.cs) so we make
            // several attempts.
            var attempts = options.MapAreasOptions ==
                    GeneratorOptions.MapAreaOptions.Auto ? 3 : 1;
            while (true) {
                var areas = new List<MapArea>(
                    options.MapAreas ?? new List<MapArea>());
                if (options.MapAreasOptions ==
                    GeneratorOptions.MapAreaOptions.Auto) {
                    areas.AddRange(
                        options.AreaGenerator.Generate(mazeSize, areas));
                }
                new AreaDistributor()
                    .Distribute(mazeSize, areas, 100);
                var errors = -existingErrors +
                    areas.Count(
                        area => areas.Any(other =>
                                    area != other &&
                                    area.Overlap(other).Area > 0)) +
                    areas.Count(
                        area => !area.FitsInto(Vector.Zero2D, mazeSize));
                if (errors <= 0) {
                    foreach (var area in areas) {
                        maze.AddArea(area);
                    }
                    break;
                }
                if (--attempts == 0) {
                    var roomsDebugStr =
                        areas.Select(a => $"P{a.Position};S{a.Size}");
                    var message =
                        $"Could not generate rooms for maze of size {mazeSize}. " +
                        $"Last set of rooms had {errors} errors " +
                        $"({string.Join(" ", roomsDebugStr)}).";
                    var impact = areas.Select(area =>
                                        areas.Where(other => area != other)
                                             .Sum(other =>
                                                area.Overlap(other).Area))
                                      .Sum() / 2;
                    if (impact > mazeSize.Area / 10) {
                        throw new InvalidOperationException(message);
                    } else {
                        Trace.TraceWarning(message);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the maze generation is complete given the
        /// <paramref name="options" />.
        /// </summary>
        protected bool IsFillComplete(GeneratorOptions options, Maze2D maze) {
            var visitedCells = maze.VisitedCells.ToList();
            if (visitedCells.Count == 0) {
                return false;
            }
            if (options.FillFactor == GeneratorOptions.FillFactorOption.FullHeight) {
                var minX = visitedCells.Min(c => c.X);
                var maxX = visitedCells.Max(c => c.X);
                return minX == 0 && maxX == maze.Size.X - 1;
            } else if (options.FillFactor == GeneratorOptions.FillFactorOption.FullWidth) {
                var minY = visitedCells.Min(c => c.Y);
                var maxY = visitedCells.Max(c => c.Y);
                return minY == 0 && maxY == maze.Size.Y - 1;
            } else if (options.FillFactor == GeneratorOptions.FillFactorOption.Full) {
                return visitedCells.Count == maze.UnlinkedCells.Count;
            } else {
                var fillFactor =
                    options.FillFactor == GeneratorOptions.FillFactorOption.Quarter ? 0.25 :
                    options.FillFactor == GeneratorOptions.FillFactorOption.Half ? 0.5 :
                    options.FillFactor == GeneratorOptions.FillFactorOption.ThreeQuarters ? 0.75 :
                    0.9;
                return visitedCells.Count >= maze.UnlinkedCells.Count * fillFactor;
            }
        }
    }
}