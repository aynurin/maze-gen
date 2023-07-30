using System;
using System.Collections.Generic;

public class AldousBroderMazeGenerator : MazeGenerator {
    override public void Generate(MazeGrid maze) {
        Console.WriteLine("AldousBroderMazeGenerator v0.1");
        Console.WriteLine($"Generating maze {maze.Rows}x{maze.Cols}");

        var currentCell = maze.Cells.GetRandom();
        var visitedCells = new HashSet<MazeCell>() { currentCell };
        while (visitedCells.Count < maze.Rows * maze.Cols) {
            var next = currentCell.Neighbors.GetRandom();
            if (!visitedCells.Contains(next)) {
                currentCell.Link(next);
                visitedCells.Add(next);
            }
            currentCell = next;
        }
    }
}