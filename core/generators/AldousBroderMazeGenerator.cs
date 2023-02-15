
public class AldousBroderMazeGenerator : MazeGenerator {
    override public void Generate(MazeGrid maze) {
        Console.WriteLine("AldousBroderMazeGenerator v0.1");
        Console.WriteLine($"Generating maze {maze.Rows}x{maze.Cols}");

        var rnd = new Random();
        var currentCell = maze[rnd.Next(maze.Rows), rnd.Next(maze.Cols)];
        var visitedCells = new HashSet<MazeCell>() { currentCell };
        while (visitedCells.Count < maze.Rows * maze.Cols) {
            var next = currentCell.Neighbours[rnd.Next(currentCell.Neighbours.Count)];
            if (!visitedCells.Contains(next)) {
                currentCell.Link(next);
                visitedCells.Add(next);
            }
            currentCell = next;
        }
    }
}