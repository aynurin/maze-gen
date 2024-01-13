using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Hunt-and-kill algorithm implementation.
    /// </summary>
    public class HuntAndKillMazeGenerator : MazeGenerator {
        /// <summary>
        /// Generates a maze using Hunt-and-kill algorithm in the specified
        /// layout.
        /// </summary>
        /// <param name="layout">The layout to generate the maze in.</param>
        /// <param name="options">The generator options to use.</param>
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