using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Wilson's algorithm implementation.
    /// </summary>
    /// <remarks>
    /// Wilson's rarely checks for priority cells, so it might not connect all
    /// areas.</remarks>
    public class WilsonsMazeGenerator : MazeGenerator {
        /// <summary>
        /// Generates a maze using Wilson's algorithm in the
        /// specified layout.
        /// </summary>
        /// <param name="builder"><see cref="Maze2DBuilder" /> instance for
        /// the maze to be generated.</param>
        override public void GenerateMaze(Maze2DBuilder builder) {
            // mark a random cell as connected
            // TODO: According to the algorithm, we need to mark an initial cell
            //       to make sure the algorithm concludes without creating one
            //       single path spanning the whole maze. But there is a chance
            //       it will never connect this cell. E.g. take a low
            //       fill factor and a case where it gets into a maze shape
            //       deadend while building the walkPath. If the walkPath
            //       contains enough cells to consider the maze complete, the
            //       algorithm will conclude with or without this cell being
            //       connected.

            var initialCell = builder.PickNextCellToLink();
            var visitedCells = new HashSet<MazeCell>() { initialCell };

            while (!builder.IsFillComplete()) {
                var walkPath = new List<MazeCell>();
                var nextCell = builder.PickNextCellToLink();

                if (visitedCells.Contains(nextCell))
                    continue;

                while (!visitedCells.Contains(nextCell)) {
                    var containsAt = walkPath.IndexOf(nextCell);
                    if (containsAt >= 0)
                        walkPath.RemoveRange(containsAt + 1, walkPath.Count - containsAt - 1);
                    else walkPath.Add(nextCell);
                    builder.TryPickRandomNeighbor(nextCell, out nextCell, honorPriority: false);
                };

                if (nextCell != null) {
                    walkPath.Add(nextCell);
                }

                for (var i = 0; i < walkPath.Count - 1; i++) {
                    builder.Connect(walkPath[i], walkPath[i + 1]);
                    visitedCells.Add(walkPath[i]);
                }
                visitedCells.Add(walkPath.Last());
            }
        }
    }
}