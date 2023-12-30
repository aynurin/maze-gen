using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Maze.PostProcessing {
    public static class DeadEnd {
        public const string DeadEndAttribute =
            "PlayersWorlds.Maps.Maze.PostProcessing.DeadEnd.DeadEndAttribute";

        public static List<MazeCell> Find(Maze2D maze) {
            var deadEnds = maze.VisitableCells.Where(cell => cell.Links().Count == 1)
                .ToList();
            foreach (var cell in deadEnds) {
                cell.Attributes.Set(DeadEndAttribute,
                    null);
            }
            return deadEnds;
        }
    }
}