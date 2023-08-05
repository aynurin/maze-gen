using System;

namespace Nour.Play.Maze {
    class MainClass
    {
        public static void Main(string[] args)
        {
            var maze = new WilsonsMazeGenerator().Generate(MazeLayout.GenerateRandom(9, 18));
            var distances = DijkstraDistances.FindLongest(maze[0, 0]);
            System.Console.WriteLine(new MazeToAscii(maze).Convert(distances));
        }
    }
}
