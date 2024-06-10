using System;
using System.Linq;
using CommandLine;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Serializer;

namespace PlayersWorlds.Maps {
    [Verb("parse", HelpText = "Parse and render a maze from a string.")]
    class ParseCommand : BaseCommand {
        [Value(0, MetaName = "serialized maze", HelpText = "Serialized maze.")]
        public string SerializedMaze { get; set; }

        override public int Run() {
            base.Run();
            var maze = LegacyAreaSerializer.ParseV01MazeString(SerializedMaze);
            var mazeCells = maze.Cells.Where(c => maze.CellHasLinks(c)).ToList();
            var areaSerializer = new AreaSerializer();
            Console.WriteLine(areaSerializer.Serialize(maze));
            Console.WriteLine(maze.ToString());
            Console.WriteLine($"Visited: " +
                mazeCells.Count());
            Console.WriteLine($"Area Cells: ");
            Console.WriteLine("  Fill ({0}): ({1})",
                maze.ChildAreas().Count(
                                a => a.Type == AreaType.Fill),
                maze.ChildAreas().Where(
                                a => a.Type == AreaType.Fill)
                             .Select(a => a.Cells.Count).Sum());
            Console.WriteLine("  Cave ({0}): ({1}): ",
                maze.ChildAreas().Count(
                                a => a.Type == AreaType.Cave),
                maze.ChildAreas().Where(
                                a => a.Type == AreaType.Cave)
                             .Select(a => a.Cells.Count).Sum());
            Console.WriteLine("  Hall ({0}): ({1}): ",
                maze.ChildAreas().Count(
                                a => a.Type == AreaType.Hall),
                maze.ChildAreas().Where(
                                a => a.Type == AreaType.Hall)
                             .Select(a => a.Cells.Count).Sum());
            Console.WriteLine(
                "Unvisited cells: " +
                string.Join(",", maze.Cells
                    .Where(c => !maze.CellHasLinks(c))));
            Console.WriteLine(maze.MazeToString());
            return 0;
        }
    }
}