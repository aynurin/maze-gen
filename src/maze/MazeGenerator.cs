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

        // TODO: Remove the generic method as it's not used as generic anywhere.
        // https://github.com/spouliot/gendarme/wiki/Gendarme.Rules.Design.Generic.AvoidMethodWithUnusedGenericTypeRule(2.10)
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
            var attempts = 3;
            while (true) {
                attempts--;
                var areaGenerator = new RandomAreaGenerator(
                    options.AreaGeneratorSettings ??
                    RandomAreaGenerator.GeneratorSettings.Default);
                // we might have different strategies of identifying the number of
                // rooms. For now we'll just use 30% of maze area.
                var addedArea = 0;
                var roomGenerateAttempts = 10;
                var areas = new List<MapArea>();
                foreach (var area in areaGenerator) {
                    if (addedArea + area.Size.Area > maze.Size.Area * 0.3) {
                        if (roomGenerateAttempts-- <= 0) break;
                        continue;
                    }
                    area.Position = new Vector(
                        GlobalRandom.Next(0, maze.Size.X - area.Size.X),
                        GlobalRandom.Next(0, maze.Size.Y - area.Size.Y));
                    areas.Add(area);
                    addedArea += area.Size.Area;
                }

                if (areas.Count == 0) {
                    break;
                }
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

        // TODO (MapArea): if the cell belongs to an area, use Area space
        //                 instead of one cell space.
        protected bool IsFillComplete(GeneratorOptions options, ICollection<MazeCell> visitedCells, ICollection<MazeCell> visitableCells, Vector mazeSize) {
            var l = new Log("Maze2D", new Log.ImmediateFileLogWriter());
            l.I("IsFillComplete: " + options.FillFactor + ", visitedCells: " + visitedCells.Count + " visitableCells: " + visitableCells.Count + " mazeSize: " + mazeSize.Area);
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