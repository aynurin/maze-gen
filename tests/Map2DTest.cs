using System;
using Nour.Play.Maze;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class Map2DTest {
        [Test]
        public void Map2D_ToString() {
            var maze = Maze2D.Parse("3x3;0:3;1:2,4;2:5;3:4;4:7;6:7;7:8");
            var map = maze.ToMap(Maze2D.MazeToMapOptions.Custom(new int[] { 2, 3, 2 }, new int[] { 2, 3, 2 }, new int[] { 1, 2 }, new int[] { 1, 3 }));
            var expected =
                "░░▓░░░░░░░░\n" +
                "░░▓░░░░░░░░\n" +
                "░░▒░░░▒▒▒░░\n" +
                "░░░░░░▓▓▓░░\n" +
                "░░░░░░▓▓▓░░\n" +
                "░░░░░░▓▓▓░░\n" +
                "▓▓▒░░░▒▒▒▓▓\n" +
                "▓▓▒░░░▒▒▒▓▓\n" +
                "░░░░░░░░░░░\n" +
                "░░░░░░░░░░░\n";
            var actual = map.ToString();
            Console.WriteLine(actual);
            Assert.AreEqual(expected, actual);
        }
    }
}