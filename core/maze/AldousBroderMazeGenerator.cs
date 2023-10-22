using System;
using System.Collections.Generic;

namespace Nour.Play.Maze {
    public class AldousBroderMazeGenerator : MazeGenerator {
        override public void GenerateMaze(Maze2D layout) {
            Console.WriteLine("AldousBroderMazeGenerator v0.1");
            Console.WriteLine($"Generating maze {layout.XHeightRows}x{layout.YWidthColumns}");

            var currentCell = layout.Cells.GetRandom();
            var visitedCells = new HashSet<MazeCell>() { currentCell };
            while (visitedCells.Count < layout.Area) {
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