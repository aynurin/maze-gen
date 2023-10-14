using System;
using System.Collections.Generic;

namespace Nour.Play.Areas {
    // ? The issue is that Halls can be entered, so they should act as cells,
    // ? and Fills can't be entered, so they are invisible to all algorithms. 
    // ? For Dijkstra, Halls should act as regular cells, but with increased a
    // ? passing complexity taking into account its cells.
    // Area size definition: Given that the mazes can be autogenerated and of
    // different sizes, we can't easily predefine areas counts and sizes, so we
    // might want to calculate areas based on some kind of heuristics. E.g., see
    // RandomZoneGenerator.
    // TODO: Create an area generator
    // TODO: Update generators to honor areas
    // TODO: Update Dijkstra to use hall sizes
    public class MapArea {
        public AreaType Type { get; private set; }
        public string[] Tags { get; private set; }
        public Vector RequestedDimensions { get; private set; }
        public List<Cell> Cells { get; private set; }

        public MapArea(AreaType type, Vector requestedDimensions, params string[] tags) {
            Cells = new List<Cell>(requestedDimensions.Area);
            Type = type;
            Tags = tags;
            RequestedDimensions = requestedDimensions;
        }
    }
}