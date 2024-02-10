using System;
using System.Linq;
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
                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
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
            Parse = 1,
            Generate = 2
        }
    }
}
