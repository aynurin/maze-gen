using System;
using System.Collections.Generic;
using System.Linq;
using Nour.Play.Maze;
using NUnit.Framework;

namespace Nour.Play {
    [TestFixture]
    public class AreaDistributorTest {

        [Test, Property("Category", "Smoke")]
        public void AreaDistributor_CanFindDistanceBetweenBoxes() {
            var log = Log.Create("CanFindDistanceBetweenBoxes");
            var env = new Maze2D(20, 20);
            var A = new AreaDistributor.Room(new VectorD(7, 8), new VectorD(6, 4));
            var B1 = new AreaDistributor.Room(new VectorD(15, 11), new VectorD(2, 3));
            var B2 = new AreaDistributor.Room(new VectorD(15, 0), new VectorD(4, 4));
            var B3 = new AreaDistributor.Room(new VectorD(11, 1), new VectorD(3, 5));
            var B4 = new AreaDistributor.Room(new VectorD(2, 2), new VectorD(2, 4));
            var B5 = new AreaDistributor.Room(new VectorD(2, 7), new VectorD(2, 4));
            var B6 = new AreaDistributor.Room(new VectorD(1, 15), new VectorD(4, 2));
            var B7 = new AreaDistributor.Room(new VectorD(6, 15), new VectorD(3, 3));
            var B8 = new AreaDistributor.Room(new VectorD(10, 15), new VectorD(5, 4));
            AreaDistributor.Draw(log, env, new List<AreaDistributor.Room>() { A, B1, B2, B3, B4, B5, B6, B7, B8 });

            Func<AreaDistributor.Room, AreaDistributor.Room, double, bool> tt =
                (a, b, expected) => {
                    var actual = AreaDistributor.Distance(log, a, b);
                    var diff = Math.Round(expected, 4) - Math.Round(actual, 4);
                    var close = diff > -Double.Epsilon && diff < Double.Epsilon;
                    if (!close) {
                        log.Buffered.Flush();
                    }
                    Assert.IsTrue(close, $"expected {expected:F4}, actual {actual:F4}");
                    return close;
                };
            tt(A, B1, 2.1667);
            tt(A, B2, 5.3151);
            tt(A, B3, 2.1428);
            tt(A, B4, 4.8293);
            tt(A, B5, 3.0305);
            tt(A, B6, 4.6098);
            tt(A, B7, 3.2142);
            tt(A, B8, 3.1856);
        }

        [Test, Property("Category", "Smoke")]
        public void AreaDistributorTest_OneTest() {
            var maze = new Maze2D(new Vector(10, 10));
            var env = new AreaDistributor.Room(VectorD.Zero2D, new VectorD(maze.Size));
            var placedRooms = new List<AreaDistributor.Room>() {
                new AreaDistributor.Room(VectorD.Zero2D, new VectorD(4, 4))
            };
            var log = Log.Create("OneTest");
            var d = new AreaDistributor(log, true);
            var placed = d.DistributePlacedRooms(maze, placedRooms, 2);

            var success = placed.All(block =>
                            placed.Where(other => block != other)
                                  .All(other => !block.Overlaps(other))) &&
                          placed.All(block => block.Fits(env));
            if (!success) {
                log.Buffered.Flush();
            }
            Assert.IsTrue(success, $"expected 3x3, but was {placed[0].Position}");
        }

        public static IEnumerable<string> OverlapTwoTests() {
            yield return "11x11: P2x2;S4x4, P5x5;S4x4";
            yield return "11x11: P5x2;S4x4, P2x5;S4x4";
            yield return "11x11: P2x5;S4x4, P5x2;S4x4";
            yield return "11x11: P5x5;S4x4, P2x2;S4x4";
            yield return "11x11: P4x4;S4x4, P3x3;S4x4";
            yield return "11x11: P4x3;S4x4, P3x4;S4x4";
            yield return "11x11: P3.5x3.5;S4x4, P3.5x3.5;S4x4";
            yield return "16x16: P4x4;S8x8, P5x9;S2x2";
            yield return "16x16: P4x4;S8x8, P5x7;S4x4";
            yield return "16x16: P5x5;S6x6, P6x6;S4x4";
        }

        [Test, Property("Category", "Smoke")]
        public void AreaDistributorTest_OverlapTwo(
            [ValueSource("OverlapTwoTests")] string layout
            ) {
            var parts = layout.Split(':');
            var maze = new Maze2D(Vector.Parse(parts[0]));
            var env = new AreaDistributor.Room(VectorD.Zero2D, new VectorD(maze.Size));
            var placedRooms = parts[1].Split(',').Select(s => AreaDistributor.Room.Parse(s)).ToList();
            var log = Log.Create("OverlapTwo");
            var d = new AreaDistributor(log, true);
            var placed = d.DistributePlacedRooms(maze, placedRooms, 4);

            var success = placed.All(block =>
                            placed.Where(other => block != other)
                                  .All(other => !block.Overlaps(other))) &&
                          placed.All(block => block.Fits(env));
            if (!success) {
                log.Buffered.Flush();
            }
            Assert.IsTrue(success, $"expected 6x6 and 1x1, or 1x6 and 6x1, but was {placed[0].Position}");
        }

        public static IEnumerable<string> VariousSingles() {
            // inside
            yield return "6x12: P1x2;S2x2";
            yield return "6x12: P2x2;S2x2";
            yield return "6x12: P3x2;S2x2";
            yield return "6x12: P1x5;S2x2";
            yield return "6x12: P2x5;S2x2";
            yield return "6x12: P3x5;S2x2";
            yield return "6x12: P1x8;S2x2";
            yield return "6x12: P2x8;S2x2";
            yield return "6x12: P3x8;S2x2";

            // outside
            yield return "6x12: P-1x-4;S2x2";
        }

        [Test, Property("Category", "Smoke")]
        public void AreaDistributorTest_SingleMapForce(
            [ValueSource("VariousSingles")] string layout
            ) {
            var parts = layout.Split(':');
            var maze = new Maze2D(Vector.Parse(parts[0]));
            var env = new AreaDistributor.Room(VectorD.Zero2D, new VectorD(maze.Size));
            var placedRooms = parts[1].Split(',').Select(s => AreaDistributor.Room.Parse(s)).ToList();
            var log = Log.Create("OverlapTwo");
            var d = new AreaDistributor(log, true);
            var placed = d.DistributePlacedRooms(maze, placedRooms, 4);

            var success = placed.All(block =>
                            placed.Where(other => block != other)
                                  .All(other => !block.Overlaps(other))) &&
                          placed.All(block => block.Fits(env));
            if (!success) {
                log.Buffered.Flush();
            }
            Assert.IsTrue(success, $"expected 6x6 and 1x1, or 1x6 and 6x1, but was {placed[0].Position}");
        }

        [Test, Property("Category", "Smoke")]
        public void AreaDistributorTest_TwoTest() {
            var maze = new Maze2D(new Vector(10, 10));
            var env = new AreaDistributor.Room(VectorD.Zero2D, new VectorD(maze.Size));
            var placedRooms = new List<AreaDistributor.Room>() {
                new AreaDistributor.Room(VectorD.Zero2D, new VectorD(2, 2)),
                new AreaDistributor.Room(new VectorD(maze.Size), new VectorD(2, 2))
            };
            var log = Log.Create("TwoTest");
            var d = new AreaDistributor(log, true);
            var placed = d.DistributePlacedRooms(maze, placedRooms, 2);

            var actual = String.Join(",", placed.Select(room => room.Position.ToString()));
            var success = placed.All(block =>
                            placed.Where(other => block != other)
                                  .All(other => !block.Overlaps(other))) &&
                          placed.All(block => block.Fits(env));
            if (!success) {
                log.Buffered.Flush();
            }
            Assert.IsTrue(success, $"expected 1x1 and 7x7, or 7x1 and 1x7, but was {actual}");
        }

        [Test]
        public void AreaDistributorTest_CanLayout(
            [ValueSource("TestLayouts")] string layout
            ) {
            var log = Log.CreateForThisTest();
            var helper = new AreaDistributorHelper(log);
            var parts = layout.Split(':');
            var maze = new Maze2D(Vector.Parse(parts[0]));
            var placedRooms = parts[1]
                .Split(
                    new char[] { ',', ' ' },
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(s => AreaDistributor.Room.Parse(s))
                .ToList();

            var d = new AreaDistributor(log, true);
            var result = helper.PlaceRoomsAndValidate(d, maze, placedRooms, 100);
            log.D($"DEBUG: {TestContext.Parameters.Exists("DEBUG")}");
            if (result.Item1 != 0 || TestContext.Parameters.Exists("DEBUG")) {
                log.Buffered.Flush();
            }
            Assert.IsTrue(result.Item1 == 0, result.Item2);
        }

        public static IEnumerable<String> TestLayouts() {
            yield return "13x8: P9x6;S3x1, P1x4;S2x1";
            yield return "20x20: P14x14;S5x3, P-2x14;S1x4, P7x-3;S2x4, P2x4;S1x3";
            yield return "6x14: P0x10;S1x3";
            yield return "23x22: P11x10;S6x2, P-1x14;S5x1, P5x-3;S5x3, P11x22;S2x1";
            yield return "18x23: P-3x-8;S5x5, P30x8;S3x6, P9x3;S4x4, P12x15;S1x3";
            yield return "23x8: P4x7;S4x1, P2x-1;S1x1";
            yield return "21x13: P8x-3;S6x3, P8x4;S6x3, P7x2;S4x2";
            yield return "16x14: P2x13;S3x1, P11x-1;S4x3";
            yield return "24x12: P12x7;S2x1, P14x-1;S5x2, P16x11;S1x1";
            yield return "9x18: P2x10;S2x2, P2x8;S2x5";
            yield return "9x18: P2x8;S2x5, P2x10;S2x2";
            yield return "21x21: P-3x-3;S6x6, P6x10;S4x2, P9x16;S2x2, P3x14;S4x2";
            yield return "14x24: P-9x5;S2x7, P26x5;S1x7, P3x11;S3x7, P5x19;S3x2, P3x6;S2x5, P2x19;S2x2";
            yield return "33x10: P13x11;S10x1, P13x-1;S10x1, P19x3;S8x1, P4x6;S9x1, P9x3;S9x2, P2x3;S3x2";
            yield return "6x49: P-1x5;S1x11, P5x17;S1x11, P2x23;S1x9, P2x14;S1x13, P0x12;S1x9";
            yield return "35x5: P5x-1;S10x1, P23x4;S8x1, P4x2;S3x1, P12x2;S2x1";
            yield return "42x5: P4x5;S10x1, P30x1;S3x1, P10x1;S3x1, P2x2;S1x1";
            yield return "39x5: P14x5;S10x1, P5x-1;S9x1, P2x2;S1x1, P10x4;S8x1";
            yield return "6x47: P6x12;S1x12, P3x13;S1x5, P3x18;S1x4, P3x38;S1x6, P2x2;S1x2";
            yield return "6x47: P-1x28;S1x11, P6x11;S1x11, P6x6;S1x11, P-1x6;S1x11, P1x35;S1x5";
            yield return "6x47: P7x10;S1x14, P2x41;S1x3, P3x4;S1x3, P3x10;S1x4, P0x25;S1x9";
            yield return "5x45: P-1x5;S1x10, P-2x21;S1x12, P7x17;S1x14, P2x40;S1x1, P1x29;S1x4";
            yield return "46x5: P4x-1;S10x1, P29x0;S7x1, P29x2;S2x1, P32x2;S10x1, P17x0;S5x1";
            yield return "6x48: P-2x28;S1x14, P6x6;S1x12, P3x22;S1x2, P1x14;S1x7, P2x23;S1x12";
            yield return "47x5: P30x-3;S13x1, P24x-2;S11x1, P17x7;S14x1, P27x2;S2x1, P4x1;S4x1";
            yield return "5x45: P-2x13;S1x10, P-2x7;S1x12, P2x3;S1x2, P0x3;S1x8, P2x10;S1x1";
            yield return "5x47: P7x28;S1x13, P6x15;S1x11, P1x2;S1x3, P2x40;S1x2, P2x43;S1x1";
            yield return "5x36: P-2x21;S1x11, P2x12;S1x1, P2x15;S1x2, P2x8;S1x3";
            yield return "6x39: P-1x20;S1x11, P3x19;S1x2, P2x11;S1x11, P2x20;S1x5, P3x10;S1x5";
            yield return "44x6: P23x-2;S13x1, P7x-2;S13x1, P6x2;S8x1, P37x3;S2x1, P29x2;S10x1";
            yield return "6x42: P-1x6;S1x11, P-2x21;S1x13, P3x36;S1x3, P5x7;S1x9, P0x27;S1x10";
            yield return "40x5: P8x6;S12x1, P24x-1;S9x1, P3x0;S8x1, P23x2;S2x1";
            yield return "47x7: P19x7;S14x1, P9x2;S8x1, P7x4;S8x1, P14x2;S4x1, P28x5;S10x1, P35x3;S2x1";
            yield return "48x6: P32x-1;S11x1, P9x7;S14x1, P5x7;S13x1, P27x2;S15x1, P5x3;S3x1";
            yield return "46x6: P20x-2;S13x1, P10x7;S14x1, P22x2;S4x1, P35x3;S6x1, P17x3;S7x1";
            yield return "6x39: P-1x10;S1x11, P0x3;S1x7, P0x11;S1x10, P2x20;S1x7, P3x26;S1x6";
            yield return "43x42: P33.91x34.92;S12.00x2.00 P39.10x11.40;S2.00x11.00, P31.81x25.88;S2.00x2.00, P2.19x11.99;S12.00x6.00, P17.39x28.49;S12.00x10.00, P2.13x20.24;S6.00x5.00, P24.94x14.67;S12.00x5.00, P35.64x2.17;S4.00x3.00, P3.20x34.43;S6.00x7.00, P17.43x13.89;S5.00x10.00, P36.81x22.96;S2.00x13.00, P23.22x2.78;S3.00x9.00, P11.17x1.74;S5.00x8.00, P10.77x25.44;S9.00x11.00";
            yield return "43x42: P3.20x34.43;S6.00x7.00, P10.77x25.44;S9.00x11.00";
            yield return "43x42: P35x26;S2x11, P34x35;S12x2";
            yield return "43x42: P32x32;S4x4, P34x34;S2x2";
            yield return "43x42: P32x32;S4x4, P34x34;S4x2";
            yield return "43x42: P32x26;S4x10, P34x34;S4x2";
            yield return "43x42: P30x26;S2x11, P29x35;S12x2";
            yield return "45x43: P38.00x15.09;S8.00x10.00 P0.42x0.08;S10.00x4.00, P17.64x33.27;S8.00x2.00, P-0.03x22.54;S1.00x12.00, P17.50x0.62;S12.00x7.00, P23.05x19.66;S14.00x7.00, P21.76x17.00;S1.00x7.00, P11.34x8.71;S8.00x9.00, P9.60x23.72;S12.00x7.00, P0.45x8.22;S8.00x5.00, P0.65x19.15;S12.00x3.00, P19.87x38.89;S7.00x2.00, P33.99x38.13;S10.00x4.00, P34.00x0.58;S11.00x12.00";
        }
    }
}