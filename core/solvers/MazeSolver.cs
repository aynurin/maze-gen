public abstract class MazeSolver {
    abstract public List<Solution> Solve(MazeGrid maze);
}

public class Solution {
    public int Complexity { get; private set; }
    public List<MazeCell> Steps { get; private set; }

    public Solution() {
        Steps = new List<MazeCell>();
    }
}