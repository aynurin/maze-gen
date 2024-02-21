using System;
using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Aldous-Broder algorithm implementation.
    /// </summary>
    public class AldousBroderMazeGenerator : MazeGenerator {
        /// <summary>
        /// Generates a maze using Aldous-Broder algorithm in the specified
        /// layout.
        /// </summary>
        /// <remarks>
        /// Aldous-Broder's algorithm walks the maze via random neighbors. It
        /// does not stretch far by picking random maze cells. Some of the side
        /// effects is that if there are scattered areas, it has a high chance
        /// of not connecting all areas with sparce maze fill factors.
        /// </remarks>
        /// <param name="builder"><see cref="Maze2DBuilder" /> instance for
        /// the maze to be generated.</param>
        override public void GenerateMaze(Maze2DBuilder builder) {
            var currentCell = builder.PickNextCellToLink();
            var ineffectiveLoops = 0;
            while (!builder.IsFillComplete()) {
                if (ineffectiveLoops > builder.CellsToConnect.Count * 10) {
                    currentCell = builder.PickNextCellToLink();
                }
                if (builder.TryPickRandomNeighbor(currentCell, out var next)) {
                    // if we found an unconnected neighbor, or if currentCell
                    // itself is not connected (which means we just called 
                    // PickNextCellToLink) 
                    if (!builder.IsConnected(next) ||
                        !builder.IsConnected(currentCell)) {
                        builder.Connect(currentCell, next);
                        ineffectiveLoops = 0;
                    } else {
                        ineffectiveLoops++;
                    }
                    currentCell = next;
                } else {
                    throw new NotImplementedException(
                        $"Investigate TryPickRandomNeighbor returning " +
                        $"empty for neighbors of {currentCell} in maze:\n" +
                        builder.ToString());
                }
            }
        }
    }
}