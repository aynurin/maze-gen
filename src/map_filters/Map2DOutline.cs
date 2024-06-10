using System.Linq;

namespace PlayersWorlds.Maps.MapFilters {

    /// <summary>
    /// A <see cref="Area" /> filter that detects area edges and places another
    /// cell type around the edges.
    /// </summary>
    /// <remarks>
    /// Use this filter to create outlines around edges of specific cell type.
    /// E.g., to draw walls around trails.
    /// </remarks>
    public class Map2DOutline : Map2DFilter {
        private readonly Cell.CellTag[] _cellType;
        private readonly Cell.CellTag _outlineType;
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
        public Map2DOutline(Cell.CellTag[] cellType,
                            Cell.CellTag outlineType,
                            int outlineWidth,
                            int outlineHeight) {
            _cellType = cellType;
            _outlineType = outlineType;
            _outlineWidth = outlineWidth;
            _outlineHeight = outlineHeight;
        }

        /// <summary>
        /// Apply the filter to the specified <see cref="Area" />.
        /// </summary>
        /// <param name="map">The map to apply the filter to.</param>
        override public void Render(Area map) {
            for (var y = 0; y < map.Size.Y; y++) {
                for (var x = 0; x < map.Size.X; x++) {
                    var cell = new Vector(x, y);
                    if (map[cell].Tags.Any(t => _cellType.Contains(t)) ||
                        map[cell].Tags.Contains(_outlineType)) continue;
                    var setOutline =
                        map.Cells.SafeRegion(
                            new Vector(x - _outlineWidth, y - _outlineHeight),
                            new Vector(_outlineWidth * 2 + 1, _outlineHeight * 2 + 1))
                           .Any(c => c != cell &&
                                map[c].Tags.Any(t => _cellType.Contains(t)));
                    if (setOutline) {
                        foreach (var tag in _cellType)
                            if (map[cell].Tags.Contains(tag))
                                map[cell].Tags.Remove(tag);
                        map[cell].Tags.Add(_outlineType);
                    }

                }
            }
        }
    }
}