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
            var results = new List<AreaDistributorHelper.DistributeResult>();
            var ops = new ParallelOptions {
                MaxDegreeOfParallelism = 24
            };
            // !! Current fail rate is at ~2%. Requires investigation.
            var numTotal = 1000;
            var numPassed = 0;
            _ = Parallel.For(0, numTotal, ops, (i, state) => {
                var log = TestLog.CreateForThisTest();
                var maze = new Maze2D(GlobalRandom.Next(5, 50), GlobalRandom.Next(5, 50));
                var roomsCount = (int)Math.Sqrt(maze.Size.Area) / 3;
                var rooms = new List<MapArea>();
                for (var j = 0; j < roomsCount; j++) {
                    var size = new Vector(
                        GlobalRandom.Next(1, maze.Size.X / 3),
                        GlobalRandom.Next(1, maze.Size.Y / 3));
                    var position = new Vector(
                        GlobalRandom.Next(0, (maze.Size - size).X),
                        GlobalRandom.Next(0, (maze.Size - size).Y));
                    rooms.Add(MapArea.CreateAutoPositioned(
                        AreaType.None, position, size));
                }
                var result = AreaDistributorHelper.Distribute(log, maze.Size, rooms, 100);
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