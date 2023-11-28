
using System;
using System.Linq;

namespace Nour.Play.Areas.Evolving {
    public class VectorDistanceForceProducer : IAreaForceProducer {
        private readonly Log _log;
        private readonly IForceFormula _forceFormula;
        private readonly double _overlapFactor;

        public VectorDistanceForceProducer(
            Log log, IForceFormula forceFormula, double overlapFactor) {
            _log = log;
            _forceFormula = forceFormula;
            _overlapFactor = overlapFactor;
        }

        public VectorD GetAreaForce(FloatingArea area, FloatingArea other) {
            // the rooms are touching each other, force = 1/0.1
            // the rooms are at distance from each other, force = 1/distance
            // the rooms overlap, boost the rooms away from each other, force = (distance + 1) / timeBoost
            // !! we can't determine if the rooms overlap or touch each other using 
            // !! just a center-to-center vector. E.g., imagine this:
            // !!            ┌─┐
            // !!            │B│
            // !!            │ │
            // !!            │*│
            // !!            │ │
            // !! ┌──────────┼─┼┐
            // !! │    A *   └─┘│
            // !! └─────────────┘
            // !! The A(center)->B(center) vector will have a gap in the center and
            // !! it won't show that the boxes overlap.
            // !! In case of an overlap, we will take a vector from the center of
            // !! this box to the center of the overlap.
            // !! At the same time, if the box is fully contained by another box,
            // !! cropping will result in competing between two similar boxes, which
            // !! is less efficient than we can do, so we check the centers to not
            // !! be inside the other box.
            // Calculate the coordinates of the overlapping rectangle.
            if (area.Overlaps(other) && !area.Contains(other.Center) && !other.Contains(area.Center)) {
                var overlapHighX = Math.Min(area.HighX, other.HighX);
                var overlapHighY = Math.Min(area.HighY, other.HighY);
                var overlapLowX = Math.Max(area.LowX, other.LowX);
                var overlapLowY = Math.Max(area.LowY, other.LowY);
                other = FloatingArea.Unlinked(
                    new VectorD(overlapLowX, overlapLowY),
                    new VectorD(
                        overlapHighX - overlapLowX,
                        overlapHighY - overlapLowY)
                );
            }
            var direction = area.Center - other.Center;
            if (direction.IsZero()) {
                // Room centers match
                if (!area.OpposingForce.IsZero()) {
                    direction = VectorD.Zero2D - area.OpposingForce;
                } else {
                    while (direction.IsZero()) {
                        direction = new VectorD(
                            GlobalRandom.Next(-1, 2) / 10D,
                            GlobalRandom.Next(-1, 2) / 10D
                        );
                    }
                    other.OpposingForce = direction;
                }
            }
            var thisV = direction.CropWithBox2D(area.Size);
            var otherV = VectorD.Zero2D - (VectorD.Zero2D - direction).CropWithBox2D(other.Size);

            var caseName = "NONE";
            VectorD force, distance;
            if (VectorD.MIN > Math.Abs((thisV + otherV).MagnitudeSq - direction.MagnitudeSq)) {
                // the rooms are touching each other, force = 1/0.1
                distance = new VectorD(direction.Value.Select(v => v > 0.1 ? 0.1 : v < -0.1 ? -0.1 : v));
                force = distance.WithMagnitude(
                    _forceFormula.CollideForce(distance.Magnitude, _overlapFactor));
                caseName = "COLLIDE";
            } else if ((thisV + otherV).MagnitudeSq < direction.MagnitudeSq) {
                // TODO: measuring distance using vectors is not optimal, e.g.:
                // !! ║     ┌─────────────────────────┐───┐         ║                                         
                // !! ║     │            *         B` │B  │         ║                                         
                // !! ║     └─────────S2.00x13.00─────┘   │         ║                                         
                // !! ║                            A` │A  │         ║                                         
                // !! ║                               │   │         ║                                         
                // !! ║                               │   │         ║                                         
                // !! ║                            S12.00x2.00      ║                                         
                // !! ║                               │   │         ║                                         
                // !! ║                               │   │         ║                                         
                // !! ║                               │   │         ║                                         
                // !! ║                               │   │         ║                                         
                // !! ║                               │   │         ║                                         
                // !! ║                               └───┘         ║ 
                // !! The vector distance is large, but the actual distance is
                // !! close to 0, so there has to be a force pushing away.
                // the rooms are at distance from each other, force = 1/distance
                distance = direction - thisV - otherV;
                force = distance.WithMagnitude(
                    _forceFormula.NormalForce(distance.Magnitude));
                caseName = "NORMAL";
            } else {
                // rooms overlap
                distance = thisV - direction + otherV;
                // boost the rooms away from each other, force = (distance + 1) / timeBoost
                var forceX = _forceFormula.OverlapForce(distance.X, _overlapFactor);
                var forceY = _forceFormula.OverlapForce(distance.Y, _overlapFactor);
                force = new VectorD(forceX, forceY);
                caseName = "OVERLAP";
            }
            _log.Buffered.D(5, $"GetRoomForce({area}, {other}): {caseName},thisV={thisV},otherV={otherV},direction={direction},distance={distance},distance.Magnitude={distance.Magnitude},force={force}");
            return force;
        }
    }
}