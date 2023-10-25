namespace Nour.Play.Maze {
    class MainClass {
        public static void Main(string[] args) {
            var maze = MazeGenerator.Generate<WilsonsMazeGenerator>(new Vector(9, 18));
            System.Console.WriteLine(new MazeToAscii(maze).WithTrail());
        }
    }
}
