using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nour.Play.Maze;
using NUnit.Framework;

namespace Nour.Play {
    public class AreaDistributorHelper {
        private int counter;
        private int item1;
        private int countFit;

        private readonly Log _log;

        public AreaDistributorHelper(Log log) {
            _log = log;
        }

        public void Stats() {
            _log.Buffered.I($"===== FIT ROOMS: {countFit} =====");
            _log.Buffered.I($"===== NOT FIT ROOMS: {item1} =====");
            _log.Buffered.I($"===== REQUESTS: {counter} =====");
        }

        public Tuple<int, string> PlaceRoomsAndValidate(AreaDistributor d,
            Maze2D maze, List<AreaDistributor.Room> placedRooms, int maxEpochs = -1) {
            Interlocked.Increment(ref counter);
            Tuple<int, string> result;
            try {
                placedRooms = d.DistributePlacedRooms(maze, placedRooms, maxEpochs);
                var fitRooms = placedRooms.Where(room =>
                                                (room.Position + room.Size).RoundToInt().AllLessThanOrEqualTo(maze.Size) &&
                                                 room.Position.RoundToInt().AllGreaterThanOrEqualTo(Vector.Zero2D)).ToList();
                var unfitRooms = placedRooms.Where(room =>
                                                (room.Position + room.Size).RoundToInt().AnyGreaterThan(maze.Size) ||
                                                 room.Position.RoundToInt().AnyLessThan(Vector.Zero2D)).ToList();
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
            return result;
        }

        private bool DoOverlap(AreaDistributor.Room one, AreaDistributor.Room another) {
            bool noOverlap = one.LowX > another.HighX ||
                               one.HighY > another.LowY ||
                               another.LowX > one.HighX ||
                               another.HighY > one.LowY;
            return !noOverlap;
        }
    }
}