using System;
using CommandLine;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps {
    [Verb("generate", HelpText = "Generate a random maze with the specified algorithm and size.")]
    class UseCaseCommand : BaseCommand {
        override public int Run() {
            base.Run();
            var randomSource = RandomSource.CreateFromEnv();

            var maze = new GeneratedWorld(randomSource)
                .AddLayer(new Vector(3, 3)).OfMaze()
                .Scale(new Vector(12, 12))
                .AddAreas(
                    new AreaType[] { AreaType.Hall, AreaType.Cave },
                    new string[] { "hall", "lake" },
                    /* areas count = */2,
                    /* min size = */new Vector(2, 3),
                    /* max size = */new Vector(5, 5))
                .AddLayer().OfMaze()
                .MarkDeadends()
                .MarkLongestPath()
                .ToMap()
                .WithElevation()
                .AddEnvironmentAreas(new string[] { "underground", "open" });

            Console.WriteLine(maze.Serialize());
            Console.WriteLine(maze.Map().ToString());
            return 0;
        }
    }
}
