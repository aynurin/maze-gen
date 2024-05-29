using System;
using System.Runtime.Serialization;

namespace PlayersWorlds.Maps.Maze {
    [Serializable]
    internal class MazeBuilderException : Exception {
        public Maze2DBuilder Builder { get; set; }

        public MazeBuilderException(Maze2DBuilder builder, string message) : base(message) {
            Builder = builder;
        }

        public MazeBuilderException(Maze2DBuilder builder, Exception innerException) : base(innerException.Message, innerException) {
            Builder = builder;
        }

        protected MazeBuilderException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        public override string Message {
            get {
                return base.Message + "\n" + Builder.Random.ToString() + "\n" + Builder.ToString();
            }
        }
    }
}