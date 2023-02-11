public class MazeGen {
    public MazeGrid CreateMaze() {
        var maze = new MazeGrid(5, 10);
        maze.Randomize(new BinaryTreeMazeGenerator());
        return maze;
    }
}