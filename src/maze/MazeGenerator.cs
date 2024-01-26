using System;
using System.Collections.Generic;
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
        /// <param name="maze">The maze layout to generate the maze in.</param>
        /// <param name="options">Maze generator options.</param>
        public abstract void GenerateMaze(Maze2D maze, GeneratorOptions options);

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
        public static Maze2D Generate(Vector size, GeneratorOptions options) {
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
            Console.WriteLine($"{options.Algorithm.Name}: Generating maze " +
                $"{maze.Size} with {options.ShortDebugString()}");  /*  */
            GenerateMazeAreas(size, options, maze);
            (Activator.CreateInstance(options.Algorithm) as MazeGenerator)
                .GenerateMaze(maze, options);
            maze.ApplyAreas();
            maze.Attributes.Set(DeadEnd.DeadEndAttribute, DeadEnd.Find(maze));
            maze.Attributes.Set(DijkstraDistance.LongestTrailAttribute,
                DijkstraDistance.FindLongestTrail(maze));
            return maze;
        }

        private static void GenerateMazeAreas(Vector mazeSize,
                                              GeneratorOptions options,
                                              Maze2D maze) {
            // when we auto-generate the areas, there is a 5% chance that we
            // can't auto-distribute (see SideToSideForceProducer.cs) so we make
            // several attempts.
            var attempts = options.MapAreasOptions ==
                    GeneratorOptions.MapAreaOptions.Auto ? 3 : 1;
            // count existing (desired) placement errors we can ignore when
            // checking auto-generated areas.
            var existingErrors = options.MapAreas == null ? 0 :
                options.MapAreas.Count(
                    a => a.IsPositionFixed &&
                         options.MapAreas.Any(b => a != b && a.Overlaps(b))) +
                options.MapAreas.Count(
                    block => !block.FitsInto(Vector.Zero2D, mazeSize));
            while (true) {
                attempts--;
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
                        a => areas.Any(b => a != b && a.Overlaps(b))) +
                    areas.Count(block => !block.FitsInto(Vector.Zero2D, mazeSize));
                if (errors <= 0) {
                    foreach (var area in areas) {
                        maze.AddArea(area);
                    }
                    break;
                }
                if (attempts == 0) {
                    var roomsDebugStr =
                        areas.Select(a => $"P{a.Position};S{a.Size}");
                    throw new MazeGenerationException(
                        $"Could not generate rooms for maze of size {mazeSize}. " +
                        $"Last set of rooms had {errors} errors " +
                        $"({string.Join(" ", roomsDebugStr)}).");
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