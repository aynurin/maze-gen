using System;
using System.Collections.Generic;
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
        private readonly MapAreaStringRenderer _renderer;
        private readonly bool _verboseOutput;

        /// <summary />
        public AreaDistributor() : this(null, false) { }

        /// <summary>
        /// Creates an instance of <see cref="AreaDistributor" /> allowing to 
        /// render the resulting map area for debugging purposes.
        /// </summary>
        internal AreaDistributor(MapAreaStringRenderer renderer = null,
                               bool verboseOutput = false) {
            _renderer = renderer;
            _verboseOutput = verboseOutput;
        }

        /// <summary>
        /// Distributes objects in an area. Uses the <see cref="EvolvingSimulator"/>
        /// to evolve a <see cref="MapAreasSystem" />.
        /// </summary>
        public void Distribute(Vector mapSize,
                               IEnumerable<MapArea> areas,
                               int maxEpochs) {
            if (_renderer != null) {
                Console.WriteLine(_renderer.Render(mapSize, areas));
            }
            var simulator = new EvolvingSimulator(maxEpochs, 10);
            var system = new MapAreasSystem(mapSize, areas, r => { },
                r => {
                    if (_verboseOutput && _renderer != null) {
                        Console.WriteLine(_renderer.Render(mapSize, areas));
                    }
                });
            simulator.Evolve(system);
        }
    }
}