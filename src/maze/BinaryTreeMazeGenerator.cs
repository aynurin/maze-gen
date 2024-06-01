using System;
using System.Diagnostics;
using System.Linq;
using static PlayersWorlds.Maps.Maze.GeneratorOptions;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Binary Tree algorithm implementation.
    /// </summary>
    public class BinaryTreeMazeGenerator : MazeGenerator {
        /// <summary>
        /// Generates a maze using Binary Tree algorithm in the specified
        /// layout.
        /// </summary>
        /// <param name="builder"><see cref="Maze2DBuilder" /> instance for
        /// the maze to be generated.</param>
        override public void GenerateMaze(Maze2DBuilder builder) {
            builder.ThrowIfIncompatibleOptions(new GeneratorOptions() {
                FillFactor = MazeFillFactor.Full,
            });
            var states = builder.Random.NextBytes(builder.AllCells.Count);
            var i = 0;
            foreach (var currentCell in builder.AllCells) {
                var linkNorth = states[i++] % 2 == 0;
                var canConnectEast = builder.CanConnect(currentCell, currentCell.Position + Vector.East2D);
                var canConnectNorth = builder.CanConnect(currentCell, currentCell.Position + Vector.North2D);
                Cell cellToLink = null;
                if ((linkNorth || !canConnectEast) && canConnectNorth) {
                    cellToLink = builder.MazeArea[currentCell.Position + Vector.North2D];
                } else if (canConnectEast) {
                    cellToLink = builder.MazeArea[currentCell.Position + Vector.East2D];
                }

                if (cellToLink == null && !builder.MazeArea.CellHasLinks(currentCell.Position)) {
                    // This link is not connected, and won't be connected
                    // because of this maze geometry. Let's connect it to
                    // any other cell so that it's not left out.
                    if (!builder.TryPickRandomNeighbor(
                            currentCell, out cellToLink)) {
                        // this cell doesn't have any neighbors we can
                        // connect. Perhaps it is surrounded by walls,
                        // filled areas, or halls.
                        Trace.TraceWarning(
                            $"BinaryTreeMazeGenerator could not find any " +
                            $"neighbors to connect {currentCell} to.");
                    }
                }
                if (cellToLink != null) {
                    builder.Connect(currentCell.Position, cellToLink.Position);
                }
            }
        }
    }
}