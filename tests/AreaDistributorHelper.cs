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
                .Select(x => new MapArea(x.Type, x.Size, x.Position, x.Tags))
                .ToList();

            var result = new DistributeResult {
                Log = log,
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
                            area => !area.Fits(Vector.Zero2D, mapSize))
                        .ToList(),
                PlacedAreas = managedAreas
            };

            new AreaDistributor(
                log,
                debugLevel >= 4 ?
                    new MapAreaLogRenderer(log) :
                    null,
                debugLevel >= 5)
                .Distribute(mapSize, managedAreas, maxEpochs);

            result.PlacedOverlapping =
                result.PlacedAreas
                    .Where(a =>
                        result.PlacedAreas.Any(b => a != b && a.Overlaps(b)))
                    .ToList();
            result.PlacedOutOfBounds =
                result.PlacedAreas
                    .Where(block => !block.Fits(Vector.Zero2D, mapSize))
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
                Assert.IsEmpty(PlacedOutOfBounds,
                    "Out Of Bounds: " + string.Join(", ",
                        PlacedOutOfBounds.Select(area => $"P{area.Position};S{area.Size}")));
                Assert.IsEmpty(PlacedOverlapping,
                    "Overlapping: " + string.Join(", ",
                        PlacedOverlapping.Select(area => $"P{area.Position};S{area.Size}")));
            }
        }
    }
}