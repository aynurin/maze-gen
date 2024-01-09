using System.Linq;

namespace PlayersWorlds.Maps.MapFilters {

    /// <summary>
    /// A <see cref="Map2D" /> filter that detects area edges and places another
    /// cell type around the edges.
    /// </summary>
    /// <remarks>
    /// Use this filter to create outlines around edges of specific cell type.
    /// E.g., to draw walls around trails.
    /// </remarks>
    public class Map2DOutline : Map2DFilter {
        private readonly string[] _cellType;
        private readonly string _outlineType;
        private readonly int _outlineWidth;
        private readonly int _outlineHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="Map2DOutline" /> class.
        /// </summary>
        /// <param name="cellType">Type of cells around which to draw outline.
        /// </param>
        /// <param name="outlineType">Outline cell type</param>
        /// <param name="outlineWidth">Minimal width of the outline.</param>
        /// <param name="outlineHeight">Minimal height of the outline.</param>
        public Map2DOutline(string[] cellType,
                            string outlineType,
                            int outlineWidth,
                            int outlineHeight) {
            _cellType = cellType;
            _outlineType = outlineType;
            _outlineWidth = outlineWidth;
            _outlineHeight = outlineHeight;
        }

        /// <summary>
        /// Apply the filter to the specified <see cref="Map2D" />.
        /// </summary>
        /// <param name="map">The map to apply the filter to.</param>
        override public void Render(Map2D map) {
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