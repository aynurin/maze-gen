
public class SidewinderMazeGenerator : MazeGenerator {
    override public void Generate(MazeGrid maze) {
        Console.WriteLine("Sidewinder v0.1");
        Console.WriteLine($"Generating maze {maze.Rows}x{maze.Cols}");
        var cellStates = System.Security.Cryptography.RandomNumberGenerator.GetBytes(maze.Size);
        for (int row = 0; row < maze.Rows; row++) {
            var run = new List<MazeCell>();
            for (int col = 0; col < maze.Cols; col++) {
                var index = row * maze.Cols + col;

                var cell = maze[row, col];

                run.Add(cell);

                // link north
                if ((cellStates[index] % 2 == 0 || col == maze.Cols - 1) && row > 0) {
                    var member = run[cellStates[index] % run.Count];
                    member.Link(MazeCell.GatePosition.North, maze[row - 1, member.Col]);
                    maze[row - 1, member.Col].Link(MazeCell.GatePosition.South, member);
                    run.Clear();
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