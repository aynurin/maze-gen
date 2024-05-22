using System;
using System.Runtime.Serialization;

namespace PlayersWorlds.Maps.Maze {
    [Serializable]
    internal class MazeGenerationException : Exception {
        public Area Maze { get; set; }

        public MazeGenerationException(Area maze, string message) : base(message) {
            Maze = maze;
        }

        public MazeGenerationException(Area maze, Exception innerException) : base(innerException.Message, innerException) {
            Maze = maze;
        }

        protected MazeGenerationException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        public override string Message {
            get {
                return base.Message + "\n" + Maze.Serialize();
            }
        }
    }
}