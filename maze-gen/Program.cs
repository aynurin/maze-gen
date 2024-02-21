using System;
using System.Linq;
using System.Reflection;
using PlayersWorlds.Maps.Areas;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps.Maze {
    class MainClass {
        public static void Main(string[] args) {
            var verb = Enum.Parse(typeof(Verb), args[0], true);
            switch (verb) {
                case Verb.Generate: {
                        var size = args.Length > 3 ? Vector.Parse(args[3]) : new Vector(10, 10);
                        var options = new GeneratorOptions() {
                            Algorithm = args.Length > 2 ? Algorithm(args[2]) :
                                GeneratorOptions.Algorithms.AldousBroder,
                            FillFactor = GeneratorOptions.FillFactorOption.Full,
                            MapAreasOptions = GeneratorOptions.MapAreaOptions.Auto,
                        };
                        var maze = MazeGenerator.Generate(size, options);
                        Console.WriteLine(maze.Serialize());
                        Console.WriteLine(maze.ToString());
                        Console.WriteLine(maze.ToMap(MazeToMapOptions.RectCells(2, 1)).ToString());
                        break;
                    }

                case Verb.Parse: {
                        var maze = Maze2D.Parse(args[1]);
                        Console.WriteLine(maze.Serialize());
                        Console.WriteLine(maze.ToString());
                        Console.WriteLine($"Visited: " +
                            maze.VisitedCells.Count());
                        Console.WriteLine($"Total Cells: " +
                            maze.AllCells.Count());
                        Console.WriteLine($"Area Cells: ");
                        Console.WriteLine("  Fill ({0}): ({1})",
                            maze.MapAreas.Count(
                                            a => a.Key.Type == AreaType.Fill),
                            maze.MapAreas.Where(
                                            a => a.Key.Type == AreaType.Fill)
                                         .Select(a => a.Value.Count).Sum());
                        Console.WriteLine("  Cave ({0}): ({1}): ",
                            maze.MapAreas.Count(
                                            a => a.Key.Type == AreaType.Cave),
                            maze.MapAreas.Where(
                                            a => a.Key.Type == AreaType.Cave)
                                         .Select(a => a.Value.Count).Sum());
                        Console.WriteLine("  Hall ({0}): ({1}): ",
                            maze.MapAreas.Count(
                                            a => a.Key.Type == AreaType.Hall),
                            maze.MapAreas.Where(
                                            a => a.Key.Type == AreaType.Hall)
                                         .Select(a => a.Value.Count).Sum());
                        Console.WriteLine("Unvisited cells: " +
                            string.Join(",",
                                maze.AllCells
                                    .Where(c =>
                                        !c.IsVisited &&
                                        !maze.MapAreas.Any(
                                            area => area.Value.Contains(c)))));
                        break;
                    }

                case Verb.Run: {
                        Run(args[1], args.Length > 2 ? int.Parse(args[2]) : 1);
                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public static void Run(string path, int times) {
            var parts = path.Split('.').ToArray();
            var methodName = parts.Last();
            var typeName = string.Join(".", parts.Take(parts.Length - 1));
            var type = Type.GetType(typeName + ", PlayersWorlds.Maps.Tests");
            var testObj = Activator.CreateInstance(type);
            Console.WriteLine($"Creating obj: " + testObj?.GetType().FullName);
            var method = type.GetMethod(methodName);
            for (var i = 0; i < times; i++) {
                Console.WriteLine(method + ": " + i);
                method.Invoke(testObj, null);
            }
        }

        private static Type Algorithm(string v) {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm.GetName().Name == "PlayersWorlds.Maps")
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(MazeGenerator) != p &&
                            typeof(MazeGenerator).IsAssignableFrom(p))
                .Where(p => p.Name == v + "MazeGenerator")
                .First();
        }

        private enum Verb {
            /// <summary>
            /// Parse a maze from a string
            /// </summary>
            Parse = 1,
            /// <summary>
            /// Generate a random maze with the specified algorithm and size.
            /// </summary>
            Generate = 2,
            /// <summary>
            /// Run a specific test N times
            /// </summary>
            Run = 3
        }
    }
}
