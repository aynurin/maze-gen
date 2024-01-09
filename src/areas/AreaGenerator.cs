using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Areas {
    /// <summary>
    /// Implemented the AreaGenerator as a helper to generate areas for a map.
    /// </summary>
    public abstract class AreaGenerator {
        /// <summary>
        /// When overridden in the deriving class, generates areas for a map of
        /// the specified size.
        /// </summary>
        /// <param name="size">The size of the map.</param>
        /// <returns>Areas to be added to the map.</returns>
        public abstract IEnumerable<MapArea> Generate(Vector size);
    }
}