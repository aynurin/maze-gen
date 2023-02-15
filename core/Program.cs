var maze = new MazeGrid(10, 10);
new SidewinderMazeGenerator().Generate(maze);
var distances = DijkstraDistances.Find(maze[0, 0]);
Console.WriteLine(new MazeToAscii(maze).Convert(distances));