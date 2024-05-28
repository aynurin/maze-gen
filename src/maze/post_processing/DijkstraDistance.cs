using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Maze.PostProcessing {
    /// <summary>
    /// Dijkstra's algorithm to find the paths between cells. Used in many
    /// cases, for example to find the longest path in the maze, or the best
    /// start and end points.
    /// </summary>
    public static class DijkstraDistance {
        /// <summary>
        /// An extension object that contains the list of all cells that
        /// constitute the longest trail in the maze.
        /// </summary>
        public class LongestTrailExtension {
            /// <summary>
            /// Creates an instance of the LongestTrailExtension.
            /// </summary>
            /// <param name="longestTrail"></param>
            public LongestTrailExtension(IEnumerable<Cell> longestTrail) {
                LongestTrail = longestTrail.ToList();
            }

            /// <summary>
            /// The list of all cells that constitute the longest trail in the
            /// maze.
            /// </summary>
            public List<Cell> LongestTrail { get; private set; }
        }

        /// <summary>
        /// An extension object that denotes a longest trail.
        /// </summary>
        public class IsLongestTrailExtension {
        }

        /// <summary>
        /// An extension object that denotes a longest trail start.
        /// </summary>
        public class IsLongestTrailStartExtension {
        }

        /// <summary>
        /// An extension object that denotes a longest trail end.
        /// </summary>
        public class IsLongestTrailEndExtension {
        }

        /// <summary>
        /// An extension object that contains a distance from the start of the
        /// longest path in the maze.
        /// </summary>
        public class DistanceExtension {

            /// <summary>
            /// Creates an instance of the DistanceExtension.
            /// </summary>
            /// <param name="value"></param>
            public DistanceExtension(int value) {
                Value = value;
            }


            /// <summary>
            /// Distance value.
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Finds Dijkstra distances for the given cell.
        /// </summary>
        /// <param name="startingCell">The cell to start the BFS walk.</param>
        /// <returns></returns>
        public static Dictionary<Cell, int> Find(Cell startingCell) {
            var distances = new Dictionary<Cell, int> {
                { startingCell, 0 }
            };
            startingCell.X(new DistanceExtension(0));
            var stack = new Stack<Cell>();
            stack.Push(startingCell);
            Cell nextCell;
            while (stack.Count > 0) {
                nextCell = stack.Pop();
                var distance = distances[nextCell];
                foreach (var neighbor in nextCell.Links()) {
                    // if a maze has loops, we have to check if we are building
                    // a shorter path or a longer one.
                    if (!distances.ContainsKey(neighbor)) {
                        distances.Add(neighbor, distance + 1);
                    } else if (distance + 1 < distances[neighbor]) {
                        distances[neighbor] = distance + 1;
                    } else continue;
                    neighbor.X(new DistanceExtension(distances[neighbor]));
                    stack.Push(neighbor);
                }
            }
            return distances;
        }

        /// <summary>
        /// Finds Dijkstra distances for the given cell using connectable
        /// neighbors as opposed to actual maze links.
        /// </summary>
        /// <param name="builder">Maze builder that provides info on maze
        /// structure.</param>
        /// <param name="startingCell">The cell to start the BFS walk.</param>
        /// <returns></returns>
        public static Dictionary<Cell, int> FindRaw(Maze2DBuilder builder, Cell startingCell) {
            var distances = new Dictionary<Cell, int> {
                { startingCell, 0 }
            };
            startingCell.X(new DistanceExtension(0));
            var stack = new Stack<Cell>();
            stack.Push(startingCell);
            Cell nextCell;
            while (stack.Count > 0) {
                nextCell = stack.Pop();
                var distance = distances[nextCell];
                foreach (var neighborXY in nextCell.Neighbors()) {
                    var neighbor = builder.MazeArea[neighborXY];
                    if (!builder.CanConnect(nextCell, neighborXY))
                        continue;
                    // if a maze has loops, we have to check if we are building
                    // a shorter path or a longer one.
                    if (!distances.ContainsKey(neighbor)) {
                        distances.Add(neighbor, distance + 1);
                        neighbor.X(new DistanceExtension(distances[neighbor]));
                    } else if (distance + 1 < distances[neighbor]) {
                        distances[neighbor] = distance + 1;
                        neighbor.X<DistanceExtension>().Value = distances[neighbor];
                    } else continue;
                    stack.Push(neighbor);
                }
            }
            return distances;
        }

        /// <summary>
        /// Finds the shortest path from the startingCell to the targetCell.
        /// </summary>
        /// <returns><c>List&lt;Cell&gt;</c> containing the solution from
        /// <paramref name="startingCell" /> to <paramref name="targetCell" />
        /// or <c>Optional&lt;List&lt;Cell&gt;&gt;.Empty</c> if the solution
        /// does not exist.</returns>        
        public static Optional<List<Cell>> Solve(
            Cell startingCell, Cell targetCell) {
            var distances = Find(startingCell);
            if (!distances.ContainsKey(targetCell)) {
                return Optional<List<Cell>>.Empty;
            }
            var solution = new List<Cell>() { targetCell };
            while (distances[targetCell] > 0) {
                targetCell = targetCell.Links().OrderBy(cell => distances[cell]).First();
                solution.Add(targetCell);
            }
            solution.Reverse();
            return solution;
        }

        /// <summary />
        public static LongestTrailExtension FindLongestTrail(Maze2DBuilder builder) {
            builder.AllCells.ThrowIfNullOrEmpty("maze.MazeCells");
            var distances = Find(builder.AllCells.First());
            var startingPoint = distances.OrderByDescending(kvp => kvp.Value)
                                         .Select(kvp => kvp.Key).First();
            startingPoint.X(new IsLongestTrailStartExtension());
            distances = Find(startingPoint);
            var targetPoint = distances.OrderByDescending(kvp => kvp.Value)
                                       .Select(kvp => kvp.Key).First();
            startingPoint.X(new IsLongestTrailEndExtension());
            var solution = Solve(startingPoint, targetPoint).Value;
            foreach (var cell in solution) {
                startingPoint.X(new IsLongestTrailExtension());
            }
            return new LongestTrailExtension(solution);
        }

        [Obsolete]
        public static LongestTrailExtension FindLongestTrail(Area maze) {
            maze.Cells.ThrowIfNullOrEmpty("maze.MazeCells");
            var distances = Find(maze.Cells.First());
            var startingPoint = distances.OrderByDescending(kvp => kvp.Value)
                                         .Select(kvp => kvp.Key).First();
            startingPoint.X(new IsLongestTrailStartExtension());
            distances = Find(startingPoint);
            var targetPoint = distances.OrderByDescending(kvp => kvp.Value)
                                       .Select(kvp => kvp.Key).First();
            startingPoint.X(new IsLongestTrailEndExtension());
            var solution = Solve(startingPoint, targetPoint).Value;
            foreach (var cell in solution) {
                startingPoint.X(new IsLongestTrailExtension());
            }
            return new LongestTrailExtension(solution);
        }
    }
}