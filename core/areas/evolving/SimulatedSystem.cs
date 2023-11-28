// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Nour.Play.Areas.Evolving {
    public abstract class SimulatedSystem {
        /// <summary>
        /// When overridden, evolves the system using its internal forces to
        /// <paramref cref="fragment" /> portion of the forces.
        /// </summary>
        /// <param name="fragment">The portion of the impacting factor to
        /// apply</param>
        /// <returns></returns>
        public abstract GenerationImpact Evolve(double fragment);

        /// <summary>
        /// Compares this epoch result to previous epochs results to determines
        /// if the evolution of the system has converged.
        /// </summary>
        /// <param name="generationsImpact">The impact of previous epochs.
        /// </param>
        /// <returns></returns>
        public abstract EpochResult CompleteEpoch(
            EpochResult[] previousEpochsResults,
            GenerationImpact[] thisEpochGenerationsImpacts);
    }

    public abstract class GenerationImpact { }
    public class EpochResult {
        public bool CompleteEvolution { get; set; }
    }
}