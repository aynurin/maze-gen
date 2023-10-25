using System;
using System.Collections.Generic;
using System.Linq;
using Nour.Play.Maze;
using Nour.Play.Maze.PostProcessing;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class MazeGeneratorTest {

        [Test]
        public void MazeGenerator_CanGenerateMazes(
            [ValueSource("GetAllGenerators")] Type generatorType
        ) {
            var generator = (MazeGenerator)Activator.CreateInstance(generatorType);
            // var map = new Maze2D(3, 4);
            // Assert.IsTrue(map.Cells.All(cell => cell.Links().Count == 0));
            var map = (Maze2D)typeof(MazeGenerator).GetMethod("Generate")
                .MakeGenericMethod(generatorType)
                .Invoke(null, new Object[] { new Vector(3, 4) });
            // generator.GenerateMaze(map);
            Assert.IsTrue(map.Cells.Count == 12);
            Assert.IsTrue(map.Cells.Any(cell => cell.Links().Count > 0));
        }

        [Test]
        public void DijkstraDistance_CanFindPaths(
            [ValueSource("GetAllGenerators")] Type generatorType
        ) {
            var generator = (MazeGenerator)Activator.CreateInstance(generatorType);
            var map = new Maze2D(3, 4);
            generator.GenerateMaze(map);
            List<MazeCell> solution = new List<MazeCell>();
            Assert.DoesNotThrow(() => solution = DijkstraDistance.FindLongestTrail(map));
            Assert.IsNotEmpty(solution);
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