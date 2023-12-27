using System;
using System.Collections.Generic;

namespace Nour.Play.Maze {
    public class WilsonsMazeGenerator : MazeGenerator {
        override public void GenerateMaze(Maze2D layout, GeneratorOptions options) {
            var currentCell = layout.VisitableCells.GetRandom();
            var visitedCells = new HashSet<MazeCell>() { currentCell };

            while (!IsFillComplete(options, layout)) {
                var walkPath = new List<MazeCell>();

                var nextCell = layout.VisitableCells.GetRandom();

                if (visitedCells.Contains(nextCell))
                    continue;

                while (!visitedCells.Contains(nextCell)) {
                    var containsAt = walkPath.IndexOf(nextCell);
                    if (containsAt >= 0)
                        walkPath.RemoveRange(containsAt + 1, walkPath.Count - containsAt - 1);
                    else walkPath.Add(nextCell);
                    nextCell = nextCell.Neighbors().GetRandom();
                };

                walkPath.Add(nextCell);

                for (var i = 0; i < walkPath.Count - 1; i++) {
                    walkPath[i].Link(walkPath[i + 1]);
                    visitedCells.Add(walkPath[i]);
                }
            }
        }
    }
}