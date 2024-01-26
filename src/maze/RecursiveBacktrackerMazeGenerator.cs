using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Recursive Backtracker algorithm implementation.
    /// </summary>
    public class RecursiveBacktrackerMazeGenerator : MazeGenerator {
        /// <summary>
        /// Generates a maze using Recursive Backtracker algorithm in the
        /// specified layout.
        /// </summary>
        /// <param name="layout">The layout to generate the maze in.</param>
        /// <param name="options">The generator options to use.</param>
        override public void GenerateMaze(Maze2D layout, GeneratorOptions options) {
            var stack = new Stack<MazeCell>();
            stack.Push(layout.UnlinkedCells.GetRandom());
            while (!IsFillComplete(options, layout) && stack.Count > 0) {
                var potentiallyNext = stack.First().Neighbors().Where(cell => !cell.IsVisited).ToList();
                if (potentiallyNext.Count > 0) {
                    var nextCell = potentiallyNext.GetRandom();
                    stack.First().Link(nextCell);
                    stack.Push(nextCell);
                } else {
                    stack.Pop();
                }
            }
        }
    }
}