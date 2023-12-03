using System;
using System.Collections.Generic;

namespace Nour.Play.Maze {
    public class AldousBroderMazeGenerator : MazeGenerator {
        override public void GenerateMaze(Maze2D layout, GeneratorOptions options) {
            Console.WriteLine("AldousBroderMazeGenerator v0.1");
            Console.WriteLine($"Generating maze {layout.XWidthColumns}x{layout.YHeightRows}");

            var currentCell = layout.Cells.GetRandom();
            var visitedCells = new HashSet<MazeCell>() { currentCell };
            while (!IsFillComplete(options, visitedCells, layout.Size)) {
                var next = currentCell.Neighbors().GetRandom();
                if (!visitedCells.Contains(next)) {
                    currentCell.Link(next);
                    visitedCells.Add(next);
                }
                currentCell = next;
            }
        }
    }
}