using System;
using System.Collections.Generic;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Aldous-Broder algorithm implementation.
    /// </summary>
    public class AldousBroderMazeGenerator : MazeGenerator {
        /// <summary>
        /// Generates a maze using Aldous-Broder algorithm in the specified
        /// layout.
        /// </summary>
        /// <param name="layout">The layout to generate the maze in.</param>
        /// <param name="options">The generator options to use.</param>
        override public void GenerateMaze(Maze2D layout, GeneratorOptions options) {
            var currentCell = layout.UnlinkedCells.GetRandom();
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