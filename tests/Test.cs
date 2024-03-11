using NUnit.Framework;

namespace PlayersWorlds.Maps {
    public class Test {
        [SetUp]
        public void SetUp() {
            if (TestLog.DebugLevel > 0) {
                TestContext.Progress.WriteLine("Running : " +
                    TestContext.CurrentContext.Test.DisplayName + "...");
            }
        }
    }
}