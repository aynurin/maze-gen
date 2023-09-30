// using System;
// using Nour.Play.Maze;
// using NUnit.Framework;

// [TestFixture]
// public class MazeLayoutManagerTest {

//     [Test]
//     public void LayoutManager_CanGenerateZones() {
//         var zonesGenerator = new RandomZoneGenerator();
//         var layoutManager = new MazeLayoutManager(new Size(5, 5), zonesGenerator);
//         var zones = layoutManager.GenerateZones();
//         Assert.Greater(zones.Count, 0);
//     }
// }