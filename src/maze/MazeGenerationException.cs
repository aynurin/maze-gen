using System;
using System.Runtime.Serialization;

namespace PlayersWorlds.Maps.Maze {
    [Serializable]
    internal class MazeGenerationException : Exception {
        public MazeGenerationException() {
        }

        public MazeGenerationException(string message) : base(message) {
        }

        public MazeGenerationException(string message, Exception innerException) : base(message, innerException) {
        }

        protected MazeGenerationException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}