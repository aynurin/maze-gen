using System;

public class WilsonsMazeGenerator : MazeGenerator {
    override public void Generate(MazeGrid maze) {
        Console.WriteLine("WilsonsMazeGenerator v0.1");
        Console.WriteLine($"Generating maze {maze.Rows}x{maze.Cols}");

        throw new NotImplementedException();
    }
}