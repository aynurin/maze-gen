
namespace PlayersWorlds.Maps.Areas {
    /// <summary>
    /// Defines if a given area is a regular maze (the default), a joint .
    /// </summary>
    public enum AreaType {
        /// <summary>
        /// Same as <see cref="Maze"/>.
        /// </summary>
        None = 0,
        /// <summary>
        /// A regular area that might have different tags to denote any
        /// specific purpose like styling or activity, but does not limit
        /// generative algorithms.
        /// </summary>
        Maze = 0,
        /// <summary>
        /// E.g., a hall with walls around it or a valley with a lake and a
        /// shore around the lake, the player can enter and walk the hall or
        /// the shores.
        /// </summary>
        Hall = 1,
        /// <summary>
        /// An area the player cannot enter, e.g., a lake or a rock.
        /// </summary>
        Fill = 2
    }
}