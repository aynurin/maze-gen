using System;
using System.Collections.Generic;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Sidewinder algorithm implementation.
    /// </summary>
    public class SidewinderMazeGenerator : MazeGenerator {
        /// <summary>
        /// Generates a maze using Sidewinder algorithm in the
        /// specified layout.
        /// </summary>
        /// <param name="layout">The layout to generate the maze in.</param>
        /// <param name="options">The generator options to use.</param>
        override public void GenerateMaze(Maze2D layout, GeneratorOptions options) {
            if (options.FillFactor != GeneratorOptions.FillFactorOption.Full) {
                throw new ArgumentException(this.GetType().Name + " doesn't currently " +
                    "support fill factors other than Full");
            }
            var cellStates = GlobalRandom.NextBytes(layout.Area);
            var currentX = 0;
            var run = new List<MazeCell>();
            for (var i = 0; i < layout.UnlinkedCells.Count; i++) {
                var cell = layout.UnlinkedCells[i];
                var linkNorth = cellStates[i] % 2 == 0;
                if (cell.X != currentX) {
                    run.Clear();
                    currentX = cell.X;
                }
                run.Add(cell);

                // link north
                if (linkNorth || !cell.Neighbors(Vector.East2D).HasValue) {
                    var member = run[cellStates[i] % run.Count];
                    if (member.Neighbors(Vector.North2D).HasValue) {
                        member.Link(member.Neighbors(Vector.North2D).Value);
                        run.Clear();
                    }
                }

                // link east
                if ((!linkNorth || !cell.Neighbors(Vector.North2D).HasValue) && cell.Neighbors(Vector.East2D).HasValue) {
                    cell.Link(cell.Neighbors(Vector.East2D).Value);
                }
            }
        }
    }
}