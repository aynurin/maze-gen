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

        [Test]
        public void CellsAt_OneCell() {
            var map = new Map2D(new Vector(5, 5));
            var cells = map.IterateArea(new Vector(0, 0), new Vector(1, 1)).ToList();
            var debugString = string.Join(",", cells.Select(c => map.Cells.IndexOf(c.cell)));
            Assert.That(1, Is.EqualTo(cells.Count()));
            Assert.That(new Vector(0, 0).ToIndex(map.Size), Is.EqualTo(map.Cells.IndexOf(cells.First().cell)));
        }

        [Test]
        public void CellsAt_TwoCells() {
            var map = new Map2D(new Vector(5, 5));
            var cells = map.IterateArea(new Vector(0, 0), new Vector(2, 1)).ToList();
            var debugString = string.Join(",", cells.Select(c => map.Cells.IndexOf(c.cell)));
            Assert.That(2, Is.EqualTo(cells.Count()));
            Assert.That(new Vector(0, 0).ToIndex(map.Size), Is.EqualTo(map.Cells.IndexOf(cells.First().cell)), "0,0: " + debugString);
            Assert.That(new Vector(1, 0).ToIndex(map.Size), Is.EqualTo(map.Cells.IndexOf(cells.Last().cell)), "1,0: " + debugString);
        }

        [Test]
        public void CellsAt_TreeByTwo() {
            var map = new Map2D(new Vector(5, 5));
            var cells = map.IterateArea(new Vector(0, 0), new Vector(3, 2)).ToList();
            var debugString = string.Join(",", cells.Select(c => map.Cells.IndexOf(c.cell)));
            Assert.That(6, Is.EqualTo(cells.Count()));
            Assert.That(new Vector(0, 0).ToIndex(map.Size), Is.EqualTo(map.Cells.IndexOf(cells[0].cell)), "0,0: " + debugString);
            Assert.That(new Vector(0, 1).ToIndex(map.Size), Is.EqualTo(map.Cells.IndexOf(cells[1].cell)), "0,1: " + debugString);
            Assert.That(new Vector(1, 0).ToIndex(map.Size), Is.EqualTo(map.Cells.IndexOf(cells[2].cell)), "1,0: " + debugString);
            Assert.That(new Vector(1, 1).ToIndex(map.Size), Is.EqualTo(map.Cells.IndexOf(cells[3].cell)), "1,1: " + debugString);
            Assert.That(new Vector(2, 0).ToIndex(map.Size), Is.EqualTo(map.Cells.IndexOf(cells[4].cell)), "2,0: " + debugString);
            Assert.That(new Vector(2, 1).ToIndex(map.Size), Is.EqualTo(map.Cells.IndexOf(cells[5].cell)), "2,1: " + debugString);
        }

        [Test]
        public void CellsAt_FourCellsFar() {
            var map = new Map2D(new Vector(5, 5));
            var cells = map.IterateArea(new Vector(3, 3), new Vector(2, 2)).ToList();
            var debugString = string.Join(",", cells.Select(c => map.Cells.IndexOf(c.cell)));
            Assert.That(4, Is.EqualTo(cells.Count()));
            Assert.That(new Vector(3, 3).ToIndex(map.Size), Is.EqualTo(map.Cells.IndexOf(cells[0].cell)), "3,3: " + debugString);
            Assert.That(new Vector(3, 4).ToIndex(map.Size), Is.EqualTo(map.Cells.IndexOf(cells[1].cell)), "3,4: " + debugString);
            Assert.That(new Vector(4, 3).ToIndex(map.Size), Is.EqualTo(map.Cells.IndexOf(cells[2].cell)), "4,3: " + debugString);
            Assert.That(new Vector(4, 4).ToIndex(map.Size), Is.EqualTo(map.Cells.IndexOf(cells[3].cell)), "4,4: " + debugString);
        }

        [Test]
        public void CellsAt_OutOfBounds() {
            var map = new Map2D(new Vector(5, 5));
            Assert.Throws<IndexOutOfRangeException>(() => map.IterateArea(new Vector(5, 5), new Vector(1, 1)).ToList());
            Assert.Throws<IndexOutOfRangeException>(() => map.IterateArea(new Vector(6, 3), new Vector(1, 1)).ToList());
            Assert.Throws<IndexOutOfRangeException>(() => map.IterateArea(new Vector(0, 0), new Vector(6, 6)).ToList());
            Assert.Throws<IndexOutOfRangeException>(() => map.IterateArea(new Vector(0, 0), new Vector(6, 1)).ToList());
        }

        [Test]
        public void CellsAt_AnyCellAtP1x1() {
            var map = new Map2D(new Vector(5, 5));
            var cells = map.IterateIntersection(new Vector(2, 2), new Vector(1, 1)).ToList();
            Assert.That(cells, Has.Exactly(1).Items);
            Assert.That(cells.First().cell, Is.EqualTo(map.Cells[new Vector(2, 2).ToIndex(map.Size)]));
        }

        [Test]
        public void CellsAt_AnyCellAt2x2() {
            var map = new Map2D(new Vector(5, 5));
            var cells = map.IterateIntersection(new Vector(2, 2), new Vector(2, 2)).ToList();
            Assert.That(cells, Has.Exactly(4).Items);
            Assert.That(cells[0].cell, Is.EqualTo(map.Cells[new Vector(2, 2).ToIndex(map.Size)]));
            Assert.That(cells[1].cell, Is.EqualTo(map.Cells[new Vector(2, 3).ToIndex(map.Size)]));
            Assert.That(cells[2].cell, Is.EqualTo(map.Cells[new Vector(3, 2).ToIndex(map.Size)]));
            Assert.That(cells[3].cell, Is.EqualTo(map.Cells[new Vector(3, 3).ToIndex(map.Size)]));
        }

        [Test]
        public void CellsAt_AnyCellAtEdge1x1() {
            var map = new Map2D(new Vector(5, 5));
            var cells = map.IterateIntersection(new Vector(4, 4), new Vector(1, 1)).ToList();
            Assert.That(cells, Has.Exactly(1).Items);
            Assert.That(cells[0].cell, Is.EqualTo(map.Cells[new Vector(4, 4).ToIndex(map.Size)]));
        }

        [Test]
        public void CellsAt_AnyCellAtFarEdge2x2() {
            var map = new Map2D(new Vector(5, 5));
            var cells = map.IterateIntersection(new Vector(4, 4), new Vector(2, 2)).ToList();
            Assert.That(cells, Has.Exactly(1).Items);
            Assert.That(cells[0].cell, Is.EqualTo(map.Cells[new Vector(4, 4).ToIndex(map.Size)]));
        }

        [Test]
        public void CellsAt_AnyCellAtCloseEdge2x2() {
            var map = new Map2D(new Vector(5, 5));
            var cells = map.IterateIntersection(new Vector(0, 0), new Vector(2, 2)).ToList();
            Assert.That(cells, Has.Exactly(4).Items);
            Assert.That(cells[0].cell, Is.EqualTo(map.Cells[new Vector(0, 0).ToIndex(map.Size)]));
            Assert.That(cells[1].cell, Is.EqualTo(map.Cells[new Vector(0, 1).ToIndex(map.Size)]));
            Assert.That(cells[2].cell, Is.EqualTo(map.Cells[new Vector(1, 0).ToIndex(map.Size)]));
            Assert.That(cells[3].cell, Is.EqualTo(map.Cells[new Vector(1, 1).ToIndex(map.Size)]));
        }

        [Test]
        public void CellsAt_AnyCellAtZeroSize() {
            var map = new Map2D(new Vector(5, 5));
            Assert.That(() => map.IterateIntersection(new Vector(2, 2), new Vector(0, 0)).First(), Throws.ArgumentException);
        }
    }
}