using System;
using System.Runtime.Serialization;

namespace PlayersWorlds.Maps.Areas {
    [Serializable]
    internal class AreaGeneratorException : Exception {
        public AreaGenerator Generator { get; set; }

        public AreaGeneratorException(AreaGenerator generator, string message) : base(message) {
            Generator = generator;
        }

        public AreaGeneratorException(AreaGenerator generator, Exception innerException) : base(innerException.Message, innerException) {
            Generator = generator;
        }

        protected AreaGeneratorException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        public override string Message {
            get {
                return base.Message + "\n" + Generator.ToString();
            }
        }
    }
}