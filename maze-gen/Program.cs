using Nour.Play.Maze.Solvers;

namespace Nour.Play.Maze {
    class MainClass {
        public static void Main(string[] args) {
            var maze = MazeGenerator.Generate<WilsonsMazeGenerator>(new Vector(9, 18));
            var distances = DijkstraDistance.FindLongest(maze[0, 0]);
            System.Console.WriteLine(new MazeToAscii(maze).Convert(distances));
        }
    }
}
