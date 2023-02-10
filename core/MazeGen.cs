public class MazeGen {
    public MazeDefinition CreateMaze() {
        var maze = new MazeDefinition(10, 10);
        maze.Randomize();
        return maze;
    }
}