using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Renderers;

namespace PlayersWorlds.Maps.Areas.Evolving {
    internal class AreaDistributorHelper {
        internal static DistributeResult Distribute(
            Log log,
            Vector mapSize,
            IEnumerable<MapArea> areas,
            int maxEpochs = -1) {

            var debugLevel = 0;
            if (TestContext.Parameters.Exists("DEBUG")) {
                debugLevel = int.Parse(TestContext.Parameters["DEBUG"]);
            }
            if (TestContext.Parameters.Exists("EPOCHS")) {
                maxEpochs = int.Parse(TestContext.Parameters["EPOCHS"]);
            }

            var managedAreas = areas.ToList();
            var originalCopy = managedAreas
                .Select(x => x.ShallowCopy())
                .ToList();

            var result = new DistributeResult {
                Log = log,
                MaxEpochs = maxEpochs,
                OriginalAreas = originalCopy,
                OriginalOverlapping =
                    originalCopy
                        .Where(
                            a => originalCopy.Any(
                                    b => a != b &&
                                         a.Overlap(b).Area > 0))
                        .ToList(),
                OriginalOutOfBounds =
                    originalCopy
                        .Where(
                            area => !area.FitsInto(Vector.Zero2D, mapSize))
                        .ToList(),
                PlacedAreas = managedAreas
            };

            new AreaDistributor(
                debugLevel >= 4 ?
                    new MapAreaStringRenderer() :
                    null,
                debugLevel >= 5)
                .Distribute(mapSize, managedAreas, maxEpochs);

            result.PlacedOverlapping =
                result.PlacedAreas
                    .Where(a =>
                        result.PlacedAreas.Any(
                            b => a != b &&
                                 a.Overlap(b).Area > 0))
                    .ToList();
            result.PlacedOutOfBounds =
                result.PlacedAreas
                    .Where(block => !block.FitsInto(Vector.Zero2D, mapSize))
                    .ToList();

            result.TestString = $"yield return \"{mapSize}: " +
                string.Join(" ", result.OriginalAreas.Select(area =>
                    $"P{area.Position};S{area.Size}")) + "\"";

            if (debugLevel >= 4) {
                log.Buffered.Flush();
            }

            return result;
        }

        internal class DistributeResult {
            public Log Log { get; set; }
            public List<MapArea> OriginalOutOfBounds { get; set; }
            public List<MapArea> OriginalOverlapping { get; set; }
            public List<MapArea> OriginalAreas { get; set; }
            public List<MapArea> PlacedOutOfBounds { get; set; }
            public List<MapArea> PlacedOverlapping { get; set; }
            public List<MapArea> PlacedAreas { get; set; }
            public int MaxEpochs { get; set; }
            public string TestString { get; set; }

            internal void AssertAllFit() {
                if (PlacedOutOfBounds.Count > 0 || PlacedOverlapping.Count > 0) {
                    Log?.Buffered.Flush();
                }
                Assert.That(PlacedOutOfBounds, Is.Empty,
                    "Out Of Bounds: " + string.Join(", ",
                        PlacedOutOfBounds.Select(area => $"P{area.Position};S{area.Size}")));
                Assert.That(PlacedOverlapping, Is.Empty,
                    "Overlapping: " + string.Join(", ",
                        PlacedOverlapping.Select(area => $"P{area.Position};S{area.Size}")));
            }

            internal void AssertDoesNotFit() {
                if (PlacedOutOfBounds.Count > 0 || PlacedOverlapping.Count > 0) {
                    Log?.Buffered.Flush();
                }
                Assert.That(PlacedOutOfBounds.Concat(PlacedOverlapping), Is.Not.Empty,
                    "Out Of Bounds: " + string.Join(", ",
                        PlacedOutOfBounds.Select(area => $"P{area.Position};S{area.Size}") +
                    ". Overlapping: " + string.Join(", ",
                        PlacedOverlapping.Select(area => $"P{area.Position};S{area.Size}"))));
            }
        }
    }
}