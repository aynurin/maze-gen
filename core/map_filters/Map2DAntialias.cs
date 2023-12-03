using System.Linq;

namespace Nour.Play.MapFilters {

    public class Map2DAntialias {
        private readonly string _cellType;
        private readonly string _antialiasType;
        private readonly int _antialiasWidth;
        private readonly int _antialiasHeight;

        public Map2DAntialias(string cellType,
                              string antialiasType,
                              int antialiasWidth = 2,
                              int antialiasHeight = 1) {
            _cellType = cellType;
            _antialiasType = antialiasType;
            _antialiasWidth = antialiasWidth;
            _antialiasHeight = antialiasHeight;
        }

        public void Render(Map2D map) {
            for (var y = 0; y < map.Size.Y; y++) {
                for (var x = 0; x < map.Size.X; x++) {
                    var cell = map.CellAt(new Vector(x, y));
                    if (cell.Tags.Contains(_cellType) ||
                        cell.Tags.Contains(_antialiasType)) continue;
                    var a = map.AnyCellsAt(
                                new Vector(x - _antialiasWidth, y),
                                new Vector(_antialiasWidth * 2 + 1, 1))
                               .Count(c => c.Tags.Contains(_cellType));
                    var b = map.AnyCellsAt(
                                new Vector(x, y - _antialiasHeight),
                                new Vector(1, _antialiasHeight * 2 + 1))
                               .Count(c => c.Tags.Contains(_cellType));
                    var setOutline = a > 0 && b > 0;
                    if (setOutline) {
                        if (cell.Tags.Contains(_cellType))
                            cell.Tags.Remove(_cellType);
                        cell.Tags.Add(_antialiasType);
                    }
                }
            }
        }
    }
}