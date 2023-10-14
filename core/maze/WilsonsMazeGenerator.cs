using System;
using System.Collections.Generic;

namespace Nour.Play.Maze {
    public class WilsonsMazeGenerator : MazeGenerator {
        override public void GenerateMaze(Map2D layout) {
            Console.WriteLine("WilsonsMazeGenerator v0.1");
            Console.WriteLine($"Generating maze {layout.XHeightRows}x{layout.YWidthColumns}");

            var currentCell = layout.Cells.GetRandom();
            var visitedCells = new HashSet<Cell>() { currentCell };

            while (visitedCells.Count < layout.Cells.Count) {
                var walkPath = new List<Cell>();

                var nextCell = layout.Cells.GetRandom();

                if (visitedCells.Contains(nextCell))
                    continue;

                while (!visitedCells.Contains(nextCell)) {
                    walkPath.Add(nextCell);
                    var containsAt = walkPath.IndexOf(nextCell);
                    if (containsAt >= 0)
                        walkPath.RemoveRange(containsAt + 1, walkPath.Count - containsAt - 1);
                    nextCell = nextCell.Neighbors.GetRandom();
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