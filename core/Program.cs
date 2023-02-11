var mazeGen = new MazeGen();
var maze = mazeGen.CreateMaze();
Console.WriteLine(new MazeToAscii().Convert(maze));