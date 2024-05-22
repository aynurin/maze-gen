using System;
using System.Linq;
using NUnit.Framework;

namespace PlayersWorlds.Maps.Maze.PostProcessing {
    [TestFixture]
    public class DijkstraDistanceTest : Test {
        [Test]
        public void DijkstraDistance_FindsAllDistances() {
            var random = RandomSource.CreateFromEnv();
            var maze = MazeTestHelper.GenerateMaze(new Vector(10, 10),
                new GeneratorOptions() {
                    Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.FillFactorOption.Full
                });
            var visitedCells = maze.Cells
                .Where(cell => cell.Links().Count > 0).ToList();
            var distances = DijkstraDistance.Find(random.RandomOf(visitedCells));
            Assert.That(distances.Count, Is.EqualTo(visitedCells.Count));
            Assert.That(distances.Values.Average(), Is.GreaterThan(1));
        }

        [Test]
        public void DijkstraDistance_CanSolveAMaze() {
            var maze = Area.ParseAsMaze("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            var solution = DijkstraDistance
                .Solve(maze.Cells.First(), maze.Cells.Last());
            Assert.That(solution.HasValue, Is.True);
            Assert.That(5, Is.EqualTo(solution.Value.Count));
            Assert.That(new Vector(0, 0), Is.EqualTo(
                solution.Value.First().Position));
            Assert.That(new Vector(2, 2), Is.EqualTo(
                solution.Value.Last().Position));
        }

        [Test]
        public void DijkstraDistance_ReturnsEmptyIfNoSolutionFound() {
            var maze = Area.ParseAsMaze("3x3;0:3;1:4;2:5;3:4;4:7;6:7;7:8");
            var solution = DijkstraDistance
                .Solve(maze.Cells[new Vector(2, 1)],
                       maze.Cells[new Vector(1, 2)]);
            Assert.That(solution.HasValue, Is.False);
        }

        [Test]
        public void DijkstraDistance_CanFindLongestTrail() {
            var maze = Area.ParseAsMaze("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            var solution = DijkstraDistance.FindLongestTrail(maze);
            Assert.That(solution.LongestTrail.Count, Is.EqualTo(6));
            Assert.That(maze.Cells.Count(
                cell => cell.X<DijkstraDistance.IsLongestTrailStartExtension>() != null), Is.EqualTo(1));
            Assert.That(maze.Cells.Count(
                cell => cell.X<DijkstraDistance.IsLongestTrailEndExtension>() != null), Is.EqualTo(1));
            // perhaps the caller of FindLongestTrail will add it as maze extension?..
            Assert.That(maze.Cells.Count(
                cell => maze.X<DijkstraDistance.LongestTrailExtension>() != null), Is.EqualTo(0));
        }
    }
}