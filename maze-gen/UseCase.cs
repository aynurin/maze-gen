using System;
using System.Linq;
using CommandLine;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps {
    [Verb("usecase", HelpText = "Generate a random maze with the specified algorithm and size.")]
    class UseCaseCommand : BaseCommand {

        [Option('c', "case", Default = 1, Required = true, HelpText = "Use case number.")]
        public int Case { get; set; }

        override public int Run() {
            base.Run();
            var randomSource = RandomSource.CreateFromEnv();

            var maze = _useCases[Case](randomSource);

            // var maze = new GeneratedWorld(randomSource)
            //     .AddLayer(new Vector(3, 2))
            //     .OfMaze(typeof(HuntAndKillMazeGenerator))
            //     .ToMap(MazeToMapOptions.SquareCells(5, 2))
            //     .AddLayer(area => Area.CreateFrom(area,
            //         area.Select(cell =>
            //             cell.Tags.Contains(Cell.CellTag.MazeTrail) ?
            //                 new Cell(AreaType.Maze) :
            //                 new Cell(AreaType.None))))
            //     .OfMaze(typeof(HuntAndKillMazeGenerator))
            //     .ToMap(MazeToMapOptions.SquareCells(1, 1))
            // .AddAreas(
            //     new AreaType[] { AreaType.Hall, AreaType.Cave },
            //     new string[] { "hall", "lake" },
            //     /* areas count = */2,
            //     /* min size = */new Vector(2, 3),
            //     /* max size = */new Vector(5, 5))
            // .AddLayer().OfMaze()
            // .MarkDeadends()
            // .MarkLongestPath()
            // .ToMap()
            // .WithElevation()
            // .AddEnvironmentAreas(new string[] { "underground", "open" })
            ;

            Console.WriteLine(maze.ToString());
            Console.WriteLine(maze.RenderToString());
            return 0;
        }

        private readonly Func<RandomSource, Area>[] _useCases =
            new Func<RandomSource, Area>[] {
            r => new GeneratedWorld(r)
                .AddLayer(AreaType.Maze, new Vector(3, 2))
                .OfMaze(typeof(HuntAndKillMazeGenerator))
                .ToMap(MazeToMapOptions.SquareCells(5, 2))
                .AddLayer(area => Area.CreateFrom(area,
                    area.Select(cell =>
                        cell.Tags.Contains(Cell.CellTag.MazeTrail) ?
                            new Cell(AreaType.Maze) :
                            new Cell(AreaType.None))))
                .OfMaze(typeof(HuntAndKillMazeGenerator))
                .ToMap(MazeToMapOptions.SquareCells(1, 1))
                .Map(),
            r => new GeneratedWorld(r)
                .AddLayer(AreaType.Environment, new Vector(10, 10))
                .WithAreas(
                    Area.Create(new Vector(1, 2), new Vector(3, 6), AreaType.Maze),
                    Area.Create(new Vector(6, 2), new Vector(3, 6), AreaType.Maze))
                .OfMaze()
                // TODO: Can't use ToMap on a non-maze layer.
                .ToMap(MazeToMapOptions.SquareCells(1, 1))
                .Map()
            };
    }
}
