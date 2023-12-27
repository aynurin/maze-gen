using System;
using System.Collections.Generic;

namespace Nour.Play.Maze {
    public class AldousBroderMazeGenerator : MazeGenerator {
        override public void GenerateMaze(Maze2D layout, GeneratorOptions options) {
            var currentCell = layout.VisitableCells.GetRandom();
            var visitedCells = new HashSet<MazeCell>() { currentCell };
            while (!IsFillComplete(options, layout)) {
                // TODO (MapArea): If there are unvisited areas, start at one of them.
                // TODO (MapArea): Choose only visitable areas.
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