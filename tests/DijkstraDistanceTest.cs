using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Maze;
using PlayersWorlds.Maps.Maze.PostProcessing;

namespace PlayersWorlds.Maps {
    [TestFixture]
    public class DijkstraDistanceTest {
        [Test]
        public void DijkstraDistance_FindsAllDistances() {
            var maze = MazeGenerator
                .Generate<AldousBroderMazeGenerator>(new Vector(10, 10),
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Full
                });
            var visitedCells = maze.VisitedCells.ToList();
            var distances = DijkstraDistance.Find(visitedCells.GetRandom());
            Assert.AreEqual(visitedCells.Count, distances.Count);
            Assert.Greater(distances.Values.Average(), 1);
        }

        [Test]
        public void DijkstraDistance_CanSolveAMaze() {
            var maze = Maze2D.Parse("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            var solution = DijkstraDistance
                .Solve(maze.VisitableCells.First(), maze.VisitableCells.Last());
            Assert.IsTrue(solution.HasValue);
            Assert.AreEqual(5, solution.Value.Count);
            Assert.AreEqual(new Vector(0, 0),
                solution.Value.First().Coordinates);
            Assert.AreEqual(new Vector(2, 2),
                solution.Value.Last().Coordinates);
        }

        [Test]
        public void DijkstraDistance_ReturnsEmptyIfNoSolutionFound() {
            var maze = Maze2D.Parse("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            var solution = DijkstraDistance
                .Solve(maze.VisitableCells.First(), new MazeCell(1, 2));
            Assert.IsFalse(solution.HasValue);
        }

        [Test]
        public void DijkstraDistance_CanFindLongestTrail() {
            var maze = Maze2D.Parse("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            var solution = DijkstraDistance.FindLongestTrail(maze);
            Assert.AreEqual(6, solution.Count);
            Assert.AreEqual(8, maze.AllCells.Count(
                cell => !cell.Attributes.ContainsKey(
                    DijkstraDistance.LongestTrailStartAttribute)));
            Assert.AreEqual(8, maze.AllCells.Count(
                cell => !cell.Attributes.ContainsKey(
                    DijkstraDistance.LongestTrailEndAttribute)));
            Assert.AreEqual(3, maze.AllCells.Count(
                cell => !cell.Attributes.ContainsKey(
                    DijkstraDistance.LongestTrailAttribute)));
        }
    }
}