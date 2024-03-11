using System;
using System.Linq;
using CommandLine;

namespace PlayersWorlds.Maps {
    partial class MainClass {
        [Verb("run", HelpText = "Run a specific test N times.")]
        class RunCommand : BaseCommand {
            [Option('p', "path", Required = true, HelpText = "Test path to run.")]
            public string Codepath { get; set; }

            [Option('c', "count", Default = 1, Required = false, HelpText = "Number of runs.")]
            public int RunCount { get; set; }

            override public int Run() {
                base.Run();
                var parts = Codepath.Split('.').ToArray();
                var methodName = parts.Last();
                var typeName = string.Join(".", parts.Take(parts.Length - 1));
                var type = Type.GetType(typeName + ", PlayersWorlds.Maps.Tests");
                var testObj = Activator.CreateInstance(type);
                Console.WriteLine($"Creating obj {testObj?.GetType().FullName}: {testObj}");
                var method = type.GetMethod(methodName);
                for (var i = 0; i < RunCount; i++) {
                    Console.WriteLine(method + ": " + i);
                    method.Invoke(testObj, null);
                }
                return 0;
            }
        }
    }
}
