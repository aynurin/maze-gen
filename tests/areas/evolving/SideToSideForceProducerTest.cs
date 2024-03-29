
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PlayersWorlds.Maps.Areas.Evolving {
    [TestFixture]
    public class SideToSideForceProducerTest : Test {
        private SideToSideForceProducer _forceProducer;

        [SetUp]
        override public void SetUp() {
            base.SetUp();
            _forceProducer = new SideToSideForceProducer(new ForceFormula(), 1);
        }

        [Test]
        public void MapAreaSystemTest_DistancePositiveNormal() {
            var (distance, sign, overlap) =
                _forceProducer.GetAxisDistance(0, 1, 2, 3);
            Assert.That(overlap, Is.False);
            Assert.That(1D, Is.EqualTo(distance));
            Assert.That(-1D, Is.EqualTo(sign));
        }
        [Test]
        public void MapAreaSystemTest_DistancePositiveOverlap() {
            var (distance, sign, overlap) =
                _forceProducer.GetAxisDistance(0, 2, 1, 3);
            Assert.That(overlap, Is.True);
            Assert.That(1D, Is.EqualTo(distance));
            Assert.That(-1D, Is.EqualTo(sign));
        }

        [Test]
        public void MapAreaSystemTest_DistancePositiveCollide() {
            var (distance, sign, overlap) =
                _forceProducer.GetAxisDistance(0, 2, 2, 4);
            Assert.That(overlap, Is.False);
            Assert.That(0D, Is.EqualTo(distance));
            Assert.That(-1D, Is.EqualTo(sign));
        }

        [Test]
        public void MapAreaSystemTest_DistanceCentersMatch() {
            var (distance, sign, overlap) =
                _forceProducer.GetAxisDistance(0, 2, -1, 4);
            Assert.That(overlap, Is.True);
            Assert.That(3D, Is.EqualTo(distance));
            Assert.That(1D, Is.EqualTo(sign));
        }

        [Test]
        public void MapAreaSystemTest_DistanceNegativeNormal() {
            var (distance, sign, overlap) =
                _forceProducer.GetAxisDistance(0, 1, -3, 2);
            Assert.That(overlap, Is.False);
            Assert.That(1D, Is.EqualTo(distance));
            Assert.That(1D, Is.EqualTo(sign));
        }

        [Test]
        public void MapAreaSystemTest_DistanceNegativeOverlap() {
            var (distance, sign, overlap) =
                _forceProducer.GetAxisDistance(0, 2, -2, 3);
            Assert.That(overlap, Is.True);
            Assert.That(1D, Is.EqualTo(distance));
            Assert.That(1D, Is.EqualTo(sign));
        }

        [Test]
        public void MapAreaSystemTest_DistanceNegativeCollide() {
            var (distance, sign, overlap) =
                _forceProducer.GetAxisDistance(0, 2, -4, 4);
            Assert.That(overlap, Is.False);
            Assert.That(0D, Is.EqualTo(distance));
            Assert.That(1D, Is.EqualTo(sign));
        }

        [Test]
        public void GetOpposingForce_WhenCenterMatches_NoOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 0.0;

            // Act
            var result = _forceProducer
                .GetOpposingForce(
                    1.0, 1.0, thisOpposingForce,
                    0.0, 3.0, thisForce);

            // Assert
            Assert.That(thisForce, Is.EqualTo(result.thisForce));
            Assert.That(thisForce, Is.EqualTo(result.opposingForce));
        }

        [Test]
        public void GetOpposingForce_WhenCenterMatches_WithOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 2.5;

            // Act
            var result = _forceProducer
                .GetOpposingForce(
                    1.0, 1.0, thisOpposingForce,
                    0.0, 3.0, thisForce);

            // Assert
            Assert.That(-thisOpposingForce, Is.EqualTo(result.thisForce));
            Assert.That(-thisOpposingForce, Is.EqualTo(result.opposingForce));
        }

        [Test]
        public void GetOpposingForce_CentersInside_NoOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 0.0;

            // Act
            var result = _forceProducer
                .GetOpposingForce(
                    1.0, 1.0, thisOpposingForce,
                    1.0, 1.5, thisForce);

            // Assert
            Assert.That(thisForce, Is.EqualTo(result.thisForce));
            Assert.That(0, Is.EqualTo(result.opposingForce));
        }

        [Test]
        public void GetOpposingForce_CentersInside_WithOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 2.0;

            // Act
            var result = _forceProducer
                .GetOpposingForce(
                    1.0, 1.0, thisOpposingForce,
                    1.0, 1.5, thisForce);

            // Assert
            Assert.That(thisForce, Is.EqualTo(result.thisForce));
            Assert.That(0, Is.EqualTo(result.opposingForce));
        }

        [Test]
        public void GetOpposingForce_CentersOutside_NoOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 0.0;

            // Act
            var result = _forceProducer
                .GetOpposingForce(
                    1.0, 1.0, thisOpposingForce,
                    1.6, 3.0, thisForce);

            // Assert
            Assert.That(thisForce, Is.EqualTo(result.thisForce));
            Assert.That(0, Is.EqualTo(result.opposingForce));
        }

        [Test]
        public void GetOpposingForce_CentersOutside_WithOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 2.0;

            // Act
            var result = _forceProducer
                .GetOpposingForce(
                    1.0, 1.0, thisOpposingForce,
                    1.6, 3.0, thisForce);

            // Assert
            Assert.That(thisForce, Is.EqualTo(result.thisForce));
            Assert.That(0, Is.EqualTo(result.opposingForce));
        }

        [Test]
        public void GetOpposingForce_NoIntersection_NoOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 0.0;

            // Act
            var result = _forceProducer
                .GetOpposingForce(
                    1.0, 1.0, thisOpposingForce,
                    2.0, 3.0, thisForce);

            // Assert
            Assert.That(thisForce, Is.EqualTo(result.thisForce));
            Assert.That(0, Is.EqualTo(result.opposingForce));
        }

        [Test]
        public void GetOpposingForce_NoIntersection_WithOpposingForce() {
            // Arrange
            var thisForce = 1.5;
            var thisOpposingForce = 2.0;

            // Act
            var result = _forceProducer
                .GetOpposingForce(
                    1.0, 1.0, thisOpposingForce,
                    2.0, 3.0, thisForce);

            // Assert
            Assert.That(thisForce, Is.EqualTo(result.thisForce));
            Assert.That(0, Is.EqualTo(result.opposingForce));
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
            var (distance1, sign1, overlap1) = FNormal;
            Assert.That(overlap1, Is.False, "f1 overlap");
            Assert.That(1D, Is.EqualTo(distance1), "f1 distance");
            Assert.That(-1D, Is.EqualTo(sign1), "f1 sign");

            var (distance2, sign2, overlap2) = FNormalR;
            Assert.That(overlap2, Is.False, "f2 overlap");
            Assert.That(1D, Is.EqualTo(distance2), "f2 distance");
            Assert.That(1D, Is.EqualTo(sign2), "f2 sign");
        }

        [Test]
        public void GetAxisDistance_Normal2() {
            var (distance1, sign1, overlap1) = FNormalFar;
            Assert.That(overlap1, Is.False, "f1 overlap");
            Assert.That(4D, Is.EqualTo(distance1), "f1 distance");
            Assert.That(-1D, Is.EqualTo(sign1), "f1 sign");

            var (distance2, sign2, overlap2) = FNormalFarR;
            Assert.That(overlap2, Is.False, "f2 overlap");
            Assert.That(4D, Is.EqualTo(distance2), "f2 distance");
            Assert.That(1D, Is.EqualTo(sign2), "f2 sign");
        }

        [Test]
        public void GetAxisDistance_Collide() {
            var (distance1, sign1, overlap1) = Collide;
            Assert.That(overlap1, Is.False, "f1 overlap");
            Assert.That(0D, Is.EqualTo(distance1), "f1 distance");
            Assert.That(-1D, Is.EqualTo(sign1), "f1 sign");

            var (distance2, sign2, overlap2) = CollideR;
            Assert.That(overlap2, Is.False, "f2 overlap");
            Assert.That(0D, Is.EqualTo(distance2), "f2 distance");
            Assert.That(1D, Is.EqualTo(sign2), "f2 sign");
        }

        [Test]
        public void GetAxisDistance_Overlap1() {
            var (distance1, sign1, overlap1) = OverlapShallow;
            Assert.That(overlap1, Is.True, "f1 overlap");
            Assert.That(1D, Is.EqualTo(distance1), "f1 distance");
            Assert.That(-1D, Is.EqualTo(sign1), "f1 sign");

            var (distance2, sign2, overlap2) = OverlapShallowR;
            Assert.That(overlap2, Is.True, "f2 overlap");
            Assert.That(1D, Is.EqualTo(distance2), "f2 distance");
            Assert.That(1D, Is.EqualTo(sign2), "f2 sign");
        }

        [Test]
        public void GetAxisDistance_Overlap2() {
            var (distance1, sign1, overlap1) = OverlapDeep;
            Assert.That(overlap1, Is.True, "f1 overlap");
            Assert.That(2D, Is.EqualTo(distance1), "f1 distance");
            Assert.That(-1D, Is.EqualTo(sign1), "f1 sign");

            var (distance2, sign2, overlap2) = OverlapDeepR;
            Assert.That(overlap2, Is.True, "f2 overlap");
            Assert.That(2D, Is.EqualTo(distance2), "f2 distance");
            Assert.That(1D, Is.EqualTo(sign2), "f2 sign");
        }

        [Test]
        public void GetAxisDistance_Contain1() {
            var (distance1, sign1, overlap1) = ContainTouch;
            Assert.That(overlap1, Is.True, "f1 overlap");
            Assert.That(2D, Is.EqualTo(distance1), "f1 distance");
            Assert.That(-1D, Is.EqualTo(sign1), "f1 sign");

            var (distance2, sign2, overlap2) = ContainTouchR;
            Assert.That(overlap2, Is.True, "f2 overlap");
            Assert.That(2D, Is.EqualTo(distance2), "f2 distance");
            Assert.That(1D, Is.EqualTo(sign2), "f2 sign");
        }

        [Test]
        public void GetAxisDistance_Contain2() {
            var (distance1, sign1, overlap1) = ContainMatchCenter;
            Assert.That(overlap1, Is.True, "f1 overlap");
            Assert.That(4D, Is.EqualTo(distance1), "f1 distance");
            Assert.That(1D, Is.EqualTo(sign1), "f1 sign");

            var (distance2, sign2, overlap2) = ContainMatchCenterR;
            Assert.That(overlap2, Is.True, "f2 overlap");
            Assert.That(4D, Is.EqualTo(distance2), "f2 distance");
            Assert.That(1D, Is.EqualTo(sign2), "f2 sign");
        }
    }
}