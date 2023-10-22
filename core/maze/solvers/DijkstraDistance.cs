using System;
using System.Collections.Generic;
using System.Linq;

namespace Nour.Play.Maze.Solvers {
    public class DijkstraDistance {
        private readonly Dictionary<MazeCell, int> _distances;
        private Optional<List<MazeCell>> _solution = Optional<List<MazeCell>>.Empty;

        public int this[MazeCell cell] { get => _distances[cell]; }
        public Optional<List<MazeCell>> Solution { get => _solution; }

        private DijkstraDistance(Dictionary<MazeCell, int> distances) {
            _distances = distances;
        }

        /// Finds DijkstraDistance starting from the given cell
        public static DijkstraDistance Find(MazeCell startingCell) {
            var distances = new Dictionary<MazeCell, int>();
            distances.Add(startingCell, 0);
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
                        stack.Push(neighbor);
                    }
                }
            }
            return new DijkstraDistance(distances);
        }

        /// <summary>
        /// Finds the shortest path from the targetCell to the startingCell this
        /// instance has been build with. Stores the Solution in this instance.
        /// </summary>
        /// <param name="targetCell"></param>
        /// <returns></returns>        
        public Optional<List<MazeCell>> Solve(MazeCell targetCell) {
            var solution = new List<MazeCell>() { targetCell };
            while (_distances[targetCell] > 0) {
                targetCell = targetCell.Links().OrderBy(cell => _distances[cell]).First();
                solution.Add(targetCell);
            }
            return _solution = new Optional<List<MazeCell>>(solution);
        }

        public static DijkstraDistance FindLongest(MazeCell arbitrary) {
            var distances = Find(arbitrary);
            var startingPoint = distances._distances.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).First();
            distances = Find(startingPoint);
            var targetPoint = distances._distances.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).First();
            distances.Solve(targetPoint);
            return distances;
        }
    }
}