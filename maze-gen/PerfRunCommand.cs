using System;
using System.Linq;
using System.Reflection;
using CommandLine;

namespace PlayersWorlds.Maps {
    partial class MainClass {
        [Verb("perfrun", HelpText = "Run a predefined set of tests to get a performance baseline.")]
        class PerfRunCommand : BaseCommand {
            //clone options here
            override public int Run() {
                base.Run();
                var assy = "PlayersWorlds.Maps.Tests";
                var ns = "PlayersWorlds.Maps.Maze";
                var allTypes =
                    Assembly.Load(assy).GetTypes()
                        .Where(t => !string.IsNullOrEmpty(t.Namespace) &&
                                        t.Namespace.StartsWith(ns))
                        .Where(t => t.GetCustomAttributes()
                                    .Any(a => a.GetType().Name ==
                                                    "TestFixtureAttribute"))
                        .ToList();
                foreach (var type in allTypes) {
                    Console.Write($"Creating " + type.FullName + "... ");
                    var testObj = Activator.CreateInstance(type);
                    Console.WriteLine(testObj != null ? "OK" : "FAILED");
                    if (testObj == null) {
                        continue;
                    }
                    var allMethods =
                        type.GetMethods()
                            .Where(m => m.GetParameters().Length == 0)
                            .Where(m => m.GetCustomAttributes()
                                         .Any(a => a.GetType().Name == "TestAttribute"));
                    foreach (var method in allMethods) {
                        Console.Write($"Calling {type.FullName}.{method.Name}()...");
                        try {
                            method.Invoke(testObj, null);
                            Console.WriteLine("OK");
                        } catch (Exception e) {
                            if (e.ToString().IndexOf("SuccessException") >= 0) {
                                Console.WriteLine("SuccessException");
                            } else {
                                Console.WriteLine(e.ToString());
                            }
                        }
                    }
                }
                return 0;
            }
        }
    }
}
