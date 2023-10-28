using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nour.Play.Maze;
using Nour.Play.Maze.PostProcessing;

namespace Nour.Play.Renderers {
    public class Map2DAsciiRenderer {

        public string Render(Map2D map) {
            var buffer = new StringBuilder();
            for (int x = 0; x < map.Size.X; x++) {
                for (int y = 0; y < map.Size.Y; y++) {
                    buffer.Append(
                        map.CellAt(new Vector(x, y)).Tags.Contains(Maze2DToMap2DConverter.MAP2D_CELL_TYPE_WALL) ? "▓" :
                        map.CellAt(new Vector(x, y)).Tags.Contains(Maze2DToMap2DConverter.MAP2D_CELL_TYPE_TRAIL) ? "░" :
                        map.CellAt(new Vector(x, y)).Tags.Contains(Maze2DToMap2DConverter.MAP2D_CELL_TYPE_EDGE) ? "▒" :
                        map.CellAt(new Vector(x, y)).Tags.Contains(Maze2DToMap2DConverter.MAP2D_CELL_TYPE_VOID) ? " " :
                        "0");
                }
                buffer.Append("\n");
            }
            return buffer.ToString();
        }
    }
}