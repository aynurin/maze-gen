using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Maze {
    public class HuntAndKillMazeGenerator : MazeGenerator {
        override public void GenerateMaze(Maze2D layout, GeneratorOptions options) {
            var currentCell = layout.VisitableCells.GetRandom();
            while (!IsFillComplete(options, layout)) {
                var potentiallyNext = currentCell.Neighbors().Where(cell => !cell.IsVisited).ToList();
                if (potentiallyNext.Count > 0) {
                    var nextCell = potentiallyNext.GetRandom();
                    currentCell.Link(nextCell);
                    currentCell = nextCell;
                } else {
                    foreach (var hunt in layout.VisitableCells) {
                        if (!hunt.IsVisited && hunt.Neighbors().Any(cell => cell.IsVisited)) {
                            currentCell = hunt;
                            currentCell.Link(hunt.Neighbors().Where(cell => cell.IsVisited).First());
                            break;
                        }
                    }
                }
            }
        }
    }
}