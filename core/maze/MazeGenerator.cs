using System;
using System.Collections.Generic;
using System.Linq;
using Nour.Play.Maze.PostProcessing;

namespace Nour.Play.Maze {
    public abstract class MazeGenerator {
        public abstract void GenerateMaze(Maze2D map, GeneratorOptions options);

        public static Maze2D Generate<T>(Vector size, GeneratorOptions options)
            where T : MazeGenerator, new() {
            var maze = new Maze2D(size);
            // TODO: Add rooms here.
            new T().GenerateMaze(maze, options);
            maze.Attributes.Set(DeadEnd.DeadEndAttribute, DeadEnd.Find(maze));
            maze.Attributes.Set(DijkstraDistance.LongestTrailAttribute,
                DijkstraDistance.FindLongestTrail(maze));
            return maze;
        }

        protected bool IsFillComplete(GeneratorOptions options, Maze2D map) =>
            IsFillComplete(options, map.VisitedCells.ToList(), map.Size);

        protected bool IsFillComplete(GeneratorOptions options, ICollection<MazeCell> visitedCells, Vector mazeSize) {
            if (options.FillFactor == GeneratorOptions.FillFactorOption.FullHeight) {
                var minX = visitedCells.Min(c => c.X);
                var maxX = visitedCells.Max(c => c.X);
                return minX == 0 && maxX == mazeSize.X - 1;
            } else if (options.FillFactor == GeneratorOptions.FillFactorOption.FullWidth) {
                var minY = visitedCells.Min(c => c.Y);
                var maxY = visitedCells.Max(c => c.Y);
                return minY == 0 && maxY == mazeSize.Y - 1;
            } else if (options.FillFactor == GeneratorOptions.FillFactorOption.Full) {
                return visitedCells.Count == mazeSize.Area;
            } else {
                var fillFactor =
                    options.FillFactor == GeneratorOptions.FillFactorOption.Quarter ? 0.25 :
                    options.FillFactor == GeneratorOptions.FillFactorOption.Half ? 0.5 :
                    options.FillFactor == GeneratorOptions.FillFactorOption.ThreeQuarters ? 0.75 :
                    0.9;
                return visitedCells.Count >= mazeSize.Area * fillFactor;
            }
        }
    }
}