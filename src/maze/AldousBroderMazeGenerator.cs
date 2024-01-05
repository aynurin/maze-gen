using System;
using System.Collections.Generic;

namespace PlayersWorlds.Maps.Maze {
    public class AldousBroderMazeGenerator : MazeGenerator {
        override public void GenerateMaze(Maze2D layout, GeneratorOptions options) {
            var currentCell = layout.VisitableCells.GetRandom();
            var visitedCells = new HashSet<MazeCell>() { currentCell };
            while (!IsFillComplete(options, layout)) {
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