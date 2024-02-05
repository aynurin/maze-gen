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
            var builder = new Maze2DBuilder(layout, options);
            var currentCell = builder.PickRandomCellToLink();
            builder.MarkConnected(currentCell);
            while (!builder.IsFillComplete()) {
                var next = builder.PickRandomNeighborToLink(currentCell);
                if (!next.HasValue) {
                    throw new NotImplementedException(
                        $"Investigate PickRandomNeighborToLink returning " +
                        $"empty for neighbors of {currentCell} in maze:\n" +
                        layout.ToString());
                }
                if (!builder.IsVisited(next.Value)) {
                    currentCell.Link(next.Value);
                    builder.MarkConnected(next.Value);
                }
                currentCell = next.Value;
            }
            builder.ConnectHalls();
        }
    }
}