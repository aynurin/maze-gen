// using System;
// using System.Collections.Generic;
// using System.Linq;
// using NUnit.Framework;
// using PlayersWorlds.Maps.Areas;
// using PlayersWorlds.Maps.Maze;

// namespace PlayersWorlds.Maps.Mazes {

//     [TestFixture]
//     internal class MazeGeneratorAreasTest {
//         [Test]
//         public void FillAreasAreAppliedProperly(
//             [ValueSource("GetAllGenerators")] Type generatorType) {
//             var maze = MazeGenerator.Generate(new Vector(10, 10),
//                 new GeneratorOptions() {
//                     FillFactor = GeneratorOptions.FillFactorOption.Full,
//                     Algorithm = generatorType,
//                     MapAreasOptions = GeneratorOptions.MapAreaOptions.Manual,
//                     MapAreas = new List<MapArea>() {
//                             MapArea.Create(AreaType.Fill, new Vector(5, 7), new Vector(2, 3))
//                     }
//                 });
//             var log = Log.CreateForThisTest();
//             log.D(5, maze.ToString());
//             Assert.That(maze.MapAreas.Count, Is.EqualTo(1), "Wrong number of filled areas");
//             Assert.That(maze.MapAreas.First().Key.Type, Is.EqualTo(AreaType.Fill));
//             Assert.That(maze.MapAreas.First().Value, Has.Exactly(35).Items);
//             Assert.That(maze.VisitedCells, Has.Exactly(65).Items);
//             Assert.That(maze.UnlinkedCells, Is.Empty);
//         }

//         public static IEnumerable<Type> GetAllGenerators() {
//             return AppDomain.CurrentDomain.GetAssemblies()
//                 .Where(asm => asm.GetName().Name == "PlayersWorlds.Maps")
//                 .SelectMany(s => s.GetTypes())
//                 .Where(p => typeof(MazeGenerator) != p &&
//                             typeof(MazeGenerator).IsAssignableFrom(p));
//         }
//     }
// }