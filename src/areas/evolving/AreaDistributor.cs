using System;
using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps.Renderers;

namespace PlayersWorlds.Maps.Areas.Evolving {
    /// <summary>
    /// Distributes objects in an area. Uses the <see cref="EvolvingSimulator"/>
    /// to evolve a <see cref="MapAreasSystem" /> resulting in an evolution
    /// similar to <a
    /// href="https://en.wikipedia.org/wiki/Force-directed_graph_drawing"
    /// title="Force-directed graph drawing on Wikipedia">
    /// Force-directed graph drawing</a>.
    /// </summary>
    public class AreaDistributor {
        private readonly Log _log = Log.ToConsole<AreaDistributor>();
        private readonly RandomSource _random;
        private readonly MapAreaStringRenderer _renderer;
        private readonly bool _verboseOutput;

        /// <summary />
        public AreaDistributor(RandomSource random) : this(random, null, false) { }

        /// <summary>
        /// Creates an instance of <see cref="AreaDistributor" /> allowing to 
        /// render the resulting map area for debugging purposes.
        /// </summary>
        internal AreaDistributor(RandomSource random, MapAreaStringRenderer renderer = null,
                               bool verboseOutput = false) {
            _random = random;
            _renderer = renderer;
            _verboseOutput = verboseOutput;
        }

        /// <summary>
        /// Distributes objects in an area. Uses the <see cref="EvolvingSimulator"/>
        /// to evolve a <see cref="MapAreasSystem" />.
        /// </summary>
        public void Distribute(Area targetArea,
                               IEnumerable<Area> areasToAdd,
                               int maxEpochs) {
            var allAreas = targetArea.ChildAreas.Concat(areasToAdd).ToList();
            var areasWithNicknames = MapAreasSystem.GetNicknames(allAreas);
            if (_renderer != null) {
                _log.D(1, _renderer.Render(targetArea.Size, areasWithNicknames));
            }
            var simulator = new EvolvingSimulator(maxEpochs, 20);
            var system = new MapAreasSystem(_random, targetArea.Size, allAreas, r => { },
                r => {
                    if (_verboseOutput && _renderer != null) {
                        _log.D(1, _renderer.Render(targetArea.Size, areasWithNicknames));
                    }
                });
            simulator.Evolve(system);
        }
    }
}