using System;
using System.Diagnostics;
using NUnit.Framework;
using NUnit.Framework.Internal;

[SetUpFixture]
public class SetUpTests {
    [OneTimeSetUp]
    public void RunBeforeAnyTests() {
        if (TestContext.Parameters.Exists("DEBUG")) {
            Log.DebugLevel = int.Parse(TestContext.Parameters["DEBUG"]);
        }
        if (Log.DebugLevel >= 5) {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests() {
        if (Log.DebugLevel >= 5) {
            Trace.Flush();
        }
    }
}