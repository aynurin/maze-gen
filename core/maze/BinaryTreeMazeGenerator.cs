using System;

namespace Nour.Play.Maze {
    public class BinaryTreeMazeGenerator : MazeGenerator {
        override public void GenerateMaze(Maze2D layout, GeneratorOptions options) {
            Console.WriteLine("BinaryTree v0.1");
            Console.WriteLine($"Generating maze {layout.XHeightRows}x{layout.YWidthColumns}");
            if (options.FillFactor != GeneratorOptions.FillFactorOption.Full) {
                throw new ArgumentException(this.GetType().Name + " doesn't currently " +
                    "support fill factors other than Full");
            }
            var states = GlobalRandom.NextBytes(layout.Area);
            for (var i = 0; i < layout.Cells.Count; i++) {
                var cell = layout.Cells[i];
                var linkNorth = states[i] % 2 == 0;
                // link north
                if ((linkNorth || !cell.Neighbors(Vector.East2D).HasValue) && cell.Neighbors(Vector.North2D).HasValue) {
                    cell.Link(cell.Neighbors(Vector.North2D).Value);
                }

                // link east
                if ((!linkNorth || !cell.Neighbors(Vector.North2D).HasValue) && cell.Neighbors(Vector.East2D).HasValue) {
                    cell.Link(cell.Neighbors(Vector.East2D).Value);
                }
            }
        }
    }
}