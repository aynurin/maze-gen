
public class BinaryTreeMazeGenerator : MazeGenerator {
    override public void Generate(MazeGrid maze) {
        Console.WriteLine("BinaryTree v0.1");
        Console.WriteLine($"Generating maze {maze.Rows}x{maze.Cols}");
        var cellStates = System.Security.Cryptography.RandomNumberGenerator.GetBytes(maze.Size);
        for (int row = 0; row < maze.Rows; row++) {
            for (int col = 0; col < maze.Cols; col++) {
                var index = row * maze.Cols + col;

                var cell = maze[row, col];

                // link north
                if ((cellStates[index] % 2 == 0 || col == maze.Cols - 1) && row > 0) {
                    cell.Link(MazeCell.GatePosition.North, maze[row - 1, col]);
                    maze[row - 1, col].Link(MazeCell.GatePosition.South, cell);
                }

                // link east
                if ((cellStates[index] % 2 == 1 || row == 0) && col < maze.Cols - 1) {
                    cell.Link(MazeCell.GatePosition.East, maze[row, col + 1]);
                    maze[row, col + 1].Link(MazeCell.GatePosition.West, cell);
                }
            }
        }
    }
}