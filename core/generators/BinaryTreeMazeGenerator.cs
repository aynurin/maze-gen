using System;
using System.Collections.Generic;

namespace Nour.Play.Maze {
    public class BinaryTreeMazeGenerator : MazeGenerator {
        override public MazeGrid Generate(MazeLayout layout) {
            var maze = new MazeGrid(layout.Size);

            Console.WriteLine("BinaryTree v0.1");
            Console.WriteLine($"Generating maze {maze.Rows}x{maze.Cols}");
            var cellStates = GetRandomBytes(maze.Size);
            for (int row = 0; row < maze.Rows; row++) {
                for (int col = 0; col < maze.Cols; col++) {
                    var index = row * maze.Cols + col;

                    var cell = maze[row, col];

                    // link north
                    if ((cellStates[index] % 2 == 0 || col == maze.Cols - 1) && row > 0) {
                        cell.Link(maze[row - 1, col]);
                    }

                    // link east
                    if ((cellStates[index] % 2 == 1 || row == 0) && col < maze.Cols - 1) {
                        cell.Link(maze[row, col + 1]);
                    }
                }
            }

            return maze;
        }
    }
}