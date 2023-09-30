
using System.Collections.Generic;

namespace Nour.Play.Maze {
    internal class MazeLayoutManager {
        private readonly Size _mazeSize;
        private readonly IEnumerable<MazeZone> _mazeZones;

        public MazeLayoutManager(Size size, IEnumerable<MazeZone> mazeZones) {
            _mazeSize = size;
            _mazeZones = mazeZones;
        }

        public IDictionary<Point, MazeZone> GenerateZones() {
            var createdZones = new Dictionary<Point, MazeZone>();
            var placeMore = true;
            var zoneEnumerator = _mazeZones.GetEnumerator();
            while (placeMore && zoneEnumerator.MoveNext()) {
                var placement = PlaceZone(_mazeSize, createdZones, zoneEnumerator.Current);
                if (placement == Point.Empty)
                    placeMore = false;
            }
            return createdZones;
        }

        private Point PlaceZone(
            Size mazeSize, Dictionary<Point, MazeZone> existingZones, MazeZone newZone) {

            var placeableAreas = new Queue<PlaceableArea>();
            placeableAreas.Enqueue(new PlaceableArea(new Point(0, 0), mazeSize));
            foreach (var existingZone in existingZones) {
                var existingZoneArea = new PlaceableArea(existingZone.Key, existingZone.Value.Size);
                PlaceableArea placeableArea;
                while (placeableAreas.TryDequeue(out placeableArea)) {
                    var newAreas = placeableArea.Slice(existingZoneArea);
                    foreach (var newArea in newAreas)
                        placeableAreas.Enqueue(newArea);
                    if (placeableArea.Contains(existingZoneArea)) break;
                    // slice will yield either of four results:
                    // 0 areas - this placeable area was fully consumed by the given zone
                    // 1 area == existing area - this area was not impacted by the given zone
                    // 1 area != existing area - this area was shrunk by the given zone
                    // 2+ areas - this area was sliced by the given zone
                    // whatever the result, we should add the new areas to the queue
                    // to check other zones.
                    // if the fits parameter is true, we are sone with this Zone and
                    // should break to continue checking the other zones.
                }
            }

            // placeableAreas will contain all unoccupied areas we can use to try
            // and place the new Zone
            var fittingAreas = new List<PlaceableArea>();
            foreach (var placeableArea in placeableAreas) {
                if (placeableArea.Fits(newZone.Size))
                    fittingAreas.Add(placeableArea);
            }

            // TODO: Select random fitting coords within a random area
            if (fittingAreas.Count > 0)
                return fittingAreas.GetRandom().Position;
            else return Point.Empty;
        }

        // layout interface:

        // cell interface:
        // getAllNeighbors
        // getAllNeighbors(Side)
        // getAllLinks
        // getAllLinks(Side)
    }
}