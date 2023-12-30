using System;
using System.Collections.Generic;
using System.Linq;

namespace Nour.Play.Maze.PostProcessing {
    public static class DijkstraDistance {
        public const string LongestTrailStartAttribute =
            "Nour.Play.Maze.PostProcessing.DijkstraDistance.LongestTrailStartAttribute";
        public const string LongestTrailEndAttribute =
            "Nour.Play.Maze.PostProcessing.DijkstraDistance.LongestTrailEndAttribute";
        public const string LongestTrailAttribute =
            "Nour.Play.Maze.PostProcessing.DijkstraDistance.LongestTrailAttribute";
        public const string DistanceAttribute =
            "Nour.Play.Maze.PostProcessing.DijkstraDistance.DistanceAttribute";

        /// Finds Dijkstra distances for the given cell.
        public static Dictionary<MazeCell, int> Find(MazeCell startingCell) {
            var distances = new Dictionary<MazeCell, int>();
            distances.Add(startingCell, 0);
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
        /// <returns><code>List<MazeCell></code> containing the solution from
        /// <paramref name="startingCell" /> to <paramref name="targetCell" />
        /// or <code>Optional<List<MazeCell>>.Empty</code> if the solution
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