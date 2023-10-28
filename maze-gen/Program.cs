using System;

namespace Nour.Play.Maze {
    class MainClass {
        public static void Main(string[] args) {
            var maze = MazeGenerator.Generate<WilsonsMazeGenerator>(
                new Vector(4, 4), new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Half
                });
            if (args[0] == "ascii") {
                Console.WriteLine(maze.ToMap(Maze2DToMap2DConverter.MazeToMapOptions.Custom(1, 2, 1, 2, maze.Size)));
            } else {
                Console.WriteLine(maze);
            }
        }
    }
}
