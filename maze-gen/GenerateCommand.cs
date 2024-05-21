using System;
using CommandLine;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps {
    [Verb("generate", HelpText = "Generate a random maze with the specified algorithm and size.")]
    class GenerateCommand : BaseCommand {
        [Option('a', "algo", Default = "AldousBroder", Required = false, HelpText = "Maze generation algorithm.")]
        public string AlgorithmName { get; set; }
        [Option('s', "size", Default = "10x10", Required = false, HelpText = "Maze size, e.g. 3x4.")]
        public string MazeSize { get; set; }

        override public int Run() {
            base.Run();
            var size = Vector.Parse(MazeSize);
            var randomSource = RandomSource.CreateFromEnv();
            var generatorOptions = new GeneratorOptions() {
                Algorithm = Type.GetType(
                    "PlayersWorlds.Maps.Maze." + AlgorithmName +
                    "MazeGenerator, PlayersWorlds.Maps"),
                FillFactor = GeneratorOptions.FillFactorOption.Full,
                MapAreasOptions = GeneratorOptions.MapAreaOptions.Auto,
                RandomSource = randomSource,
                AreaGenerator = new RandomAreaGenerator(
                    new RandomAreaGenerator.RandomAreaGeneratorSettings(
                        randomSource))
            };
            var maze = MazeGenerator.Generate(size, generatorOptions);
            Console.WriteLine(maze.Serialize());
            Console.WriteLine(maze.ToString());
            Console.WriteLine(maze.ToMap(MazeToMapOptions.RectCells(2, 1)).ToString());
            return 0;
        }
    }
}
