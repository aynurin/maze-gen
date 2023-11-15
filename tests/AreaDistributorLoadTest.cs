using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nour.Play.Maze;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class AreaDistributorLoadTest {

        [Test]
        public void AreaDistributor_LoadTest() {
            var log = Log.CreateForThisTest();
            var helper = new AreaDistributorHelper(log);
            var results = new List<Tuple<int, string>>();
            for (int i = 0; i < 1000; i++) {
                var maze = new Maze2D(GlobalRandom.Next(5, 50), GlobalRandom.Next(5, 50));
                var roomsCount = (int)Math.Sqrt(maze.Area) / 3;
                var rooms = new List<AreaDistributor.Room>();
                for (int j = 0; j < roomsCount; j++) {
                    var size = new Vector(
                        GlobalRandom.Next(1, maze.Size.X / 3),
                        GlobalRandom.Next(1, maze.Size.Y / 3));
                    var position = AreaDistributor.RandomRoomPosition(maze.Size - size);
                    rooms.Add(new AreaDistributor.Room(new VectorD(position), new VectorD(size)));
                }
                var d = new AreaDistributor(log);
                var result = helper.PlaceRoomsAndValidate(d, maze, rooms);
                if (result.Item1 > 0) {
                    log.D(result.Item2);
                }
                results.Add(result);
                log.Buffered.Reset();
            }
            Assert.IsTrue(results.All(r => r.Item1 == 0));
        }
    }
}