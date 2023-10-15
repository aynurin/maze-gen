using System;
using System.Collections.Generic;
using System.Linq;

namespace Nour.Play.Maze.Solvers {
    public class DijkstraDistance {
        private readonly Dictionary<Cell, int> _distances;
        private Optional<List<Cell>> _solution = Optional<List<Cell>>.Empty;

        public int this[Cell cell] { get => _distances[cell]; }
        public Optional<List<Cell>> Solution { get => _solution; }

        private DijkstraDistance(Dictionary<Cell, int> distances) {
            _distances = distances;
        }

        /// Finds DijkstraDistance starting from the given cell
        public static DijkstraDistance Find(Cell startingCell) {
            var distances = new Dictionary<Cell, int>();
            distances.Add(startingCell, 0);
            var stack = new Stack<Cell>();
            stack.Push(startingCell);
            Cell nextCell;
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

        /// Finds the shortest path from the targetCell to the startingCell this
        /// instance has been build with. 
        public Optional<List<Cell>> Solve(Cell targetCell) {
            var solution = new List<Cell>() { targetCell };
            while (_distances[targetCell] > 0) {
                targetCell = targetCell.Links().OrderBy(cell => _distances[cell]).First();
                solution.Add(targetCell);
            }
            return _solution = new Optional<List<Cell>>(solution);
        }

        public static DijkstraDistance FindLongest(Cell arbitrary) {
            var distances = Find(arbitrary);
            var startingPoint = distances._distances.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).First();
            distances = Find(startingPoint);
            var targetPoint = distances._distances.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).First();
            distances.Solve(targetPoint);
            return distances;
        }
    }
}