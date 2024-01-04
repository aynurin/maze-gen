using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Maze.PostProcessing {
    public static class DijkstraDistance {
        public const string LongestTrailStartAttribute =
            "PlayersWorlds.Maps.Maze.PostProcessing.DijkstraDistance.LongestTrailStartAttribute";
        public const string LongestTrailEndAttribute =
            "PlayersWorlds.Maps.Maze.PostProcessing.DijkstraDistance.LongestTrailEndAttribute";
        public const string LongestTrailAttribute =
            "PlayersWorlds.Maps.Maze.PostProcessing.DijkstraDistance.LongestTrailAttribute";
        public const string DistanceAttribute =
            "PlayersWorlds.Maps.Maze.PostProcessing.DijkstraDistance.DistanceAttribute";

        /// <summary>
        /// Finds Dijkstra distances for the given cell.
        /// </summary>
        /// <param name="startingCell">The cell to start the BFS walk.</param>
        /// <returns></returns>
        public static Dictionary<MazeCell, int> Find(MazeCell startingCell) {
            var distances = new Dictionary<MazeCell, int> {
                { startingCell, 0 }
            };
            startingCell.Attributes.Set(DistanceAttribute, "0");
            var stack = new Stack<MazeCell>();
            stack.Push(startingCell);
            MazeCell nextCell;
            while (true) {
                try {
                    nextCell = stack.Pop();
                } catch (InvalidOperationException) {
                    break;
                }
                var distance = distances[nextCell];
                foreach (var neighbor in nextCell.Links()) {
                    if (!distances.ContainsKey(neighbor)) {
                        distances.Add(neighbor, distance + 1);
                        neighbor.Attributes.Set(DistanceAttribute,
                            (distance + 1).ToString());
                        stack.Push(neighbor);
                    }
                }
            }
            return distances;
        }

        /// <summary>
        /// Finds the shortest path from the startingCell to the targetCell.
        /// </summary>
        /// <param name="targetCell"></param>
        /// <returns><c>List&lt;MazeCell&gt;</c> containing the solution from
        /// <paramref name="startingCell" /> to <paramref name="targetCell" />
        /// or <c>Optional&lt;List&lt;MazeCell&gt;&gt;.Empty</c> if the solution
        /// does not exist.</returns>        
        public static Optional<List<MazeCell>> Solve(
            MazeCell startingCell, MazeCell targetCell) {
            var distances = Find(startingCell);
            if (!distances.ContainsKey(targetCell)) {
                return Optional<List<MazeCell>>.Empty;
            }
            var solution = new List<MazeCell>() { targetCell };
            while (distances[targetCell] > 0) {
                targetCell = targetCell.Links().OrderBy(cell => distances[cell]).First();
                solution.Add(targetCell);
            }
            solution.Reverse();
            return solution;
        }

        public static List<MazeCell> FindLongestTrail(Maze2D maze) {
            maze.VisitedCells.ThrowIfNullOrEmpty("maze.VisitedCells");
            var distances = Find(maze.VisitedCells.First());
            var startingPoint = distances.OrderByDescending(kvp => kvp.Value)
                                         .Select(kvp => kvp.Key).First();
            startingPoint.Attributes.Set(LongestTrailStartAttribute, null);
            distances = Find(startingPoint);
            var targetPoint = distances.OrderByDescending(kvp => kvp.Value)
                                       .Select(kvp => kvp.Key).First();
            targetPoint.Attributes.Set(LongestTrailEndAttribute, null);
            var solution = Solve(startingPoint, targetPoint).Value;
            foreach (var cell in solution) {
                cell.Attributes.Set(LongestTrailAttribute, null);
            }
            return solution;
        }
    }
}