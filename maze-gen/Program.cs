// See https://aka.ms/new-console-template for more information
var maze = new MazeGrid(9, 18);
new AldousBroderMazeGenerator().Generate(maze);
var distances = DijkstraDistances.FindLongest(maze[0, 0]);
System.Console.WriteLine(new MazeToAscii(maze).Convert(distances));