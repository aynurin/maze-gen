using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// Provides a source of randomness with the ability to use a specific seed.
    /// </summary>
    public class FakeRandomSource : RandomSource {
        public FakeRandomSource() : base(0) { }
    }
}