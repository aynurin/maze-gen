using System.Text;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps.Renderers {
    public class Map2DStringRenderer {

        public string Render(Map2D map) {
            var buffer = new StringBuilder();
            for (var y = map.Size.Y - 1; y >= 0; y--) {
                for (var x = 0; x < map.Size.X; x++) {
                    buffer.Append(
                        map.CellAt(new Vector(x, y)).Tags.Contains(Maze2DRenderer.MapCellType.Void) ? " " :
                        map.CellAt(new Vector(x, y)).Tags.Contains(Maze2DRenderer.MapCellType.Edge) ? "▒" :
                        map.CellAt(new Vector(x, y)).Tags.Contains(Maze2DRenderer.MapCellType.Wall) ? "▓" :
                        map.CellAt(new Vector(x, y)).Tags.Contains(Maze2DRenderer.MapCellType.Trail) ? "░" :
                        "0");
                }
                buffer.Append("\n");
            }
            return buffer.ToString();
        }
    }
}