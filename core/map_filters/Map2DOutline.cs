using System.Linq;

namespace Nour.Play.MapFilters {

    public class Map2DOutline {
        private readonly string[] _cellType;
        private readonly string _outlineType;
        private readonly int _outlineWidth;
        private readonly int _outlineHeight;

        public Map2DOutline(string[] cellType,
                            string outlineType,
                            int outlineWidth,
                            int outlineHeight) {
            _cellType = cellType;
            _outlineType = outlineType;
            _outlineWidth = outlineWidth;
            _outlineHeight = outlineHeight;
        }

        public void Render(Map2D map) {
            for (var y = 0; y < map.Size.Y; y++) {
                for (var x = 0; x < map.Size.X; x++) {
                    var cell = map.CellAt(new Vector(x, y));
                    if (cell.Tags.Any(t => _cellType.Contains(t)) ||
                        cell.Tags.Contains(_outlineType)) continue;
                    var setOutline =
                        map.AnyCellsAt(
                            new Vector(x - _outlineWidth, y - _outlineHeight),
                            new Vector(_outlineWidth * 2 + 1, _outlineHeight * 2 + 1))
                           .Any(c => c != cell &&
                                c.Tags.Any(t => _cellType.Contains(t)));
                    if (setOutline) {
                        foreach (var tag in _cellType)
                            if (cell.Tags.Contains(tag))
                                cell.Tags.Remove(tag);
                        cell.Tags.Add(_outlineType);
                    }

                }
            }
        }
    }
}