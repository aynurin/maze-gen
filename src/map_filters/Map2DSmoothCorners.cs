using System.Linq;

namespace PlayersWorlds.Maps.MapFilters {

    /// <summary>
    /// A <see cref="Area" /> filter that detects corner cells to allow placing
    /// different types of cells in the corners.
    /// </summary>
    /// <remarks>
    /// Use this filter to mark the corners of different areas on the
    /// map to allow easier automated placement of map objects in corners.
    /// </remarks>
    public class Map2DSmoothCorners : Map2DFilter {
        private readonly Cell.CellTag _cellType;
        private readonly Cell.CellTag _cornerType;
        private readonly int _cornerWidth;
        private readonly int _cornerHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="Map2DSmoothCorners" />
        /// class.
        /// </summary>
        /// <param name="cellType">The type of cells where to look for
        /// corners.</param>
        /// <param name="cornerType">The type of cells to put in the corners.
        /// </param>
        /// <param name="cornerWidth">Minimal width of the corner cells block.
        /// </param>
        /// <param name="cornerHeight">Minimal height of the corner cells block.
        /// </param>
        public Map2DSmoothCorners(Cell.CellTag cellType,
                              Cell.CellTag cornerType,
                              int cornerWidth = 2,
                              int cornerHeight = 1) {
            _cellType = cellType;
            _cornerType = cornerType;
            _cornerWidth = cornerWidth;
            _cornerHeight = cornerHeight;
        }

        /// <summary>
        /// Apply the filter to the specified <see cref="Area" />.
        /// </summary>
        /// <param name="map">The map to apply the filter to.</param>
        override public void Render(Area map) {
            for (var y = 0; y < map.Size.Y; y++) {
                for (var x = 0; x < map.Size.X; x++) {
                    var cell = map[new Vector(x, y)];
                    if (cell.Tags.Contains(_cellType) ||
                        cell.Tags.Contains(_cornerType)) continue;
                    var a = map.Cells.SafeRegion(
                                new Vector(x - _cornerWidth, y),
                                new Vector(_cornerWidth * 2 + 1, 1))
                               .Count(c => map[c].Tags.Contains(_cellType));
                    var b = map.Cells.SafeRegion(
                                new Vector(x, y - _cornerHeight),
                                new Vector(1, _cornerHeight * 2 + 1))
                               .Count(c => map[c].Tags.Contains(_cellType));
                    var setOutline = a > 0 && b > 0;
                    if (setOutline) {
                        if (cell.Tags.Contains(_cellType))
                            cell.Tags.Remove(_cellType);
                        cell.Tags.Add(_cornerType);
                    }
                }
            }
        }
    }
}