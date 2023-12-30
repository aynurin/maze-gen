using System.Collections.Generic;
using PlayersWorlds.Maps.Renderers;

namespace PlayersWorlds.Maps.Areas.Evolving {
    public class AreaDistributor {
        private readonly Log _log;
        private readonly MapAreaLogRenderer _renderer;
        private readonly bool _verboseOutput;

        public AreaDistributor() : this(null, null, false) { }

        public AreaDistributor(Log log,
                               MapAreaLogRenderer renderer = null,
                               bool verboseOutput = false) {
            _log = log;
            _renderer = renderer;
            _verboseOutput = verboseOutput;
        }

        public void Distribute(Vector mapSize,
                               List<MapArea> areas,
                               int maxEpochs) {
            _renderer?.Draw(mapSize, areas);
            var simulator = new EvolvingSimulator(maxEpochs, 10);
            var system = new MapAreasSystem(_log, mapSize,
                areas,
                r => { },
                r => { if (_verboseOutput) _renderer?.Draw(mapSize, areas); });
            simulator.Evolve(system);
        }

        // TODO: change VectorD to Single
        // TODO: I still have X and Y all messed up.
    }
}