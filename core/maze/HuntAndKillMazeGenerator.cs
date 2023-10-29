using System;
using System.Collections.Generic;
using System.Linq;

namespace Nour.Play.Maze {
    public class HuntAndKillMazeGenerator : MazeGenerator {
        override public void GenerateMaze(Maze2D layout, GeneratorOptions options) {
            Console.WriteLine("HuntAndKillMazeGenerator v0.1");
            Console.WriteLine($"Generating maze {layout.XHeightRows}x{layout.YWidthColumns}");

            var currentCell = layout.Cells.GetRandom();
            while (!IsFillComplete(options, layout)) {
                var potentiallyNext = currentCell.Neighbors().Where(cell => !cell.IsVisited).ToList();
                if (potentiallyNext.Count > 0) {
                    var nextCell = potentiallyNext.GetRandom();
                    currentCell.Link(nextCell);
                    currentCell = nextCell;
                } else {
                    foreach (var hunt in layout.Cells) {
                        if (!hunt.IsVisited && hunt.Neighbors().Any(cell => cell.IsVisited)) {
                            currentCell = hunt;
                            currentCell.Link(hunt.Neighbors().Where(cell => cell.IsVisited).First());
                            break;
                        }
                    }
                }
            }
        }
    }
}