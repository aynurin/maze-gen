using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace PlayersWorlds.Maps {
    public class Test {
        private readonly Dictionary<string, Stopwatch> _runningTests =
            new Dictionary<string, Stopwatch>();

        [SetUp]
        public virtual void SetUp() {
            if (TestLog.DebugLevel > 1) {
                TestContext.Progress.WriteLine("Running : " +
                    TestContext.CurrentContext.Test.FullName + "...");
            }
            _runningTests.Add(TestContext.CurrentContext.Test.ID,
                            Stopwatch.StartNew());
        }

        [TearDown]
        public virtual void TearDown() {
            var sw = _runningTests[TestContext.CurrentContext.Test.ID];
            _runningTests.Remove(TestContext.CurrentContext.Test.ID);
            sw.Stop();
            var duration = sw.Elapsed;
            if (TestLog.DebugLevel > 0 && duration.TotalMilliseconds > 200) {
                TestContext.Progress.WriteLine("Completed : " +
                    TestContext.CurrentContext.Test.FullName + ": " +
                    duration.ToString());
            }
        }
    }
}