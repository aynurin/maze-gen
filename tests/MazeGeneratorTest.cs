using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using Nour.Play.Maze;
using Nour.Play.Maze.Solvers;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class MazeGeneratorTest {

        [Test]
        public void MazeGenerator_CanGenerateMazes(
            [ValueSource("GetAllGenerators")] Type generatorType
        ) {
            var generator = (MazeGenerator)Activator.CreateInstance(generatorType);
            var map = new Map2D(3, 4);
            Assert.IsTrue(map.Cells.All(cell => cell.Links().Count == 0));
            generator.GenerateMaze(map);
            Assert.IsTrue(map.Cells.Any(cell => cell.Links().Count > 0));
        }

        [Test]
        public void DijkstraDistance_CanFindPaths(
            [ValueSource("GetAllGenerators")] Type generatorType
        ) {
            var generator = (MazeGenerator)Activator.CreateInstance(generatorType);
            var map = new Map2D(3, 4);
            generator.GenerateMaze(map);
            var dijkstra = DijkstraDistance.FindLongest(map[0, 0]);
            Assert.IsTrue(dijkstra.Solution.HasValue);
            Assert.IsNotEmpty(dijkstra.Solution.Value);
        }

        public static IEnumerable<Type> GetAllGenerators() {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm.GetName().Name == "Nour.Play.Maze")
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(MazeGenerator) != p &&
                            typeof(MazeGenerator).IsAssignableFrom(p));
        }
    }
}