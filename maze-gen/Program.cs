using System;
using Nour.Play.Renderers;

namespace Nour.Play.Maze {
    class MainClass {
        public static void Main(string[] args) {
            var size = new Vector(20, 20);
            // As a user, how can I generate a maze?
            // TODO: Rename namespaces
            // TODO: Organize tests by namespaces
            // TODO: Check public symbols
            // TODO: Create an extendible generator class
            // TODO: Rename core to src
            // TODO: Warning if missing comment on a public symbol
            // TODO: StyleCop or something
            // TODO: Remove maze size option from MazeToMapOptions
            var map = MazeGenerator.Generate(size,
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.RecursiveBacktracker,
                    FillFactor = GeneratorOptions.FillFactorOption.ThreeQuarters,
                    MapAreasOptions = GeneratorOptions.MapAreaOptions.Manual,
                }).ToMap(Maze.Maze2DRenderer.MazeToMapOptions.SquareCells(1, 1));
            Console.WriteLine(map);
        }
    }
}
