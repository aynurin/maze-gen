using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Maze.PostProcessing {
    /// <summary>
    /// Find dead ends in the maze.
    /// </summary>
    public static class DeadEnd {
        /// <summary>
        /// An extension object that contains the list of all found dead ends.
        /// </summary>
        public class DeadEndsExtension {
            /// <summary>
            /// Creates an instance of the DeadEndsExtension.
            /// </summary>
            /// <param name="deadEnds"></param>
            public DeadEndsExtension(IEnumerable<Cell> deadEnds) {
                DeadEnds = deadEnds.ToList();
            }

            /// <summary>
            /// The list of all found dead ends.
            /// </summary>
            public List<Cell> DeadEnds { get; private set; }
        }

        /// <summary>
        /// An extension object that denotes a dead end.
        /// </summary>
        public class IsDeadEndExtension {
        }

        /// <summary>
        /// Find dead ends in the maze.
        /// </summary>
        public static DeadEndsExtension Find(Maze2DBuilder maze) {
            var deadEnds = maze.AllCells.Where(cell => maze.MazeArea.CellLinks(cell.Position).Count == 1)
                .ToList();
            foreach (var cell in deadEnds) {
                cell.X(new IsDeadEndExtension());
            }
            return new DeadEndsExtension(deadEnds);
        }

        /// <summary>
        /// Find dead ends in the maze.
        /// </summary>
        [Obsolete]
        public static DeadEndsExtension Find(Area maze) {
            var deadEnds = maze.Cells.Where(cell => maze.CellLinks(cell.Position).Count == 1)
                .ToList();
            foreach (var cell in deadEnds) {
                cell.X(new IsDeadEndExtension());
            }
            return new DeadEndsExtension(deadEnds);
        }
    }
}