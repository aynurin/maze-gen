using System;
using CommandLine;

namespace PlayersWorlds.Maps {
    class BaseCommand {
        [Option('r', "random", Required = false, HelpText = "Random seed.")]
        public int? RandomSeed { get; set; }

        [Option('d', "debug", Required = false, HelpText = "Debug level.")]
        public int? DebugLevel { get; set; }

        public virtual int Run() {
            if (RandomSeed.HasValue)
                RandomSource.EnvRandomSeed = RandomSeed.Value;
            if (DebugLevel.HasValue)
                Log.DebugLoggingLevel = TestLog.DebugLevel = DebugLevel.Value;
            return 0;
        }
    }
}
