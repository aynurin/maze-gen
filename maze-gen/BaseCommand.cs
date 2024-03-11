using CommandLine;

namespace PlayersWorlds.Maps {
    class BaseCommand {
        [Option('r', "random", Required = false, HelpText = "Random seed.")]
        public int? RandomSeed { get; set; }

        public virtual int Run() {
            if (RandomSeed.HasValue)
                GlobalRandom.Reseed(RandomSeed.Value);
            return 0;
        }
    }
}