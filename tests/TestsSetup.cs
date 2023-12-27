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
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests() {
        // ...
        Trace.Flush();
    }
}