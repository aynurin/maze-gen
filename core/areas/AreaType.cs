
namespace PlayersWorlds.Maps.Areas {
    public enum AreaType {
        None = 0,
        /// E.g., a hall with walls around it or a valley with a lake and a
        /// shore around the lake, the player can enter and walk the hall or
        /// the shores.
        Hall,
        /// E.g., a lake or a mount the player can not enter.
        Fill,
        /// A regular maze area (might have different tags to denote any
        /// specific purpose like styling or activity)
        Maze,
    }
}