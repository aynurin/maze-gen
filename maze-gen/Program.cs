using System;
using PlayersWorlds.Maps.Renderers;

namespace PlayersWorlds.Maps.Maze {
    class MainClass {
        public static void Main() {
            var size = new Vector(20, 20);
            // As a user, how can I generate a maze?
            // TODO: Create a readme with examples
            // TODO: Check public symbols
            // TODO: Check if all classes have tests
            // TODO: Create an extendible generator class
            // TODO: Warning if missing comment on a public symbol
            // TODO: StyleCop or something
            // TODO: Remove maze size option from MazeToMapOptions
            // TODO: Add coverage report to git
            // TODO: When picking a random cell in maze generators, see if there
            //       are any unvisited visitable areas.
            var map = MazeGenerator.Generate(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.RecursiveBacktracker,
                    FillFactor = GeneratorOptions.FillFactorOption.ThreeQuarters,
                    MapAreasOptions = GeneratorOptions.MapAreaOptions.Auto,
                }).ToMap(Maze.Maze2DRenderer.MazeToMapOptions.SquareCells(1, 1));
            Console.WriteLine(map);
        }
    }
}
