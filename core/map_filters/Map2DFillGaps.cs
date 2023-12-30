using System.Linq;

namespace PlayersWorlds.Maps.MapFilters {

    internal class Map2DFillGaps {
        private readonly string[] _gapsType;
        private readonly bool _includeVoids;
        private readonly string _fillType;
        private readonly int _maxGapWidth;
        private readonly int _maxGapHeight;

        public Map2DFillGaps(string[] gapsType,
                             bool includeVoids,
                             string fillType,
                             int maxGapWidth,
                             int maxGapHeight) {
            _gapsType = gapsType;
            _includeVoids = includeVoids;
            _fillType = fillType;
            _maxGapWidth = maxGapWidth;
            _maxGapHeight = maxGapHeight;
        }

        public void Render(Map2D map) {
            for (var y = 0; y < map.Size.Y; y++) {
                for (var x = 0; x < map.Size.X; x++) {
                    var cell = map.CellAt(new Vector(x, y));
                    if (CellIsGap(cell)) {
                        // if this is a gap, and there is a gap to the x-1 or
                        // x+1 direction, then we leave it as is.
                        if (x > 0 && CellIsGap(map.CellAt(new Vector(x - 1, y))))
                            continue;
                        if (y > 0 && CellIsGap(map.CellAt(new Vector(x, y - 1))))
                            continue;
                        var a = map.AnyCellsAt(
                                    new Vector(x, y),
                                    new Vector(_maxGapWidth, 1))
                                   .All(c => CellIsGap(c));
                        var b = map.AnyCellsAt(
                                    new Vector(x, y),
                                    new Vector(1, _maxGapHeight))
                                   .All(c => CellIsGap(c));
                        if (a || b) continue;
                        foreach (var tag in _gapsType)
                            if (cell.Tags.Contains(tag))
                                cell.Tags.Remove(tag);
                        cell.Tags.Add(_fillType);
                    }
                }
            }
        }

        private bool CellIsGap(Cell cell) =>
            _includeVoids && cell.Tags.Count() == 0 ||
            cell.Tags.Any(t => _gapsType.Contains(t));
    }
}