using System;
using System.Linq;
using CommandLine;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps {

    [Verb("parse", HelpText = "Parse and render a maze from a string.")]
    class ParseCommand : BaseCommand {
        [Value(0, MetaName = "serialized maze", HelpText = "Serialized maze.")]
        public string SerializedMaze { get; set; }

        override public int Run() {
            base.Run();
            var maze = Maze2D.Parse(SerializedMaze);
            Console.WriteLine(maze.Serialize());
            Console.WriteLine(maze.ToString());
            Console.WriteLine($"Visited: " +
                maze.MazeCells.Count());
            Console.WriteLine($"Area Cells: ");
            Console.WriteLine("  Fill ({0}): ({1})",
                maze.MapAreas.Count(
                                a => a.Key.Type == AreaType.Fill),
                maze.MapAreas.Where(
                                a => a.Key.Type == AreaType.Fill)
                             .Select(a => a.Value.Count).Sum());
            Console.WriteLine("  Cave ({0}): ({1}): ",
                maze.MapAreas.Count(
                                a => a.Key.Type == AreaType.Cave),
                maze.MapAreas.Where(
                                a => a.Key.Type == AreaType.Cave)
                             .Select(a => a.Value.Count).Sum());
            Console.WriteLine("  Hall ({0}): ({1}): ",
                maze.MapAreas.Count(
                                a => a.Key.Type == AreaType.Hall),
                maze.MapAreas.Where(
                                a => a.Key.Type == AreaType.Hall)
                             .Select(a => a.Value.Count).Sum());
            Console.WriteLine("Unvisited cells: " +
                string.Join(",",
                    maze.Cells
                        .Where(c =>
                            !c.IsConnected &&
                            !maze.MapAreas.Any(
                                area => area.Value.Contains(c)))));
            return 0;
        }
    }
}