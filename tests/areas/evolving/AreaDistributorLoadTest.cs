using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps.Areas.Evolving {
    [TestFixture]
    public class AreaDistributorLoadTest : Test {

        [Test, Category("Load")]
        public void AreaDistributor_LoadTest() {
            var testRandom = RandomSource.CreateFromEnv();
            var results = new List<AreaDistributorHelper.DistributeResult>();
            var ops = new ParallelOptions {
                MaxDegreeOfParallelism = 24
            };
            // !! Current fail rate is at <0.5%. Requires investigation.
            var numTotal = 1000;
            var numPassed = 0;
            _ = Parallel.For(0, numTotal, ops, (i, state) => {
                var log = TestLog.CreateForThisTest();
                var maze = new Maze2D(testRandom.Next(5, 50), testRandom.Next(5, 50));
                var roomsCount = (int)Math.Sqrt(maze.Size.Area) / 3;
                var rooms = new List<Area>();
                for (var j = 0; j < roomsCount; j++) {
                    var size = new Vector(
                        testRandom.Next(1, maze.Size.X / 3),
                        testRandom.Next(1, maze.Size.Y / 3));
                    var position = new Vector(
                        testRandom.Next(0, (maze.Size - size).X),
                        testRandom.Next(0, (maze.Size - size).Y));
                    rooms.Add(Area.CreateUnpositioned(
                        position, size, AreaType.None));
                }
                var result = AreaDistributorHelper.Distribute(testRandom, log, maze.Size, rooms, 100);
                lock (results) {
                    if (result.PlacedOutOfBounds.Count > 0 || result.PlacedOverlapping.Count > 0) {
                        log.D(0, result.DebugString());
                    } else {
                        numPassed++;
                    }
                    results.Add(result);
                    log.Buffered.Reset();
                }
            });
            Assert.That(results.All(r =>
                r.PlacedOutOfBounds.Count + r.PlacedOverlapping.Count == 0),
                Is.True,
                "Passed: " + numPassed + ", Failed: " + (numTotal - numPassed));
        }
    }
}