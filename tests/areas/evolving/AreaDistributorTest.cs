using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PlayersWorlds.Maps.Areas.Evolving {
    [TestFixture]
    public class AreaDistributorTest : Test {
        private RandomSource _random;
        public override void SetUp() {
            base.SetUp();
            _random = RandomSource.CreateFromEnv();
        }
        private static Area TestArea(int x, int y, int width, int height) =>
            Area.CreateUnpositioned(
                new Vector(x, y), new Vector(width, height), AreaType.None);

        [Test, Category("Integration")]
        public void AreaDistributorTest_OneTest() =>
            AreaDistributorHelper.Distribute(
                _random,
                TestLog.CreateForThisTest(), new Vector(10, 10),
                new List<Area>() { TestArea(0, 0, 4, 4) },
                maxEpochs: 10)
                .AssertAllFit();

        [Test, Category("Integration")]
        public void AreaDistributorTest_SidePressure() =>
            AreaDistributorHelper.Distribute(
                _random,
                TestLog.CreateForThisTest(), new Vector(10, 10),
                new List<Area>() {
                    TestArea(1, 4, 8, 2),
                    TestArea(1, 5, 2, 4),
                    TestArea(2, 6, 2, 4),
                    TestArea(3, 7, 2, 4)
                },
                maxEpochs: 10)
                .AssertAllFit();

        [Test, Category("Integration")]
        public void AreaDistributorTest_TwoTest() =>
            AreaDistributorHelper.Distribute(
                _random,
                TestLog.CreateForThisTest(),
                new Vector(10, 10),
                new List<Area>() {
                    TestArea(0, 0, 2, 2),
                    TestArea(10, 10, 2, 2)
                },
                maxEpochs: 10
                ).AssertAllFit();

        [Test, Category("Integration")]
        public void AreaDistributorTest_OverlapTwo(
            [ValueSource("OverlapTwoTests")] string layout
            ) {
            var parts = layout.Split(':');
            AreaDistributorHelper.Distribute(
                _random,
                TestLog.CreateForThisTest(), VectorD.Parse(parts[0]).RoundToInt(),
                parts[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => Area.Parse(s, isPositionFixed: false)),
                maxEpochs: 10)
                .AssertAllFit();
        }

        public static IEnumerable<string> OverlapTwoTests() {
            yield return "11x11: P3.5x3.5;S4x4 P3.5x3.5;S4x4";
            yield return "16x16: P4x4;S8x8 P5x9;S2x2";
            yield return "16x16: P4x4;S8x8 P5x7;S4x4";
            yield return "16x16: P5x5;S6x6 P6x6;S4x4";
            yield return "11x11: P2x2;S4x4 P5x5;S4x4";
            yield return "11x11: P5x2;S4x4 P2x5;S4x4";
            yield return "11x11: P2x5;S4x4 P5x2;S4x4";
            yield return "11x11: P5x5;S4x4 P2x2;S4x4";
            yield return "11x11: P4x4;S4x4 P3x3;S4x4";
            yield return "11x11: P4x3;S4x4 P3x4;S4x4";
        }

        [Test, Category("FixThis"), Category("Integration")]
        public void AreaDistributorTest_OverlapTwoFail(
            [ValueSource("OverlapTwoFail")] string layout
            ) {
            var parts = layout.Split(':');
            AreaDistributorHelper.Distribute(
                _random,
                TestLog.CreateForThisTest(), VectorD.Parse(parts[0]).RoundToInt(),
                parts[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => Area.Parse(s, isPositionFixed: false)),
                maxEpochs: 10)
                .AssertDoesNotFit();
        }

        public static IEnumerable<string> OverlapTwoFail() {
            return Enumerable.Empty<string>();
        }

        [Test, Category("Integration")]
        public void AreaDistributorTest_SingleMapForce(
            [ValueSource("VariousSingles")] string layout
            ) {
            var parts = layout.Split(':');
            AreaDistributorHelper.Distribute(
                _random,
                TestLog.CreateForThisTest(), VectorD.Parse(parts[0]).RoundToInt(),
                parts[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => Area.Parse(s, isPositionFixed: false)),
                maxEpochs: 10)
                .AssertAllFit();
        }

        [Test, Category("Integration"), Category("Smoke")]
        public void AreaDistributorTest_CanLayout(
            [ValueSource("TestLayouts")] string layout
            ) {
            var parts = layout.Split(':');
            AreaDistributorHelper.Distribute(
                _random,
                TestLog.CreateForThisTest(), VectorD.Parse(parts[0]).RoundToInt(),
                parts[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.ToArea()),
                maxEpochs: 10)
                .AssertAllFit();
        }

        [Test, Category("Integration"), Category("FixThis")]
        public void AreaDistributorTest_FailLayout(
            [ValueSource("FailingLayouts")] string layout
            ) {
            var parts = layout.Split(':');
            AreaDistributorHelper.Distribute(
                _random,
                TestLog.CreateForThisTest(), VectorD.Parse(parts[0]).RoundToInt(),
                parts[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.ToArea()),
                maxEpochs: 10)
                .AssertDoesNotFit();
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

        public static IEnumerable<string> TestLayouts() {
            yield return "13x8: P9x6;S3x1 P1x4;S2x1";
            yield return "20x20: P14x14;S5x3 P-2x14;S1x4 P7x-3;S2x4 P2x4;S1x3";
            yield return "6x14: P0x10;S1x3";
            yield return "23x22: P11x10;S6x2 P-1x14;S5x1 P5x-3;S5x3 P11x22;S2x1";
            yield return "18x23: P-3x-8;S5x5 P30x8;S3x6 P9x3;S4x4 P12x15;S1x3";
            yield return "23x8: P4x7;S4x1 P2x-1;S1x1";
            yield return "21x13: P8x-3;S6x3 P8x4;S6x3 P7x2;S4x2";
            yield return "16x14: P2x13;S3x1 P11x-1;S4x3";
            yield return "24x12: P12x7;S2x1 P14x-1;S5x2 P16x11;S1x1";
            yield return "9x18: P2x10;S2x2 P2x8;S2x5";
            yield return "9x18: P2x8;S2x5 P2x10;S2x2";
            yield return "21x21: P-3x-3;S6x6 P6x10;S4x2 P9x16;S2x2 P3x14;S4x2";
            yield return "14x24: P-9x5;S2x7 P26x5;S1x7 P3x11;S3x7 P5x19;S3x2 P3x6;S2x5 P2x19;S2x2";
            yield return "33x10: P13x11;S10x1 P13x-1;S10x1 P19x3;S8x1 P4x6;S9x1 P9x3;S9x2 P2x3;S3x2";
            yield return "6x49: P-1x5;S1x11 P5x17;S1x11 P2x23;S1x9 P2x14;S1x13 P0x12;S1x9";
            yield return "35x5: P5x-1;S10x1 P23x4;S8x1 P4x2;S3x1 P12x2;S2x1";
            yield return "42x5: P4x5;S10x1 P30x1;S3x1 P10x1;S3x1 P2x2;S1x1";
            yield return "39x5: P14x5;S10x1 P5x-1;S9x1 P2x2;S1x1 P10x4;S8x1";
            yield return "6x47: P6x12;S1x12 P3x13;S1x5 P3x18;S1x4 P3x38;S1x6 P2x2;S1x2";
            yield return "6x47: P-1x28;S1x11 P6x11;S1x11 P6x6;S1x11 P-1x6;S1x11 P1x35;S1x5";
            yield return "6x47: P7x10;S1x14 P2x41;S1x3 P3x4;S1x3 P3x10;S1x4 P0x25;S1x9";
            yield return "5x45: P-1x5;S1x10 P-2x21;S1x12 P7x17;S1x14 P2x40;S1x1 P1x29;S1x4";
            yield return "46x5: P4x-1;S10x1 P29x0;S7x1 P29x2;S2x1 P32x2;S10x1 P17x0;S5x1";
            yield return "6x48: P-2x28;S1x14 P6x6;S1x12 P3x22;S1x2 P1x14;S1x7 P2x23;S1x12";
            yield return "47x5: P30x-3;S13x1 P24x-2;S11x1 P17x7;S14x1 P27x2;S2x1 P4x1;S4x1";
            yield return "5x45: P-2x13;S1x10 P-2x7;S1x12 P2x3;S1x2 P0x3;S1x8 P2x10;S1x1";
            yield return "5x47: P7x28;S1x13 P6x15;S1x11 P1x2;S1x3 P2x40;S1x2 P2x43;S1x1";
            yield return "5x36: P-2x21;S1x11 P2x12;S1x1 P2x15;S1x2 P2x8;S1x3";
            yield return "6x39: P-1x20;S1x11 P3x19;S1x2 P2x11;S1x11 P2x20;S1x5 P3x10;S1x5";
            yield return "44x6: P23x-2;S13x1 P7x-2;S13x1 P6x2;S8x1 P37x3;S2x1 P29x2;S10x1";
            yield return "6x42: P-1x6;S1x11 P-2x21;S1x13 P3x36;S1x3 P5x7;S1x9 P0x27;S1x10";
            yield return "40x5: P8x6;S12x1 P24x-1;S9x1 P3x0;S8x1 P23x2;S2x1";
            yield return "47x7: P19x7;S14x1 P9x2;S8x1 P7x4;S8x1 P14x2;S4x1 P28x5;S10x1 P35x3;S2x1";
            yield return "48x6: P32x-1;S11x1 P9x7;S14x1 P5x7;S13x1 P27x2;S15x1 P5x3;S3x1";
            yield return "46x6: P20x-2;S13x1 P10x7;S14x1 P22x2;S4x1 P35x3;S6x1 P17x3;S7x1";
            yield return "6x39: P-1x10;S1x11 P0x3;S1x7 P0x11;S1x10 P2x20;S1x7 P3x26;S1x6";
            yield return "43x42: P3.20x34.43;S6.00x7.00 P10.77x25.44;S9.00x11.00";
            yield return "43x42: P35x26;S2x11 P34x35;S12x2";
            yield return "43x42: P32x32;S4x4 P34x34;S2x2";
            yield return "43x42: P32x32;S4x4 P34x34;S4x2";
            yield return "43x42: P32x26;S4x10 P34x34;S4x2";
            yield return "43x42: P30x26;S2x11 P29x35;S12x2";
            yield return "49x26: P37.95x4.74;S12.00x2.00 P0.62x11.59;S4.00x1.00 P20.55x10.45;S7.00x7.00 P19.55x3.18;S8.00x6.00 P29.30x-0.27;S12.00x4.00 P17.11x22.28;S15.00x3.00 P9.50x0.67;S9.00x2.00 P32.27x17.54;S3.00x5.00 P0.57x21.40;S7.00x4.00 P35.67x10.30;S13.00x7.00 P6.23x9.47;S10.00x7.00";
            yield return "26x29: P10.00x15.00;S7.00x3.00 P20.00x0.00;S5.00x6.00 P17.00x5.00;S7.00x7.00 P5.00x4.00;S6.00x4.00 P21.00x18.00;S4.00x2.00 P20.00x8.00;S3.00x8.00 P10.00x11.00;S4.00x5.00 P0.00x5.00;S1.00x8.00 P21.00x9.00;S3.00x6.00";
            yield return "8x45: P2.00x9.00;S1.00x6.00 P2.00x22.00;S1.00x8.00 P5.00x32.00;S1.00x10.00 P6.00x14.00;S1.00x9.00 P4.00x3.00;S1.00x14.00 P2.00x36.00;S1.00x8.00";
            yield return "31x47: P2.00x15.00;S9.00x1.00 P3.00x11.00;S5.00x4.00 P2.00x11.00;S5.00x7.00 P13.00x14.00;S5.00x10.00 P8.00x37.00;S2.00x9.00 P27.00x19.00;S3.00x8.00 P20.00x12.00;S7.00x7.00 P4.00x13.00;S4.00x2.00 P20.00x31.00;S6.00x2.00 P9.00x3.00;S9.00x3.00 P6.00x34.00;S6.00x1.00 P23.00x6.00;S5.00x2.00";
            yield return "7x47: P4.00x40.00;S1.00x5.00 P0.00x12.00;S1.00x7.00 P1.00x9.00;S1.00x3.00 P5.00x0.00;S1.00x14.00 P2.00x37.00;S1.00x9.00 P0.00x29.00;S1.00x6.00";
            yield return "48x38: P8x15;S1x3 P45x11;S2x7 P6x8;S1x3 P4x7;S10x11 P4x9;S2x8 P26x13;S1x10 P18x13;S10x7 P35x22;S10x5 P31x10;S5x9 P32x10;S13x6 P22x22;S14x6 P26x1;S3x3 P28x14;S13x10 P24x4;S12x8";
            yield return "31x32: P24.45x15.67;S8.00x7.00 P14.09x8.51;S1.00x6.00 P16.22x5.85;S9.00x9.00 P18.23x27.41;S1.00x4.00 P13.75x0.80;S3.00x3.00 P8.52x18.43;S4.00x7.00 P1.71x14.65;S9.00x6.00 P0.38x4.06;S7.00x7.00 P25.06x24.29;S5.00x7.00 P11.40x19.76;S6.00x9.00";
            yield return "45x43: P38.00x15.09;S8.00x10.00 P0.42x0.08;S10.00x4.00 P17.64x33.27;S8.00x2.00 P-0.03x22.54;S1.00x12.00 P17.50x0.62;S12.00x7.00 P23.05x19.66;S14.00x7.00 P21.76x17.00;S1.00x7.00 P11.34x8.71;S8.00x9.00 P9.60x23.72;S12.00x7.00 P0.45x8.22;S8.00x5.00 P0.65x19.15;S12.00x3.00 P19.87x38.89;S7.00x2.00 P33.99x38.13;S10.00x4.00 P34.00x0.58;S11.00x12.00";
            yield return "49x10: P4.00x1.00;S13.00x7.00 P8.00x1.00;S10.00x7.00";
            yield return "49x16: P2x7;S12x4 P7x-1;S11x3 P19x4;S13x4 P27x11;S15x4 P17x10;S3x4 P14x3;S13x4 P24x9;S6x4 P1x13;S14x2 P9x4;S4x3";
            yield return "49x42: P19x1;S11x2 P34x31;S6x8 P15x29;S13x12 P15x11;S5x12 P23x5;S12x6 P3x17;S9x2 P16x6;S15x4 P15x29;S9x11 P39x37;S6x4 P28x22;S12x9 P31x8;S6x3 P27x23;S9x5 P0x27;S13x9 P11x17;S5x11 P4x2;S1x1";
            yield return "24x46: P11x20;S3x9 P5x15;S6x10 P5x14;S7x13 P10x26;S5x11 P10x18;S1x4 P11x13;S1x5 P10x29;S6x9 P13x8;S2x10 P3x17;S7x13 P5x28;S5x5 P18x8;S5x12";
            yield return "10x10: P7x9;S2x3 P7x2;S3x2 P3x9;S2x2 P0x5;S2x3 P7x1;S2x3 P0x0;S6x5";
        }

        public static IEnumerable<string> FailingLayouts() {
            // !! Failing tests. Need to debug. May be similar reason to what's described in MapAreaSystem.cs.
            return Enumerable.Empty<string>();
        }
    }
}