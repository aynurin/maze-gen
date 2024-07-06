using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.MapFilters {
    /// <summary>
    /// When implemented, filters can be applied to a map modify it a desired
    /// way.
    /// </summary>
    public abstract class Map2DFilter {
        /// <summary>
        /// Apply the filter to the specified <see cref="Area" />.
        /// </summary>
        /// <param name="map">The map to apply the filter to.</param>
        public abstract void Render(Area map);
    }
}
