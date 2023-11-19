using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nour.Play.Maze;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class AreaDistributorLoadTest {

        [Test, Property("Category", "Load")]
        public void AreaDistributor_LoadTest() {
            var results = new List<AreaDistributorHelper.DistributeResult>();
            var ops = new ParallelOptions {
                MaxDegreeOfParallelism = 24
            };
            // !! Current fail rate is at 5.2%. See MapAreaSystem.GetAreaForce2
            // !! for the most probable reason.
            var numTotal = 1000;
            var numPassed = 0;
            _ = Parallel.For(0, numTotal, ops, (i, state) => {
                var log = Log.CreateForThisTest();
                var maze = new Maze2D(GlobalRandom.Next(5, 50), GlobalRandom.Next(5, 50));
                var env = new AreaDistributor.Room(VectorD.Zero2D, new VectorD(maze.Size));
                var roomsCount = (int)Math.Sqrt(maze.Area) / 3;
                var rooms = new List<AreaDistributor.Room>();
                for (var j = 0; j < roomsCount; j++) {
                    // TODO: make random of three types (horizontal, vertical, square)
                    var size = new Vector(
                        GlobalRandom.Next(1, maze.Size.X / 3),
                        GlobalRandom.Next(1, maze.Size.Y / 3));
                    var position = AreaDistributor.RandomRoomPosition(maze.Size - size);
                    rooms.Add(new AreaDistributor.Room(new VectorD(position), new VectorD(size)));
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
            Assert.IsTrue(results.All(r =>
                r.PlacedOutOfBounds.Count + r.PlacedOverlapping.Count == 0),
                "Passed: " + numPassed + ", Failed: " + (numTotal - numPassed));
        }
    }
}