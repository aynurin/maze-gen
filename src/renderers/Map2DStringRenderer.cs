using System.Text;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps.Renderers {
    /// <summary>
    /// Renders a map to an ASCII string.
    /// </summary>
    public class Map2DStringRenderer {
        /// <summary />
        public string Render(Area map) {
            var buffer = new StringBuilder();
            for (var y = map.Size.Y - 1; y >= 0; y--) {
                for (var x = 0; x < map.Size.X; x++) {
                    buffer.Append(
                        map[new Vector(x, y)].Tags.Contains(Cell.CellTag.MazeVoid) ? " " :
                        map[new Vector(x, y)].Tags.Contains(Cell.CellTag.MazeWallCorner) ? "▒" :
                        map[new Vector(x, y)].Tags.Contains(Cell.CellTag.MazeWall) ? "▓" :
                        map[new Vector(x, y)].Tags.Contains(Cell.CellTag.MazeTrail) ? "░" :
                        "0");
                }
                buffer.Append("\n");
            }
            return buffer.ToString();
        }
    }
}