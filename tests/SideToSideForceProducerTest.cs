
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PlayersWorlds.Maps.Areas.Evolving {
    [TestFixture]
    public class SideToSideForceProducerTest {
        private SideToSideForceProducer ForceProducer
            (Log log, double overlapFactor) =>
            new SideToSideForceProducer(
                log,
                new ForceFormula(),
                overlapFactor);

        [Test]
        public void MapAreaSystemTest_DistancePositiveNormal() {
            var d = ForceProducer(Log.CreateForThisTest(), 1)
                .GetAxisDistance(0, 1, 2, 3);
            Assert.IsFalse(d.overlap);
            Assert.AreEqual(1D, d.distance);
            Assert.AreEqual(-1D, d.sign);
        }
        [Test]
        public void MapAreaSystemTest_DistancePositiveOverlap() {
            var d = ForceProducer(Log.CreateForThisTest(), 1)
                .GetAxisDistance(0, 2, 1, 3);
            Assert.IsTrue(d.overlap);
            Assert.AreEqual(1D, d.distance);
            Assert.AreEqual(-1D, d.sign);
        }

        [Test]
        public void MapAreaSystemTest_DistancePositiveCollide() {
            var d = ForceProducer(Log.CreateForThisTest(), 1)
                .GetAxisDistance(0, 2, 2, 4);
            Assert.IsFalse(d.overlap);
            Assert.AreEqual(0D, d.distance);
            Assert.AreEqual(-1D, d.sign);
        }

        [Test]
        public void MapAreaSystemTest_DistanceCentersMatch() {
            var d = ForceProducer(Log.CreateForThisTest(), 1)
                .GetAxisDistance(0, 2, -1, 4);
            Assert.IsTrue(d.overlap);
            Assert.AreEqual(3D, d.distance);
            Assert.AreEqual(1D, d.sign);
        }

        [Test]
        public void MapAreaSystemTest_DistanceNegativeNormal() {
            var d = ForceProducer(Log.CreateForThisTest(), 1)
                .GetAxisDistance(0, 1, -3, 2);
            Assert.IsFalse(d.overlap);
            Assert.AreEqual(1D, d.distance);
            Assert.AreEqual(1D, d.sign);
        }

        [Test]
        public void MapAreaSystemTest_DistanceNegativeOverlap() {
            var d = ForceProducer(Log.CreateForThisTest(), 1)
                .GetAxisDistance(0, 2, -2, 3);
            Assert.IsTrue(d.overlap);
            Assert.AreEqual(1D, d.distance);
            Assert.AreEqual(1D, d.sign);
        }

        [Test]
        public void MapAreaSystemTest_DistanceNegativeCollide() {
            var d = ForceProducer(Log.CreateForThisTest(), 1)
                .GetAxisDistance(0, 2, -4, 4);
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
            var result = ForceProducer(Log.CreateForThisTest(), 1)
                .GetOpposingForce(
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
            var result = ForceProducer(Log.CreateForThisTest(), 1)
                .GetOpposingForce(
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
            var result = ForceProducer(Log.CreateForThisTest(), 1)
                .GetOpposingForce(
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
            var result = ForceProducer(Log.CreateForThisTest(), 1)
                .GetOpposingForce(
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
            var result = ForceProducer(Log.CreateForThisTest(), 1)
                .GetOpposingForce(
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
            var result = ForceProducer(Log.CreateForThisTest(), 1)
                .GetOpposingForce(
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
            var result = ForceProducer(Log.CreateForThisTest(), 1)
                .GetOpposingForce(
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
            var result = ForceProducer(Log.CreateForThisTest(), 1)
                .GetOpposingForce(
                    1.0, 1.0, thisOpposingForce,
                    2.0, 3.0, thisForce);

            // Assert
            Assert.AreEqual(thisForce, result.thisForce);
            Assert.AreEqual(0, result.opposingForce);
        }

        private SideToSideForceProducer _forceProducer;

        [SetUp]
        public void SetUp() {
            _forceProducer = new SideToSideForceProducer(
                Log.Create(TestContext.CurrentContext.Test.Name.Split('_').Last()),
                new ForceFormula(),
                1);
        }

        // https://docs.google.com/spreadsheets/d/1qvLtWNySSYW7v10g28d1CcepfDeN-g649f2Hsff5yDs/edit#gid=684476059

        /// <summary>1, 2, 4, 2</summary>
        private (double distance, double sign, bool overlap)
            FNormal => _forceProducer.GetAxisDistance(1, 2, 4, 2);
        /// <summary>4, 2, 1, 2</summary>
        private (double distance, double sign, bool overlap)
            FNormalR => _forceProducer.GetAxisDistance(4, 2, 1, 2);
        /// <summary>1, 2, 7, 2</summary>
        private (double distance, double sign, bool overlap)
            FNormalFar => _forceProducer.GetAxisDistance(1, 2, 7, 2);
        /// <summary>7, 2, 1, 2</summary>
        private (double distance, double sign, bool overlap)
            FNormalFarR => _forceProducer.GetAxisDistance(7, 2, 1, 2);
        /// <summary>1, 2, 3, 2</summary>
        private (double distance, double sign, bool overlap)
            Collide => _forceProducer.GetAxisDistance(1, 2, 3, 2);
        /// <summary>3, 2, 1, 2</summary>
        private (double distance, double sign, bool overlap)
            CollideR => _forceProducer.GetAxisDistance(3, 2, 1, 2);
        /// <summary>1, 3, 3, 2</summary>
        private (double distance, double sign, bool overlap)
            OverlapShallow => _forceProducer.GetAxisDistance(1, 3, 3, 2);
        /// <summary>3, 2, 1, 3</summary>
        private (double distance, double sign, bool overlap)
            OverlapShallowR => _forceProducer.GetAxisDistance(3, 2, 1, 3);
        /// <summary>1, 3, 2, 3</summary>
        private (double distance, double sign, bool overlap)
            OverlapDeep => _forceProducer.GetAxisDistance(1, 3, 2, 3);
        /// <summary>2, 3, 1, 3</summary>
        private (double distance, double sign, bool overlap)
            OverlapDeepR => _forceProducer.GetAxisDistance(2, 3, 1, 3);
        /// <summary>1, 5, 4, 2</summary>
        private (double distance, double sign, bool overlap)
            ContainTouch => _forceProducer.GetAxisDistance(1, 5, 4, 2);
        /// <summary>4, 2, 1, 5</summary>
        private (double distance, double sign, bool overlap)
            ContainTouchR => _forceProducer.GetAxisDistance(4, 2, 1, 5);
        /// <summary>1, 5, 2, 3</summary>
        private (double distance, double sign, bool overlap)
            ContainMatchCenter => _forceProducer.GetAxisDistance(1, 5, 2, 3);
        /// <summary>2, 3, 1, 5</summary>
        private (double distance, double sign, bool overlap)
            ContainMatchCenterR => _forceProducer.GetAxisDistance(2, 3, 1, 5);


        [Test]
        public void GetAxisDistance_Normal1() {
            var fp = ForceProducer(Log.CreateForThisTest(), 0.1);

            var f1 = FNormal;
            Assert.IsFalse(f1.overlap, "f1 overlap");
            Assert.AreEqual(1D, f1.distance, "f1 distance");
            Assert.AreEqual(-1D, f1.sign, "f1 sign");

            var f2 = FNormalR;
            Assert.IsFalse(f2.overlap, "f2 overlap");
            Assert.AreEqual(1D, f2.distance, "f2 distance");
            Assert.AreEqual(1D, f2.sign, "f2 sign");
        }

        [Test]
        public void GetAxisDistance_Normal2() {
            var fp = ForceProducer(Log.CreateForThisTest(), 0.1);

            var f1 = FNormalFar;
            Assert.IsFalse(f1.overlap, "f1 overlap");
            Assert.AreEqual(4D, f1.distance, "f1 distance");
            Assert.AreEqual(-1D, f1.sign, "f1 sign");

            var f2 = FNormalFarR;
            Assert.IsFalse(f2.overlap, "f2 overlap");
            Assert.AreEqual(4D, f2.distance, "f2 distance");
            Assert.AreEqual(1D, f2.sign, "f2 sign");
        }

        [Test]
        public void GetAxisDistance_Collide() {
            var fp = ForceProducer(Log.CreateForThisTest(), 0.1);

            var f1 = Collide;
            Assert.IsFalse(f1.overlap, "f1 overlap");
            Assert.AreEqual(0D, f1.distance, "f1 distance");
            Assert.AreEqual(-1D, f1.sign, "f1 sign");

            var f2 = CollideR;
            Assert.IsFalse(f2.overlap, "f2 overlap");
            Assert.AreEqual(0D, f2.distance, "f2 distance");
            Assert.AreEqual(1D, f2.sign, "f2 sign");
        }

        [Test]
        public void GetAxisDistance_Overlap1() {
            var fp = ForceProducer(Log.CreateForThisTest(), 0.1);

            var f1 = OverlapShallow;
            Assert.IsTrue(f1.overlap, "f1 overlap");
            Assert.AreEqual(1D, f1.distance, "f1 distance");
            Assert.AreEqual(-1D, f1.sign, "f1 sign");

            var f2 = OverlapShallowR;
            Assert.IsTrue(f2.overlap, "f2 overlap");
            Assert.AreEqual(1D, f2.distance, "f2 distance");
            Assert.AreEqual(1D, f2.sign, "f2 sign");
        }

        [Test]
        public void GetAxisDistance_Overlap2() {
            var fp = ForceProducer(Log.CreateForThisTest(), 0.1);

            var f1 = OverlapDeep;
            Assert.IsTrue(f1.overlap, "f1 overlap");
            Assert.AreEqual(2D, f1.distance, "f1 distance");
            Assert.AreEqual(-1D, f1.sign, "f1 sign");

            var f2 = OverlapDeepR;
            Assert.IsTrue(f2.overlap, "f2 overlap");
            Assert.AreEqual(2D, f2.distance, "f2 distance");
            Assert.AreEqual(1D, f2.sign, "f2 sign");
        }

        [Test]
        public void GetAxisDistance_Contain1() {
            var fp = ForceProducer(Log.CreateForThisTest(), 0.1);

            var f1 = ContainTouch;
            Assert.IsTrue(f1.overlap, "f1 overlap");
            Assert.AreEqual(2D, f1.distance, "f1 distance");
            Assert.AreEqual(-1D, f1.sign, "f1 sign");

            var f2 = ContainTouchR;
            Assert.IsTrue(f2.overlap, "f2 overlap");
            Assert.AreEqual(2D, f2.distance, "f2 distance");
            Assert.AreEqual(1D, f2.sign, "f2 sign");
        }

        [Test]
        public void GetAxisDistance_Contain2() {
            var fp = ForceProducer(Log.CreateForThisTest(), 0.1);

            var f1 = ContainMatchCenter;
            Assert.IsTrue(f1.overlap, "f1 overlap");
            Assert.AreEqual(4D, f1.distance, "f1 distance");
            Assert.AreEqual(1D, f1.sign, "f1 sign");

            var f2 = ContainMatchCenterR;
            Assert.IsTrue(f2.overlap, "f2 overlap");
            Assert.AreEqual(4D, f2.distance, "f2 distance");
            Assert.AreEqual(1D, f2.sign, "f2 sign");
        }
    }
}