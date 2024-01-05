using System;
using PlayersWorlds.Maps.Renderers;

namespace PlayersWorlds.Maps.Maze {
    class MainClass {
        public static void Main() {
            var size = new Vector(10, 10);
            // As a user, how do I generate a maze?
            var map = MazeGenerator.Generate(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.RecursiveBacktracker,
                    FillFactor = GeneratorOptions.FillFactorOption.ThreeQuarters,
                    MapAreasOptions = GeneratorOptions.MapAreaOptions.Auto,
                }).ToMap(Maze.Maze2DRenderer.MazeToMapOptions.RectCells(new Vector(2, 1), new Vector(2, 1)));
            Console.WriteLine(map);
        }
    }
}
