var maze = new MazeGrid(4, 4);
new BinaryTreeMazeGenerator().Generate(maze);
Console.WriteLine(new MazeToAscii().Convert(maze));