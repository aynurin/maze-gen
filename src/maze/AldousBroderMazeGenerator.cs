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
            var walkPath = new List<MazeCell>();
            while (!builder.IsFillComplete()) {
                walkPath.Add(currentCell);
                if (builder.TryPickRandomNeighbor(currentCell, out var next)) {
                    if (!builder.IsConnected(next)) {
                        builder.Connect(currentCell, next);
                    }
                    currentCell = next;
                } else {
                    throw new NotImplementedException(
                        $"Investigate TryPickRandomNeighbor returning " +
                        $"empty for neighbors of {currentCell} (walk path: {string.Join(",", walkPath)}) in maze:\n" +
                        builder.ToString());
                }
            }
        }
    }
}