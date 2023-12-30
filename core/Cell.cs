using System.Collections.Generic;

namespace PlayersWorlds.Maps {
    public class Cell {
        // TODO: Maybe a HashSet?
        public List<string> Tags { get; } = new List<string>();
    }
}