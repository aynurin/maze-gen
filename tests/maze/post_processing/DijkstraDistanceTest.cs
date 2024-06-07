using System;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Serializer;

namespace PlayersWorlds.Maps.Maze.PostProcessing {
    [TestFixture]
    public class DijkstraDistanceTest : Test {
        [Test]
        public void DijkstraDistance_FindsAllDistances() {
            var random = RandomSource.CreateFromEnv();
            var maze = MazeTestHelper.GenerateMaze(new Vector(10, 10),
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full
                });
            var visitedCells = maze.Cells.Positions
                .Where(cell => maze.CellHasLinks(cell)).ToList();
            var distances = DijkstraDistance.Find(maze, random.RandomOf(visitedCells));
            Assert.That(distances.Count, Is.EqualTo(visitedCells.Count));
            Assert.That(distances.Values.Average(), Is.GreaterThan(1));
        }

        [Test]
        public void DijkstraDistance_CanSolveAMaze() {
            var maze = LegacyAreaSerializer.ParseV01MazeString("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            var solution = DijkstraDistance
                .Solve(maze, maze.Cells.Positions.First(), maze.Cells.Positions.Last());
            Assert.That(solution.HasValue, Is.True);
            Assert.That(5, Is.EqualTo(solution.Value.Count));
            Assert.That(new Vector(0, 0), Is.EqualTo(
                solution.Value.First()));
            Assert.That(new Vector(2, 2), Is.EqualTo(
                solution.Value.Last()));
        }

        [Test]
        public void DijkstraDistance_ReturnsEmptyIfNoSolutionFound() {
            var maze = LegacyAreaSerializer.ParseV01MazeString("3x3;0:3;1:4;2:5;3:4;4:7;6:7;7:8");
            var solution = DijkstraDistance
                .Solve(maze,
                       new Vector(2, 1),
                       new Vector(1, 2));
            Assert.That(solution.HasValue, Is.False);
        }

        [Test]
        public void DijkstraDistance_CanFindLongestTrail() {
            var maze = LegacyAreaSerializer.ParseV01MazeString("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
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