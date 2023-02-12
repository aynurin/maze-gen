
public class BinaryTreeMazeGenerator : MazeGenerator {
    override public void Generate(MazeGrid maze) {
        var cellStates = System.Security.Cryptography.RandomNumberGenerator.GetBytes(maze.Size);
        for (int row = 0; row < maze.Rows; row++) {
            for (int col = 0; col < maze.Cols; col++) {
                var index = row * maze.Cols + col;

                var cell = maze[row, col];

                // link north
                if ((cellStates[index] % 2 == 0 || col == maze.Cols - 1) && row > 0) {
                    cell.Link(MazeCell.GatePosition.North, maze[row - 1, col]);
                }

                // link east
                if ((cellStates[index] % 2 == 1 || row == 0) && col < maze.Cols - 1) {
                    cell.Link(MazeCell.GatePosition.East, maze[row, col + 1]);
                }
            }
        }
    }
}