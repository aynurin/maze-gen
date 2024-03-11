using System;
using System.Diagnostics;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PlayersWorlds.Maps;

[SetUpFixture]
public class TestsSetup {
    private readonly Log _log = Log.ToConsole<TestsSetup>();
    [OneTimeSetUp]
    public void RunBeforeAnyTests() {
        if (TestContext.Parameters.Exists("SEED")) {
            var seed = int.Parse(TestContext.Parameters["SEED"]);
            GlobalRandom.Reseed(seed);
        }
        _log.I($"Running tests with seed {GlobalRandom.Seed}");
        if (TestContext.Parameters.Exists("DEBUG")) {
            Log.DebugLoggingLevel = TestLog.DebugLevel = int.Parse(TestContext.Parameters["DEBUG"]);
        }
        if (Log.DebugLoggingLevel > 0) {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests() {
        if (Log.DebugLoggingLevel > 0) {
            Trace.Flush();
        }
    }
}