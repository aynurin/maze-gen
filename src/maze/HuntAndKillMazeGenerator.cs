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
        /// <remarks>
        /// Just like Aldous-Broder's algorithm, this algorithm walks the maze
        /// in a modest way without trying to reach far. So in the same way it
        /// has a high chance of not connecting all areas with sparce maze fill 
        /// factors.
        /// </remarks>
        /// <param name="builder"><see cref="Maze2DBuilder" /> instance for
        /// the maze to be generated.</param>
        override public void GenerateMaze(Maze2DBuilder builder) {
            var currentCell = builder.PickNextCellToLink();
            while (!builder.IsFillComplete()) {
                if (builder.TryPickRandomNeighbor(
                        currentCell, out var nextCell, true)) {
                    builder.Connect(currentCell, nextCell);
                    currentCell = nextCell;
                } else {
                    foreach (var hunt in builder.PrioritizedCellsToConnect) {
                        var firstVisitedNeighbor =
                            hunt.Neighbors()
                                .Where(builder.IsConnected)
                                .FirstOrDefault();
                        if (firstVisitedNeighbor != null) {
                            currentCell = hunt;
                            builder.Connect(currentCell, firstVisitedNeighbor);
                            break;
                        }
                    }
                }
            }
        }
    }
}