using System;
using Nour.Play.Maze;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class Maze2DToMap2DConverterTest {
        [Test]
        public void Maze2DToMap2DConverter_CanGenerateMap() {
            var maze = Maze2D.Parse("4x4;0:1,4;1:2,5;2:3;3:7;4:5,8;8:12;12:13;13:14;14:10;10:11");
            var map = maze.ToMap(Maze2DToMap2DConverter.MazeToMapOptions.Custom(new int[] { 1, 2, 1, 1 }, new int[] { 2, 2, 3, 2 }, new int[] { 1, 2, 1, 2, 1 }, new int[] { 2, 3, 2, 2, 2 }));
            var expected2 =
                "▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓\n" +
                "▓▓░░░░░░░░░░░░░░░░▓▓\n" +
                "▓▓░░░░░░░▒▒▓▓▓▒▒░░▓▓\n" +
                "▓▓░░░░░░░▒▒▓▓▓▒▒░░▓▓\n" +
                "▓▓░░░░░░░▓▓▓▓▓▓▓░░▓▓\n" +
                "▓▓░░░░░░░▓▓▓▓▓▓▓░░▓▓\n" +
                "▓▓░░▒▒▒▓▓▓▓▓▓▓▓▓▓▓▓▓\n" +
                "▓▓░░▓▓▓▓▓▓▓░░░░░░░▓▓\n" +
                "▓▓░░▒▒▒▓▓▒▒░░░▒▒▓▓▓▓\n" +
                "▓▓░░▒▒▒▓▓▒▒░░░▒▒▓▓▓▓\n" +
                "▓▓░░░░░░░░░░░░▓▓▓▓  \n" +
                "▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓    \n";
            var actual = map.ToString();
            Console.WriteLine(actual);
            Assert.AreEqual(expected2, actual);
        }

        [Test]
        public void Maze2DToMap2DConverter_ThrowsIfInvalidOptions() {
            var maze = Maze2D.Parse("4x4;0:1,4;1:2,5;2:3;3:7;4:5,8;8:12;12:13;13:14;14:10;10:11");
            Assert.Throws<ArgumentException>(() => Maze2DToMap2DConverter.MazeToMapOptions.Custom(new int[] { 1 }, new int[] { 2 }, new int[] { 3 }, new int[] { 0 }));
            Assert.Throws<ArgumentException>(() => Maze2DToMap2DConverter.MazeToMapOptions.Custom(new int[] { 1 }, new int[] { 2 }, new int[] { -3 }, new int[] { 4 }));
            Assert.Throws<ArgumentException>(() => Maze2DToMap2DConverter.MazeToMapOptions.Custom(new int[] { 1 }, new int[] { -2 }, new int[] { 3 }, new int[] { 4 }));
            Assert.Throws<ArgumentException>(() => Maze2DToMap2DConverter.MazeToMapOptions.Custom(new int[] { -1 }, new int[] { 2 }, new int[] { 3 }, new int[] { 4 }));
            Assert.DoesNotThrow(() => Maze2DToMap2DConverter.MazeToMapOptions.Custom(new int[] { 1, 2, 1, 1 }, new int[] { 2, 2, 3, 2 }, new int[] { 1, 2, 1, 2, 1 }, new int[] { 2, 3, 2, 2, 2 }).ThrowIfWrong(maze));
            Assert.Throws<ArgumentException>(() => Maze2DToMap2DConverter.MazeToMapOptions.Custom(new int[] { -1 }, new int[] { 2 }, new int[] { 3 }, new int[] { 4 }).ThrowIfWrong(maze));
        }
    }
}