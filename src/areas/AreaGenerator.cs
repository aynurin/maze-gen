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
        /// <remarks>
        /// If pre-existing areas are specified, they should be counted to make
        /// sure there are enough and not too many areas in the maze.
        /// </remarks>
        /// <param name="targetArea">The map to generate areas for.</param>
        /// <returns>Areas to be added to the map.</returns>
        public abstract IEnumerable<Area> Generate(Area targetArea);
    }
}