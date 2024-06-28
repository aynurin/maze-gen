using System;
using System.Text;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps.Renderers {
    /// <summary>
    /// Renders a map to an ASCII string.
    /// </summary>
    public class Map2DStringRenderer {
        /// <summary />
        public string Render(Area map) {
            var buffer = new char[map.Size.Area];
            RenderToBuffer(map, map, buffer, Vector.Zero(map.Position.Dimensions));
            var rendered = new StringBuilder(buffer.Length);
            for (var y = map.Size.Y - 1; y >= 0; y--) {
                for (var x = 0; x < map.Size.X; x++) {
                    rendered.Append(buffer[new Vector(x, y).ToIndex(map.Size)]);
                }
                rendered.AppendLine();
            }
            return rendered.ToString();
        }

        private void RenderToBuffer(Area map, Area rootMap, char[] buffer, Vector position) {
            for (var y = map.Size.Y - 1; y >= 0; y--) {
                for (var x = 0; x < map.Size.X; x++) {
                    var cellPosition = position + new Vector(x, y);
                    buffer[cellPosition.ToIndex(rootMap.Size)] =
                        map[cellPosition].Tags.Contains(Cell.CellTag.MazeVoid) ? ' ' :
                        map[cellPosition].Tags.Contains(Cell.CellTag.MazeWallCorner) ? '▒' :
                        map[cellPosition].Tags.Contains(Cell.CellTag.MazeWall) ? '▓' :
                        map[cellPosition].Tags.Contains(Cell.CellTag.MazeTrail) ? '░' :
                        '0';
                }
            }
            foreach (var childArea in map.ChildAreas()) {
                RenderToBuffer(childArea, rootMap, buffer, position + childArea.Position);
            }
        }
    }
}