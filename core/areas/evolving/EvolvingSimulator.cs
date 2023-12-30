using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Areas.Evolving {
    /// <summary>
    /// Evolves a <see cref="TargetSystem" /> over time using its own internal
    /// impact over time.
    /// </summary>
    public class EvolvingSimulator {
        private readonly int _epochs;
        private readonly int _generationsPerEpoch;

        /// <summary>
        /// Creates an instance of EvolvingSimulator with the given max number of epochs
        /// and given number of generations per epoch.
        /// </summary>
        /// <param name="maxEpochs">Max number of epochs in this simulation</param>
        /// <param name="generationsPerEpoch">Number of generations in each
        /// epoch</param>
        public EvolvingSimulator(int maxEpochs, int generationsPerEpoch) {
            if (maxEpochs < 1) {
                // TODO: Create a test for this
                throw new ArgumentException($"Number of epochs must be greater than 0 (provided {maxEpochs})", "maxEpochs");
            }
            _epochs = maxEpochs;
            _generationsPerEpoch = generationsPerEpoch;
        }

        /// <summary>
        /// Evolves the given system over time.
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public int Evolve(SimulatedSystem system) {
            // The evolution happens in epochs, and each epoch can produce a total
            // of epoch_impact.
            // Epochs consist of generations, and each generation produces
            // epoch_impact / _generationsPerEpoch impact.
            // We evaluate epoch_impact to see if it's significant. If not, we
            // conclude the evolution because we don't expect any more significant
            // impact in the same system.

            // The point of this simulation is that we can't apply a full analogue
            // impact in a discreet system immediately, because analogue impact
            // changes along with system changes. E.g., if we apply 1/10th of full
            // impact, the overall impact will change, and the following impact is different
            // from the original impact.

            var epochResults = new List<EpochResult>();
            for (int e = 0; e < _epochs; e++) {
                var impact = Enumerable.Range(0, _generationsPerEpoch)
                    .Select(gen => system.Evolve(1D / _generationsPerEpoch));
                var epochResult = system.CompleteEpoch(
                    epochResults.ToArray(), impact.ToArray());
                epochResults.Add(epochResult);
                if (epochResult.CompleteEvolution) {
                    return e;
                }
            }
            return _epochs;
        }
    }
}