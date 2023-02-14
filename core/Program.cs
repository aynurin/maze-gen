var maze = new MazeGrid(10, 10);
new SidewinderMazeGenerator().Generate(maze);
Console.WriteLine(new MazeToAscii(maze).Convert());