using System;
using System.Linq;
using Nour.Play.Maze;
using Nour.Play.Maze.PostProcessing;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class DijkstraDistanceTest {
        [Test]
        public void DijkstraDistance_FindsAllDistances() {
            var maze = MazeGenerator
                .Generate<AldousBroderMazeGenerator>(new Vector(10, 10));
            var distances = DijkstraDistance.Find(maze.Cells.GetRandom());
            Assert.AreEqual(maze.Cells.Count, distances.Count);
            Assert.Greater(distances.Values.Average(), 1);
        }

        [Test]
        public void DijkstraDistance_CanSolveAMaze() {
            var maze = Maze2D.Parse("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            var solution = DijkstraDistance
                .Solve(maze.Cells.First(), maze.Cells.Last());
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
                .Solve(maze.Cells.First(), new MazeCell(1, 2));
            Assert.IsFalse(solution.HasValue);
        }

        [Test]
        public void DijkstraDistance_CanFindLongestTrail() {
            var maze = Maze2D.Parse("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            var solution = DijkstraDistance.FindLongestTrail(maze);
            Assert.AreEqual(6, solution.Count);
            Assert.AreEqual(8, maze.Cells.Count(
                cell => !cell.Attributes.ContainsKey(
                    DijkstraDistance.LongestTrailStartAttribute)));
            Assert.AreEqual(8, maze.Cells.Count(
                cell => !cell.Attributes.ContainsKey(
                    DijkstraDistance.LongestTrailEndAttribute)));
            Assert.AreEqual(3, maze.Cells.Count(
                cell => !cell.Attributes.ContainsKey(
                    DijkstraDistance.LongestTrailAttribute)));
        }
    }
}