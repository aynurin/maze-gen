using System;
using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Areas.Evolving;
using PlayersWorlds.Maps.Maze.PostProcessing;

namespace PlayersWorlds.Maps.Maze {
    public abstract class MazeGenerator {
        public abstract void GenerateMaze(Maze2D maze, GeneratorOptions options);

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
            return (Maze2D)typeof(MazeGenerator)
                .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .First(m => m.Name == "Generate" && m.IsGenericMethodDefinition)
                .MakeGenericMethod(options.Algorithm)
                .Invoke(null, new object[] { size, options });
        }

        public static Maze2D Generate<T>(Vector size, GeneratorOptions options)
            where T : MazeGenerator, new() {
            var maze = new Maze2D(size);
            // Maze generation process:
            //  1. The user chooses the maze size and various options, such as
            //      the size of walls and corridors, maze fill factor, rooms
            //      density, how the rooms are connected (maze vs. straight vs
            //      elbow), maze generation algorithm. All options have defaults
            //      so the user can choose to set any of the options.
            //  2. Generate rooms
            //  3. Initialize maze graph with the rooms
            //  4. Generate the maze
            Console.WriteLine($"{typeof(T).Name}: Generating maze {maze.Size} with {options.ShortDebugString()}");  /*  */
            if (options.MapAreasOptions == GeneratorOptions.MapAreaOptions.Auto) {
                GenerateMazeAreas(size, options, maze);
            } else if (options.MapAreasOptions ==
                            GeneratorOptions.MapAreaOptions.Manual
                       && options.MapAreas != null) {
                foreach (var area in options.MapAreas) {
                    maze.AddArea(area);
                }
            }
            new T().GenerateMaze(maze, options);
            maze.Attributes.Set(DeadEnd.DeadEndAttribute, DeadEnd.Find(maze));
            maze.Attributes.Set(DijkstraDistance.LongestTrailAttribute,
                DijkstraDistance.FindLongestTrail(maze));
            return maze;
        }

        private static void GenerateMazeAreas(Vector mazeSize,
                                              GeneratorOptions options,
                                              Maze2D maze) {
            // there is a 5% chance that we can't auto-distribute the areas
            // (see SideToSideForceProducer.cs) so we make several attempts.
            var attempts = 3;
            while (true) {
                attempts--;
                var areas = options.AreaGenerator.Generate(mazeSize);
                new AreaDistributor()
                    .Distribute(mazeSize, areas, 100);
                var errors =
                    areas.Count(
                        a => areas.Any(b => a != b && a.Overlaps(b))) +
                    areas.Count(block => !block.Fits(Vector.Zero2D, mazeSize));
                if (errors == 0) {
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

        protected bool IsFillComplete(GeneratorOptions options, Maze2D maze) =>
            IsFillComplete(options, maze.VisitedCells.ToList(), maze.VisitableCells, maze.Size);

        //                 instead of one cell space.
        protected bool IsFillComplete(GeneratorOptions options, ICollection<MazeCell> visitedCells, ICollection<MazeCell> visitableCells, Vector mazeSize) {
            if (visitedCells.Count == 0) {
                return false;
            }
            if (options.FillFactor == GeneratorOptions.FillFactorOption.FullHeight) {
                var minX = visitedCells.Min(c => c.X);
                var maxX = visitedCells.Max(c => c.X);
                return minX == 0 && maxX == mazeSize.X - 1;
            } else if (options.FillFactor == GeneratorOptions.FillFactorOption.FullWidth) {
                var minY = visitedCells.Min(c => c.Y);
                var maxY = visitedCells.Max(c => c.Y);
                return minY == 0 && maxY == mazeSize.Y - 1;
            } else if (options.FillFactor == GeneratorOptions.FillFactorOption.Full) {
                return visitedCells.Count == visitableCells.Count;
            } else {
                var fillFactor =
                    options.FillFactor == GeneratorOptions.FillFactorOption.Quarter ? 0.25 :
                    options.FillFactor == GeneratorOptions.FillFactorOption.Half ? 0.5 :
                    options.FillFactor == GeneratorOptions.FillFactorOption.ThreeQuarters ? 0.75 :
                    0.9;
                return visitedCells.Count >= visitableCells.Count * fillFactor;
            }
        }
    }
}