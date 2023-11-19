using System;
using System.Collections.Generic;
using System.Linq;
using Nour.Play.Areas;
using Nour.Play.Maze;
using NUnit.Framework;

namespace Nour.Play {
    internal class AreaDistributorHelper {
        internal static DistributeResult Distribute(
            Log log,
            Vector mazeSize,
            IEnumerable<AreaDistributor.Room> rooms,
            int maxEpochs = -1) {
            var mapAreas = rooms.Select(
                room => new MapArea(
                    AreaType.None,
                    room.Size.RoundToInt(),
                    room.Position.RoundToInt()))
                .ToList();
            return Distribute(log, new Maze2D(mazeSize), mapAreas, maxEpochs);
        }

        internal static DistributeResult Distribute(
            Log log,
            Maze2D maze,
            IEnumerable<MapArea> areas,
            int maxEpochs = -1) {

            var drawEachEpoch = false;
            if (TestContext.Parameters.Exists("DEBUG")) {
                drawEachEpoch = int.Parse(TestContext.Parameters["DEBUG"]) >= 5;
            }
            if (TestContext.Parameters.Exists("EPOCHS")) {
                maxEpochs = int.Parse(TestContext.Parameters["EPOCHS"]);
            }

            var originalCopy = areas
                .Select(x => new MapArea(x.Type, x.Size, x.Position, x.Tags))
                .ToList();

            var result = new DistributeResult {
                Log = log,
                VerboseOutput = TestContext.Parameters.Exists("VERBOSE"),
                MaxEpochs = maxEpochs,
                OriginalAreas = originalCopy,
                OriginalOverlapping =
                    originalCopy
                        .Where(
                            a => originalCopy.Any(b => a != b && a.Overlaps(b)))
                        .ToList(),
                OriginalOutOfBounds =
                    originalCopy
                        .Where(
                            area => !area.Fits(Vector.Zero2D, maze.Size))
                        .ToList(),
                PlacedAreas = new AreaDistributor(log)
                    .DistributePlacedRooms2(maze, areas.ToList(), maxEpochs)
            };

            result.PlacedOverlapping =
                result.PlacedAreas
                      .Where(a => result.PlacedAreas
                                        .Any(b => a != b && a.Overlaps(b)))
                      .ToList();
            result.PlacedOutOfBounds =
                result.PlacedAreas
                      .Where(block => !block.Fits(Vector.Zero2D, maze.Size))
                      .ToList();

            result.TestString = $"yield return \"{maze.Size}: " +
                String.Join(" ", result.OriginalAreas.Select(area =>
                    $"P{area.Position};S{area.Size}")) + "\"";

            if (result.VerboseOutput) {
                log.Buffered.Flush();
            }

            return result;
        }

        internal class DistributeResult {
            public Log Log { get; set; }
            public Vector MazeSize { get; set; }
            public List<MapArea> OriginalOutOfBounds { get; set; }
            public List<MapArea> OriginalOverlapping { get; set; }
            public List<MapArea> OriginalAreas { get; set; }
            public List<MapArea> PlacedOutOfBounds { get; set; }
            public List<MapArea> PlacedOverlapping { get; set; }
            public List<MapArea> PlacedAreas { get; set; }
            public int MaxEpochs { get; set; }
            public string TestString { get; set; }
            public bool VerboseOutput { get; internal set; }

            internal void AssertAllFit() {
                if (PlacedOutOfBounds.Count > 0 || PlacedOverlapping.Count > 0) {
                    Log?.Buffered.Flush();
                }
                Assert.IsEmpty(PlacedOutOfBounds,
                    "Out Of Bounds: " + String.Join(", ",
                        PlacedOutOfBounds.Select(area => $"P{area.Position};S{area.Size}")));
                Assert.IsEmpty(PlacedOverlapping,
                    "Overlapping: " + String.Join(", ",
                        PlacedOverlapping.Select(area => $"P{area.Position};S{area.Size}")));
            }
        }
    }
}