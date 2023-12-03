using System;
using System.Collections.Generic;
using System.Linq;

namespace Nour.Play.Maze {
    public class RecursiveBacktrackerMazeGenerator : MazeGenerator {
        override public void GenerateMaze(Maze2D layout, GeneratorOptions options) {
            Console.WriteLine("RecursiveBacktrackerMazeGenerator v0.1");
            Console.WriteLine($"Generating maze {layout.XWidthColumns}x{layout.YHeightRows}");

            var stack = new Stack<MazeCell>();
            stack.Push(layout.Cells.GetRandom());
            while (!IsFillComplete(options, layout) && stack.Count > 0) {
                var potentiallyNext = stack.First().Neighbors().Where(cell => !cell.IsVisited).ToList();
                if (potentiallyNext.Count > 0) {
                    var nextCell = potentiallyNext.GetRandom();
                    stack.First().Link(nextCell);
                    stack.Push(nextCell);
                } else {
                    stack.Pop();
                }
            }
        }
    }
}