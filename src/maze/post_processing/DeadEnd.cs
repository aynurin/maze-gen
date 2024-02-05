using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Maze.PostProcessing {
    /// <summary>
    /// Find dead ends in the maze.
    /// </summary>
    public static class DeadEnd {
        /// <summary>
        /// Name of the Dead Ends attribute added to the maze cells.
        /// </summary>
        public const string DeadEndAttribute =
            "PlayersWorlds.Maps.Maze.PostProcessing.DeadEnd.DeadEndAttribute";

        /// <summary>
        /// Find dead ends in the maze.
        /// </summary>
        public static List<MazeCell> Find(Maze2D maze) {
            var deadEnds = maze.AllCells.Where(cell => cell.Links().Count == 1)
                .ToList();
            foreach (var cell in deadEnds) {
                cell.Attributes.Set(DeadEndAttribute,
                    null);
            }
            return deadEnds;
        }
    }
}