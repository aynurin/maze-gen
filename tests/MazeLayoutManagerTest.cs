// using System;
// using Nour.Play.Maze;
// using NUnit.Framework;

// [TestFixture]
// public class MazeLayoutManagerTest {

//     [Test]
//     public void LayoutManager_CanGenerateZones() {
//         var zonesGenerator = new RandomZoneGenerator();
//         var layoutManager = new MazeLayoutManager(new Vector(5, 5), zonesGenerator);
//         var areas = layoutManager.GenerateZones();
//         Assert.Greater(areas.Count, 0);
//     }
// }