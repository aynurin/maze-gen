using System;
using NUnit.Framework;

namespace PlayersWorlds.Maps.MapFilters {

    [TestFixture]
    internal class Map2DEraseSpotsTest {

        [Test]
        public void BackslashEraseSpots() {
            var map = Map2DTest.Parse(Map2DTest.Backslash, Map2DTest.Tags);
            Console.WriteLine(map.ToString());

            new Map2DEraseSpots(new[] {
                Cell.CellTag.MazeWall,
                Cell.CellTag.MazeWallCorner
            }, false, Cell.CellTag.MazeTrail, 3, 3)
                .Render(map);

            var expected =
                "▓▓░░░\n" +
                "░▓▓░░\n" +
                "░░▓▓░\n" +
                "░░░▓▓\n" +
                "░░░░▓\n";
            Console.WriteLine(expected);
            Console.WriteLine(map.ToString());
            Assert.That(map.ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void VariousSpotsEraseSpots() {
            var emptyMap =
                "░░░░░\n" +
                "░░░░░\n" +
                "░░░░░\n" +
                "░░░░░\n" +
                "░░░░░\n";
            var spots = new string[] {
                Map2DTest.Spot2x2A, Map2DTest.Spot2x2B, Map2DTest.Spot2x2C,
                Map2DTest.Spot2x2D, Map2DTest.Spot2x2E, Map2DTest.Spot2x2F
            };
            foreach (var spot in spots) {
                var map = Map2DTest.Parse(spot, Map2DTest.Tags);
                Console.WriteLine(map.ToString());

                new Map2DEraseSpots(new[] {
                    Cell.CellTag.MazeWall,
                    Cell.CellTag.MazeWallCorner
                }, false, Cell.CellTag.MazeTrail, 2, 2)
                    .Render(map);

                var expected = emptyMap;
                Console.WriteLine(expected);
                Console.WriteLine(map.ToString());
                Assert.That(map.ToString(), Is.EqualTo(expected));
            }
        }

        [Test]
        public void Spot1x3SpotsEraseSpots() {
            var map = Map2DTest.Parse(Map2DTest.Spot1x3, Map2DTest.Tags);
            Console.WriteLine(map.ToString());

            new Map2DEraseSpots(new[] {
                Cell.CellTag.MazeWall,
                Cell.CellTag.MazeWallCorner
            }, false, Cell.CellTag.MazeTrail, 2, 2)
                .Render(map);

            var expected = Map2DTest.Spot1x3;
            Console.WriteLine(expected);
            Console.WriteLine(map.ToString());
            Assert.That(map.ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void SmoothBoxVoidBgEraseSpots() {
            var map = Map2DTest.Parse(Map2DTest.SmoothBoxVoidBg, Map2DTest.Tags);
            Console.WriteLine(map.ToString());

            new Map2DEraseSpots(new[] {
                Cell.CellTag.MazeVoid
            }, true, Cell.CellTag.MazeWall, 3, 3)
                .Render(map);
            var expected =
                "▒▓▓▓▒\n" +
                "▓▓▓▓▓\n" +
                "▓▓▓▓▓\n" +
                "▓▓▓▓▓\n" +
                "▒▓▓▓▓\n";
            Console.WriteLine(expected);
            Console.WriteLine(map.ToString());
            Assert.That(map.ToString(), Is.EqualTo(expected));
        }
        [Test]
        public void SmoothBoxEraseSpots() {
            var map = Map2DTest.Parse(Map2DTest.SmoothBox, Map2DTest.Tags);
            Console.WriteLine(map.ToString());

            new Map2DEraseSpots(new[] {
                Cell.CellTag.MazeTrail
            }, true, Cell.CellTag.MazeWall, 3, 3)
                .Render(map);
            var expected =
                "▒▓▓▓▒\n" +
                "▓▓▓▓▓\n" +
                "▓▓▓▓▓\n" +
                "▓▓▓▓▓\n" +
                "▒▓▓▓▓\n";
            Console.WriteLine(expected);
            Console.WriteLine(map.ToString());
            Assert.That(map.ToString(), Is.EqualTo(expected));
        }
    }
}