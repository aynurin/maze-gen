using System;
using System.Collections.Generic;

namespace Nour.Play.Maze {
    public class SidewinderMazeGenerator : MazeGenerator {
        override public void GenerateMaze(Maze2D layout) {
            Console.WriteLine("Sidewinder v0.1");
            Console.WriteLine($"Generating maze {layout.XHeightRows}x{layout.YWidthColumns}");
            var cellStates = GlobalRandom.NextBytes(layout.Area);
            for (int x = 0; x < layout.XHeightRows; x++) {
                var run = new List<MazeCell>();
                for (int y = 0; y < layout.YWidthColumns; y++) {
                    var index = x * layout.YWidthColumns + y;

                    var cell = layout[x, y];

                    run.Add(cell);

                    // link north
                    if ((cellStates[index] % 2 == 0 || y == layout.YWidthColumns - 1) && x > 0) {
                        var member = run[cellStates[index] % run.Count];
                        member.Link(layout[x - 1, member.Y]);
                        run.Clear();
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