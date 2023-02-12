var maze = new MazeGrid(4, 4);
new SidewinderMazeGenerator().Generate(maze);
Console.WriteLine(new MazeToAscii().Convert(maze));