using System;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Binary Tree algorithm implementation.
    /// </summary>
    public class BinaryTreeMazeGenerator : MazeGenerator {
        /// <summary>
        /// Generates a maze using Binary Tree algorithm in the specified
        /// layout.
        /// </summary>
        /// <param name="layout">The layout to generate the maze in.</param>
        /// <param name="options">The generator options to use.</param>
        override public void GenerateMaze(Maze2D layout, GeneratorOptions options) {
            if (options.FillFactor != GeneratorOptions.FillFactorOption.Full) {
                throw new ArgumentException(this.GetType().Name + " doesn't currently " +
                    "support fill factors other than Full");
            }
            var states = GlobalRandom.NextBytes(layout.Area);
            for (var i = 0; i < layout.VisitableCells.Count; i++) {
                var cell = layout.VisitableCells[i];
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