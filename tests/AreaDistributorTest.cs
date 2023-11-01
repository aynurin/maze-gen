using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nour.Play.Maze;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class AreaDistributorTest {
        private Log _log = Log.CreateDefault();
        private int counter;
        private int item1;
        private int countFit;

        [TestFixtureTearDown]
        public void Stats() {
            Console.WriteLine($"===== FIT ROOMS: {countFit} =====");
            Console.WriteLine($"===== NOT FIT ROOMS: {item1} =====");
            Console.WriteLine($"===== REQUESTS: {counter} =====");
        }

        [Test]
        public void AreaDistributorTest_Fits1() {
            TestLayout("13x8: P9x6;S3x1, P1x4;S2x1");
            TestLayout("20x20: P14x14;S5x3, P-2x14;S1x4, P7x-3;S2x4, P2x4;S1x3");
            TestLayout("6x14: P0x10;S1x3");
            TestLayout("23x22: P11x10;S6x2, P-1x14;S5x1, P5x-3;S5x3, P11x22;S2x1");
            TestLayout("18x23: P-3x-8;S5x5, P30x8;S3x6, P9x3;S4x4, P12x15;S1x3");
            TestLayout("23x8: P4x7;S4x1, P2x-1;S1x1");
            TestLayout("21x13: P8x-3;S6x3, P8x4;S6x3, P7x2;S4x2");
            TestLayout("16x14: P2x13;S3x1, P11x-1;S4x3");
            TestLayout("24x12: P12x7;S2x1, P14x-1;S5x2, P16x11;S1x1");
            TestLayout("9x18: P2x10;S2x2, P2x8;S2x5");
            TestLayout("9x18: P2x8;S2x5, P2x10;S2x2");
            TestLayout("21x21: P-3x-3;S6x6, P6x10;S4x2, P9x16;S2x2, P3x14;S4x2");
        }

        [Test]
        public void AreaDistributorTest_CanDistributeAreas() {
            var results = new List<Tuple<int, string>>();
            for (int i = 0; i < 1000; i++) {
                var maze = new Maze2D(GlobalRandom.Next(5, 25), GlobalRandom.Next(5, 25));
                var roomsCount = (int)Math.Sqrt(maze.Area) / 5;
                var rooms = new List<AreaDistributor.Room>();
                for (int j = 0; j < roomsCount; j++) {
                    var size = new Vector(GlobalRandom.Next(1, maze.Size.X / 3), GlobalRandom.Next(1, maze.Size.Y / 3));
                    var position = AreaDistributor.RandomRoomPosition(maze.Size - size);
                    rooms.Add(new AreaDistributor.Room(position, size));
                }
                results.Add(PlaceRoomsAndValidate(maze, rooms));
            }
            Assert.IsTrue(results.All(r => r.Item1 == 0));
        }

        private Tuple<int, string> TestLayout(string layout) {
            var parts = layout.Split(':');
            var maze = new Maze2D(Vector.Parse(parts[0]));
            var rooms = parts[1].Split(',').Select(s => AreaDistributor.Room.Parse(s).Size).ToList();
            var placedRooms = parts[1].Split(',').Select(s => AreaDistributor.Room.Parse(s)).ToList();

            var result = PlaceRoomsAndValidate(maze, placedRooms);
            Assert.IsTrue(result.Item1 == 0, result.Item2);
            return result;
        }

        private Tuple<int, string> PlaceRoomsAndValidate(Maze2D maze, List<AreaDistributor.Room> placedRooms) {
            Interlocked.Increment(ref counter);
            _log.Buffered.Reset();
            AreaDistributor d = new AreaDistributor(_log);
            Tuple<int, string> result;
            try {
                placedRooms = d.DistributePlacedRooms(maze, placedRooms);
                var fitRooms = placedRooms.Where(room =>
                                                (room.Position + room.Size).AllLessThanOrEqualTo(maze.Size) &&
                                                 room.Position.AllGreaterThanOrEqualTo(Vector.Zero2D)).ToList();
                var unfitRooms = placedRooms.Where(room =>
                                                (room.Position + room.Size).AnyGreaterThan(maze.Size) ||
                                                 room.Position.AnyLessThan(Vector.Zero2D)).ToList();
                var overlappingRooms = placedRooms.Where(room =>
                                                 placedRooms.Any(otherRoom => room != otherRoom &&
                                                 DoOverlap(room, otherRoom))).ToList();
                string message =
                    $"{maze.Size}: " +
                    (unfitRooms.Count > 0 || overlappingRooms.Count > 0 ? "OVERLAP" : "OK") +
                    $" Didn't fit {unfitRooms.Count}: " + String.Join(", ", unfitRooms) +
                    $"; Fit {fitRooms.Count}: " + String.Join(", ", fitRooms) +
                    $"; Overlap {overlappingRooms.Count}: " + String.Join(", ", overlappingRooms);
                Interlocked.Add(ref countFit, fitRooms.Count);
                result = new Tuple<int, string>(unfitRooms.Count + overlappingRooms.Count, message);
            } catch (InvalidOperationException ex) {
                result = new Tuple<int, string>(placedRooms.Count,
                    $"{maze.Size}: " + String.Join(", ", placedRooms) + ": " + ex.Message);
            } catch (Exception ex) {
                result = new Tuple<int, string>(placedRooms.Count,
                    $"{maze.Size}: " + String.Join(", ", placedRooms) + ": " + ex.ToString());
            }
            Interlocked.Add(ref item1, result.Item1);
            if (result.Item1 > 0) {
                _log.Buffered.Flush();
                _log.D(result.Item2);
            }
            return result;
        }

        private bool DoOverlap(AreaDistributor.Room one, AreaDistributor.Room another) {
            bool dontOverlap = one.LowX > another.HighX ||
                               one.HighY > another.LowY ||
                               another.LowX > one.HighX ||
                               another.HighY > one.LowY;
            return !dontOverlap;
        }
    }
}