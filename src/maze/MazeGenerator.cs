using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Areas.Evolving;
using PlayersWorlds.Maps.Maze.PostProcessing;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Base class for all maze generator algorithms. Also contains helper
    /// methods for maze generation.
    /// </summary>
    public abstract class MazeGenerator {
        private static readonly Log s_log = Log.ToConsole<MazeGenerator>();
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
        // TODO: Generator generates on a given area, it does not create areas
        public static Area Generate(Vector size, GeneratorOptions options) =>
            Generate(size, options, out var _);

        internal static Area Generate(Vector size, GeneratorOptions options,
            out Maze2DBuilder builder) {
            if (options.RandomSource == null) {
                throw new ArgumentNullException(
                    "Please specify a RandomSource to use for maze " +
                    "generation using GeneratorOptions.RandomSource.");
            }
            s_log.D(1, $"{options.Algorithm?.Name} Generating a {size} maze " +
                        $"with seed {options.RandomSource?.Seed}.");
            if (options.MapAreasOptions == GeneratorOptions.MapAreaOptions.Auto
                && options.AreaGenerator == null) {
                throw new ArgumentNullException(
                    "Please specify an AreaGenerator to use for maze " +
                    "area generation using GeneratorOptions.AreaGenerator.");
            }
            if (options.Algorithm == null) {
                throw new ArgumentNullException(
                    "Please specify maze generation algorithm using " +
                    "GeneratorOptions.Algorithm.");
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

            var maze = Area.CreateEnvironment(size);
            try {
                GenerateMazeAreas(size, options).ForEach(area => maze.CreateChildArea(area));
                s_log.D(1, $"Generated {maze.ChildAreas.Count} maze areas");
                foreach (var area in maze.ChildAreas) {
                    s_log.D(1, "Area: " + area.ToString());
                }
                builder = new Maze2DBuilder(maze, options);
                (Activator.CreateInstance(options.Algorithm) as MazeGenerator)
                    .GenerateMaze(builder);
                builder.ApplyAreas();
                maze.X(DeadEnd.Find(builder));
                maze.X(DijkstraDistance.FindLongestTrail(builder));
                return maze;
            } catch (Exception ex) {
                throw new MazeGenerationException(maze, ex);
            }
        }

        private static List<Area> GenerateMazeAreas(
            Vector mazeSize,
            GeneratorOptions options) {
            // count existing (desired) placement errors we can ignore when
            // checking auto-generated areas.
            var existingErrors = options.MapAreas == null ? 0 :
                options.MapAreas.Count(
                    area => area.IsPositionFixed &&
                            options.MapAreas.Any(other =>
                                area != other &&
                                other.IsPositionFixed &&
                                area.OverlapArea(other).Area > 0)) +
                options.MapAreas.Count(
                    area => area.IsPositionFixed &&
                            !area.FitsInto(Vector.Zero2D, mazeSize));
            // when we auto-generate the areas, there is a <1% chance that we
            // can't auto-distribute (see SideToSideForceProducer.cs) so we make
            // several attempts.
            var attempts = options.MapAreasOptions ==
                    GeneratorOptions.MapAreaOptions.Auto ? 3 : 1;
            while (true) {
                s_log.D(5, 1000, "MazeGenerator.GenerateMazeAreas()");
                var areas = new List<Area>(
                    options.MapAreas ?? new List<Area>());
                s_log.D(1, string.Join(", ", areas.Select(a => a.ToString())));
                if (options.MapAreasOptions ==
                    GeneratorOptions.MapAreaOptions.Auto) {
                    areas.AddRange(
                        options.AreaGenerator.Generate(mazeSize, areas));
                }
                new AreaDistributor(options.RandomSource)
                    .Distribute(mazeSize, areas, 1);
                var errors = -existingErrors +
                    areas.Count(
                        area => areas.Any(other =>
                                    area != other &&
                                    area.OverlapArea(other).Area > 0)) +
                    areas.Count(
                        area => !area.FitsInto(Vector.Zero2D, mazeSize));
                s_log.D(1, string.Join(", ",
                    areas.Select(a => $"P{a.Position};S{a.Size}")));
                if (errors <= 0) {
                    return areas;
                }
                if (--attempts == 0) {
                    var roomsDebugStr = string.Join(", ",
                        areas.Select(a => $"P{a.Position};S{a.Size}"));
                    var message =
                        $"Could not generate rooms for maze of size {mazeSize}. " +
                        $"Last set of rooms had {errors} errors " +
                        $"({string.Join(" ", roomsDebugStr)}).";
                    var impact = areas.Select(area =>
                                        areas.Where(other => area != other)
                                             .Sum(other =>
                                                area.OverlapArea(other).Area))
                                      .Sum() / 2;
                    throw new InvalidOperationException(message);
                }
            }
        }
    }
}