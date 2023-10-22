using System;
using System.Collections.Generic;

namespace Nour.Play.Maze {
    public class BinaryTreeMazeGenerator : MazeGenerator {
        override public void GenerateMaze(Maze2D layout) {
            Console.WriteLine("BinaryTree v0.1");
            Console.WriteLine($"Generating maze {layout.XHeightRows}x{layout.YWidthColumns}");
            var cellStates = GlobalRandom.NextBytes(layout.Area);
            for (int x = 0; x < layout.XHeightRows; x++) {
                for (int y = 0; y < layout.YWidthColumns; y++) {
                    var index = x * layout.YWidthColumns + y;

                    var cell = layout[x, y];

                    // link north
                    if ((cellStates[index] % 2 == 0 || y == layout.YWidthColumns - 1) && x > 0) {
                        cell.Link(layout[x - 1, y]);
                    }

                    // link east
                    if ((cellStates[index] % 2 == 1 || x == 0) && y < layout.YWidthColumns - 1) {
                        cell.Link(layout[x, y + 1]);
                    }
                }
            }
        }
    }
}