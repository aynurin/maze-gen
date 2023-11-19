using System;
using System.Collections.Generic;
using Nour.Play.Areas;
using NUnit.Framework;

namespace Nour.Play {
    // TODO: Create MapAreaSystem tests
    [TestFixture]
    public class MapAreasSystemTest {
        [Test]
        public void MapAreaSystemTest_DistancePositiveNormal() {
            var log = Log.CreateForThisTest();
            var d = MapAreasSystem.GetAxisDistance(0, 1, 2, 3);
            Assert.IsFalse(d.overlap);
            Assert.AreEqual(1D, d.distance);
            Assert.AreEqual(-1D, d.sign);
        }
        [Test]
        public void MapAreaSystemTest_DistancePositiveOverlap() {
            var log = Log.CreateForThisTest();
            var d = MapAreasSystem.GetAxisDistance(0, 2, 1, 3);
            Assert.IsTrue(d.overlap);
            Assert.AreEqual(1D, d.distance);
            Assert.AreEqual(-1D, d.sign);
        }

        [Test]
        public void MapAreaSystemTest_DistancePositiveCollide() {
            var log = Log.CreateForThisTest();
            var d = MapAreasSystem.GetAxisDistance(0, 2, 2, 4);
            Assert.IsFalse(d.overlap);
            Assert.AreEqual(0D, d.distance);
            Assert.AreEqual(-1D, d.sign);
        }

        [Test]
        public void MapAreaSystemTest_DistanceCentersMatch() {
            var log = Log.CreateForThisTest();
            var d = MapAreasSystem.GetAxisDistance(0, 2, -1, 4);
            Assert.IsTrue(d.overlap);
            Assert.AreEqual(3D, d.distance);
            Assert.AreEqual(1D, d.sign);
        }

        [Test]
        public void MapAreaSystemTest_DistanceNegativeNormal() {
            var log = Log.CreateForThisTest();
            var d = MapAreasSystem.GetAxisDistance(0, 1, -3, 2);
            Assert.IsFalse(d.overlap);
            Assert.AreEqual(1D, d.distance);
            Assert.AreEqual(1D, d.sign);
        }

        [Test]
        public void MapAreaSystemTest_DistanceNegativeOverlap() {
            var log = Log.CreateForThisTest();
            var d = MapAreasSystem.GetAxisDistance(0, 2, -2, 3);
            Assert.IsTrue(d.overlap);
            Assert.AreEqual(1D, d.distance);
            Assert.AreEqual(1D, d.sign);
        }

        [Test]
        public void MapAreaSystemTest_DistanceNegativeCollide() {
            var log = Log.CreateForThisTest();
            var d = MapAreasSystem.GetAxisDistance(0, 2, -4, 4);
            Assert.IsFalse(d.overlap);
            Assert.AreEqual(0D, d.distance);
            Assert.AreEqual(1D, d.sign);
        }

        [Test]
        public void GetOpposingForce_WhenCenterMatches_NoOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 0.0;

            // Act
            var result = MapAreasSystem.GetOpposingForce(
                1.0, 1.0, thisOpposingForce,
                0.0, 3.0, thisForce);

            // Assert
            Assert.AreEqual(thisForce, result.thisForce);
            Assert.AreEqual(thisForce, result.opposingForce);
        }

        [Test]
        public void GetOpposingForce_WhenCenterMatches_WithOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 2.5;

            // Act
            var result = MapAreasSystem.GetOpposingForce(
                1.0, 1.0, thisOpposingForce,
                0.0, 3.0, thisForce);

            // Assert
            Assert.AreEqual(-thisOpposingForce, result.thisForce);
            Assert.AreEqual(-thisOpposingForce, result.opposingForce);
        }

        [Test]
        public void GetOpposingForce_CentersInside_NoOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 0.0;

            // Act
            var result = MapAreasSystem.GetOpposingForce(
                1.0, 1.0, thisOpposingForce,
                1.0, 1.5, thisForce);

            // Assert
            Assert.AreEqual(thisForce, result.thisForce);
            Assert.AreEqual(0, result.opposingForce);
        }

        [Test]
        public void GetOpposingForce_CentersInside_WithOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 2.0;

            // Act
            var result = MapAreasSystem.GetOpposingForce(
                1.0, 1.0, thisOpposingForce,
                1.0, 1.5, thisForce);

            // Assert
            Assert.AreEqual(thisForce, result.thisForce);
            Assert.AreEqual(0, result.opposingForce);
        }

        [Test]
        public void GetOpposingForce_CentersOutside_NoOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 0.0;

            // Act
            var result = MapAreasSystem.GetOpposingForce(
                1.0, 1.0, thisOpposingForce,
                1.6, 3.0, thisForce);

            // Assert
            Assert.AreEqual(thisForce, result.thisForce);
            Assert.AreEqual(0, result.opposingForce);
        }

        [Test]
        public void GetOpposingForce_CentersOutside_WithOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 2.0;

            // Act
            var result = MapAreasSystem.GetOpposingForce(
                1.0, 1.0, thisOpposingForce,
                1.6, 3.0, thisForce);

            // Assert
            Assert.AreEqual(thisForce, result.thisForce);
            Assert.AreEqual(0, result.opposingForce);
        }

        [Test]
        public void GetOpposingForce_NoIntersection_NoOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 0.0;

            // Act
            var result = MapAreasSystem.GetOpposingForce(
                1.0, 1.0, thisOpposingForce,
                2.0, 3.0, thisForce);

            // Assert
            Assert.AreEqual(thisForce, result.thisForce);
            Assert.AreEqual(0, result.opposingForce);
        }

        [Test]
        public void GetOpposingForce_NoIntersection_WithOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 2.0;

            // Act
            var result = MapAreasSystem.GetOpposingForce(
                1.0, 1.0, thisOpposingForce,
                2.0, 3.0, thisForce);

            // Assert
            Assert.AreEqual(thisForce, result.thisForce);
            Assert.AreEqual(0, result.opposingForce);
        }

        [Test]
        public void NormalForce_Positive() =>
            // Test normal force calculation for positive and negative distances
            TestNormalForce(new List<Tuple<double, double>> {
                new Tuple<double, double>(0.1, 2.7),
                new Tuple<double, double>(1.0, 1.35),
                new Tuple<double, double>(5.0, 0.25)
            });

        [Test]
        public void NormalForce_Negative() {
            // Test normal force calculation for positive and negative distances
            TestNormalForce(new List<Tuple<double, double>> {
                new Tuple<double, double>(-0.1, -2.7),
                new Tuple<double, double>(-1.0, -1.35),
                new Tuple<double, double>(-5.0, -0.25)
            });
        }

        [Test]
        public void NormalForce_PositiveCapped() {
            // Test normal force calculation for positive and negative distances
            TestNormalForce(new List<Tuple<double, double>> {
                new Tuple<double, double>(Double.Epsilon, 3D),
                new Tuple<double, double>(VectorD.MIN, 3D),
                new Tuple<double, double>(0.01, 2.97D),
                new Tuple<double, double>(10.0, 0),
                new Tuple<double, double>(100.0, 0)
            });
        }

        [Test]
        public void NormalForce_NegativeCapped() {
            // Test normal force calculation for positive and negative distances
            TestNormalForce(new List<Tuple<double, double>> {
                new Tuple<double, double>(-Double.Epsilon, -3D),
                new Tuple<double, double>(-VectorD.MIN, -3D),
                new Tuple<double, double>(-0.01, -2.97D),
                new Tuple<double, double>(-10.0, 0),
                new Tuple<double, double>(-100.0, 0)
            });
        }

        private void TestNormalForce(List<Tuple<double, double>> testCases) {
            foreach (var testCase in testCases) {
                var distance = testCase.Item1;
                var expectedForce = testCase.Item2;
                var actualForce = MapAreasSystem.NormalForce(distance);
                Assert.AreEqual(expectedForce, Math.Round(actualForce, 2));
            }
        }

        [Test]
        public void CollideForce_Positive() {
            // Test collide force calculation for positive and negative distances
            var testCases = new List<double> {
                Double.Epsilon,
                VectorD.MIN,
                0.001,
                0.01,
                0.1,
                1.0,
                10.0,
                100.0,
                1000.0
            };

            foreach (var distance in testCases) {
                var actualForce = MapAreasSystem.CollideForce(distance, 0.1);
                Assert.AreEqual(10.00, Math.Round(actualForce, 2));
            }
        }

        [Test]
        public void CollideForce_Negative() {
            // Test collide force calculation for positive and negative distances
            var testCases = new List<double> {
                -Double.Epsilon,
                -VectorD.MIN,
                -0.001,
                -0.01,
                -0.1,
                -1.0,
                -10.0,
                -100.0,
                -1000.0
            };

            foreach (var distance in testCases) {
                var actualForce = MapAreasSystem.CollideForce(distance, 0.1);
                Assert.AreEqual(-10.00, Math.Round(actualForce, 2));
            }
        }

        [Test]
        public void OverlapForce_Positive() {
            // Test overlap force calculation for positive and negative distances
            var fragment = 3D;
            var testCases = new List<Tuple<double, double>> {
                new Tuple<double, double>(0.1, 0.37),
                new Tuple<double, double>(1.0, 0.67),
                new Tuple<double, double>(5.0, 2.0),
                new Tuple<double, double>(10.0, 3.67)
            };


            foreach (var testCase in testCases) {
                var distance = testCase.Item1;
                var expectedForce = testCase.Item2;
                var actualForce = MapAreasSystem.OverlapForce(distance, fragment);
                Assert.AreEqual(expectedForce, Math.Round(actualForce, 2));
            }
        }

        [Test]
        public void OverlapForce_Negative() {
            // Test overlap force calculation for positive and negative distances
            var fragment = 3D;
            var testCases = new List<Tuple<double, double>> {
                new Tuple<double, double>(-0.1, -0.37),
                new Tuple<double, double>(-1.0, -0.67),
                new Tuple<double, double>(-5.0, -2.0),
                new Tuple<double, double>(-10.0, -3.67)
            };


            foreach (var testCase in testCases) {
                var distance = testCase.Item1;
                var expectedForce = testCase.Item2;
                var actualForce = MapAreasSystem.OverlapForce(distance, fragment);
                Assert.AreEqual(expectedForce, Math.Round(actualForce, 2));
            }
        }
    }
}