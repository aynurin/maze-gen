using System;
using System.Linq;
using NUnit.Framework;

namespace PlayersWorlds.Maps.Maze.PostProcessing {
    [TestFixture]
    public class DijkstraDistanceTest {
        [Test]
        public void DijkstraDistance_FindsAllDistances() {
            var maze = MazeTestHelper.GenerateMaze(new Vector(10, 10),
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.Full
                });
            var visitedCells = maze.MazeCells.ToList();
            var distances = DijkstraDistance.Find(visitedCells.Random());
            Assert.That(distances.Count, Is.EqualTo(visitedCells.Count));
            Assert.That(distances.Values.Average(), Is.GreaterThan(1));
        }

        [Test]
        public void DijkstraDistance_CanSolveAMaze() {
            var maze = Maze2D.Parse("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            var solution = DijkstraDistance
                .Solve(maze.Cells.First(), maze.Cells.Last());
            Assert.That(solution.HasValue, Is.True);
            Assert.That(5, Is.EqualTo(solution.Value.Count));
            Assert.That(new Vector(0, 0), Is.EqualTo(
                solution.Value.First().Coordinates));
            Assert.That(new Vector(2, 2), Is.EqualTo(
                solution.Value.Last().Coordinates));
        }

        [Test]
        public void DijkstraDistance_ReturnsEmptyIfNoSolutionFound() {
            var maze = Maze2D.Parse("3x3;0:3;1:4;2:5;3:4;4:7;6:7;7:8");
            var solution = DijkstraDistance
                .Solve(maze.Cells[new Vector(2, 1)],
                       maze.Cells[new Vector(1, 2)]);
            Assert.That(solution.HasValue, Is.False);
        }

        [Test]
        public void DijkstraDistance_CanFindLongestTrail() {
            var maze = Maze2D.Parse("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            var solution = DijkstraDistance.FindLongestTrail(maze);
            Assert.That(6, Is.EqualTo(solution.Count));
            Assert.That(8, Is.EqualTo(maze.Cells.Count(
                cell => !cell.Attributes.ContainsKey(
                    DijkstraDistance.LongestTrailStartAttribute))));
            Assert.That(8, Is.EqualTo(maze.Cells.Count(
                cell => !cell.Attributes.ContainsKey(
                    DijkstraDistance.LongestTrailEndAttribute))));
            Assert.That(3, Is.EqualTo(maze.Cells.Count(
                cell => !cell.Attributes.ContainsKey(
                    DijkstraDistance.LongestTrailAttribute))));
        }
    }
}