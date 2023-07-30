using System;
using System.Collections.Generic;
using System.Linq;

public class WilsonsMazeGenerator : MazeGenerator {
    override public void Generate(MazeGrid maze) {
        Console.WriteLine("WilsonsMazeGenerator v0.1");
        Console.WriteLine($"Generating maze {maze.Rows}x{maze.Cols}");

        var currentCell = maze.Cells.GetRandom();
        var visitedCells = new HashSet<MazeCell>() { currentCell };

        while (visitedCells.Count < maze.Cells.Count) {
            var walkPath = new List<MazeCell>();

            var nextCell = maze.Cells.GetRandom();

            if (visitedCells.Contains(nextCell))
                continue;

            while (!visitedCells.Contains(nextCell)) {
                walkPath.Add(nextCell);
                var containsAt = walkPath.IndexOf(nextCell);
                if (containsAt >= 0)
                    walkPath.RemoveRange(containsAt + 1, walkPath.Count - containsAt - 1);
                nextCell = nextCell.Neighbors.GetRandom();
            };

            walkPath.Add(nextCell);

            for (int i = 0; i < walkPath.Count - 1; i++) {
                walkPath[i].Link(walkPath[i + 1]);
                visitedCells.Add(walkPath[i]);
            }
        }
    }
}