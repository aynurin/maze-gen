using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PlayersWorlds.Maps {

    [TestFixture]
    internal class Map2DTest {
        internal const string Backslash =
                        "▓▓░░░\n" +
                        "░▓▓░░\n" +
                        "░░▓▓░\n" +
                        "░░░▓▓\n" +
                        "░░░░▓\n";
        internal const string Spot2x2A =
                        "░░░░░\n" +
                        "░░░▓░\n" +
                        "░░▓▓░\n" +
                        "░░░░░\n" +
                        "░░░░░\n";
        internal const string Spot2x2B =
                        "░░░░░\n" +
                        "░░▓░░\n" +
                        "░░▓▓░\n" +
                        "░░░░░\n" +
                        "░░░░░\n";
        internal const string Spot2x2C =
                        "░░░░░\n" +
                        "░░▓▓░\n" +
                        "░░▓░░\n" +
                        "░░░░░\n" +
                        "░░░░░\n";
        internal const string Spot2x2D =
                        "░░░░░\n" +
                        "░░▓▓░\n" +
                        "░░▓░░\n" +
                        "░░░░░\n" +
                        "░░░░░\n";
        internal const string Spot2x2E =
                        "░░░░░\n" +
                        "░░▓▓░\n" +
                        "░░▓▓░\n" +
                        "░░░░░\n" +
                        "░░░░░\n";
        internal const string Spot2x2F =
                        "░░░░░\n" +
                        "░░▓░░\n" +
                        "░░░▓░\n" +
                        "░░░░░\n" +
                        "░░░░░\n";
        internal const string Spot1x3 =
                        "░░░░░\n" +
                        "░░▓░░\n" +
                        "░░▓░░\n" +
                        "░░▓░░\n" +
                        "░░░░░\n";
        internal const string BackslashVoidBg =
                        "▓▓░░░\n" +
                        "0▓▓░░\n" +
                        "00▓▓░\n" +
                        "000▓▓\n" +
                        "0000▓\n";
        internal const string SmoothCorner =
                        "░░░░░\n" +
                        "▓▓▓▒░\n" +
                        "░░▓▓░\n" +
                        "░░░▓░\n" +
                        "░░░▓░\n";
        internal const string SmoothCornerVoidBg =
                        "░░░░░\n" +
                        "▓▓▓▒░\n" +
                        "00▓▓░\n" +
                        "000▓░\n" +
                        "000▓░\n";
        internal const string SmoothBox =
                        "▒▓▓▓▒\n" +
                        "▓▓░▓▓\n" +
                        "▓░░░▓\n" +
                        "▓▓░░▓\n" +
                        "▒▓▓▓▓\n";
        internal const string SmoothBoxVoidBg =
                        "▒▓▓▓▒\n" +
                        "▓▓0▓▓\n" +
                        "▓000▓\n" +
                        "▓▓00▓\n" +
                        "▒▓▓▓▓\n";

        internal static Dictionary<char, Cell.CellTag> Tags = new Dictionary<char, Cell.CellTag>() {
            { '▓', Cell.CellTag.MazeWall },
            { '▒', Cell.CellTag.MazeWallCorner },
            { '░', Cell.CellTag.MazeTrail },
            { '0', Cell.CellTag.MazeVoid },
        };

        internal static Map2D Parse(string buffer, Dictionary<char, Cell.CellTag> tagsMapping) {
            var lines = buffer.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var size = new Vector(lines.Length, lines[0].Length);
            var map = new Map2D(size);
            var cellIndex = 0;
            for (var y = lines.Length - 1; y >= 0; y--) {
                for (var x = 0; x < lines[y].Length; x++) {
                    var tag = tagsMapping[lines[y][x]];
                    if (tag != Cell.CellTag.MazeVoid) {
                        map[Vector.FromIndex(cellIndex, size)]
                            .Tags.Add(tag);
                    }
                    cellIndex++;
                }
            }
            return map;
        }

        [Test]
        public void ParseMap() {
            Assert.That(Parse(Backslash, Tags).ToString(), Is.EqualTo(Backslash));
            Assert.That(Parse(BackslashVoidBg, Tags).ToString(), Is.EqualTo(BackslashVoidBg));
            Assert.That(Parse(SmoothCorner, Tags).ToString(), Is.EqualTo(SmoothCorner));
            Assert.That(Parse(SmoothCornerVoidBg, Tags).ToString(), Is.EqualTo(SmoothCornerVoidBg));
            Assert.That(Parse(SmoothBox, Tags).ToString(), Is.EqualTo(SmoothBox));
            Assert.That(Parse(SmoothBoxVoidBg, Tags).ToString(), Is.EqualTo(SmoothBoxVoidBg));
        }
    }
}