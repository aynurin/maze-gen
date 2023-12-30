
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace PlayersWorlds.Maps.Areas.Evolving {
    [TestFixture]
    public class ForceFormulaTestTest {
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
                var actualForce = new ForceFormula().NormalForce(distance);
                Assert.AreEqual(expectedForce, Math.Round(actualForce, 2));
            }
        }

        [Test]
        public void CollideForce_Positive() {
            // Test collide force calculation for positive and negative
            // distances
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
                var actualForce = new ForceFormula()
                    .CollideForce(distance, 0.1);
                Assert.AreEqual(10.00, Math.Round(actualForce, 2));
            }
        }

        [Test]
        public void CollideForce_Negative() {
            // Test collide force calculation for positive and negative
            // distances
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
                var actualForce = new ForceFormula()
                    .CollideForce(distance, 0.1);
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
                var actualForce = new ForceFormula()
                    .OverlapForce(distance, fragment);
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
                var actualForce = new ForceFormula()
                    .OverlapForce(distance, fragment);
                Assert.AreEqual(expectedForce, Math.Round(actualForce, 2));
            }
        }
    }
}