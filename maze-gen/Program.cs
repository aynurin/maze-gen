using System;
using Nour.Play.Renderers;

namespace Nour.Play.Maze {
    class MainClass {
        public static void Main(string[] args) {
            var alsoDrawAscii = args[0] == "ascii";
            Generate<AldousBroderMazeGenerator>(new Vector(4, 8), alsoDrawAscii, true, GeneratorOptions.FillFactorOption.ThreeQuarters);
            Generate<WilsonsMazeGenerator>(new Vector(4, 8), alsoDrawAscii, true, GeneratorOptions.FillFactorOption.ThreeQuarters);
        }

        private static void Generate<T>(Vector size, bool drawAscii, bool drawAsciiBox, GeneratorOptions.FillFactorOption fillFactor)
        where T : MazeGenerator, new() {
            var maze = MazeGenerator.Generate<T>(size,
                new GeneratorOptions() { FillFactor = fillFactor });
            var map = maze.ToMap(Maze2DToMap2DConverter.MazeToMapOptions.Custom(1, 2, 1, 2, maze.Size));
            if (drawAscii) {
                Console.WriteLine(new Map2DAsciiRenderer().Render(map));
            }
            if (drawAsciiBox) {
                Console.WriteLine(new Maze2DAsciiBoxRenderer(maze).WithTrail());
            }
        }
    }
}
