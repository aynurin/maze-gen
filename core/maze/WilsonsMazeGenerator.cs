using System;
using System.Collections.Generic;

namespace Nour.Play.Maze {
    public class WilsonsMazeGenerator : MazeGenerator {
        override public void GenerateMaze(Maze2D layout, GeneratorOptions options) {
            Console.WriteLine("WilsonsMazeGenerator v0.1");
            Console.WriteLine($"Generating maze {layout.XWidthColumns}x{layout.YHeightRows}");

            var currentCell = layout.Cells.GetRandom();
            var visitedCells = new HashSet<MazeCell>() { currentCell };

            while (!IsFillComplete(options, visitedCells, layout.Size)) {
                var walkPath = new List<MazeCell>();

                var nextCell = layout.Cells.GetRandom();

                if (visitedCells.Contains(nextCell))
                    continue;

                while (!visitedCells.Contains(nextCell)) {
                    walkPath.Add(nextCell);
                    var containsAt = walkPath.IndexOf(nextCell);
                    if (containsAt >= 0)
                        walkPath.RemoveRange(containsAt + 1, walkPath.Count - containsAt - 1);
                    nextCell = nextCell.Neighbors().GetRandom();
                };

                walkPath.Add(nextCell);

                for (int i = 0; i < walkPath.Count - 1; i++) {
                    walkPath[i].Link(walkPath[i + 1]);
                    visitedCells.Add(walkPath[i]);
                }
            }
        }
    }
}