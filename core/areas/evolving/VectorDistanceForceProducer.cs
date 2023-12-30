
using System;
using System.Linq;

namespace PlayersWorlds.Maps.Areas.Evolving {
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
            var thisV = CropWithBox2D(direction, area.Size);
            var otherV = VectorD.Zero2D - CropWithBox2D((VectorD.Zero2D - direction), other.Size);

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
            _log?.Buffered.D(5, $"GetRoomForce({area}, {other}): {caseName},thisV={thisV},otherV={otherV},direction={direction},distance={distance},distance.Magnitude={distance.Magnitude},force={force}");
            return force;
        }

        /// <summary>
        /// Crops the given vector using the given box
        /// </summary>
        /// <param name="vector">The vector to crop will be treated as going out of the center of the box</param>
        /// <param name="box">The box to use to crop the vector</param>
        /// <returns></returns>
        public VectorD CropWithBox2D(VectorD vector, VectorD box) {
            // TODO: I still have X and Y all messed up.
            double x, y;
            var rt = box / 2;
            var rb = new VectorD(-box.X / 2, box.Y / 2);
            var lb = VectorD.Zero2D - box / 2;
            var lt = new VectorD(box.X / 2, -box.Y / 2);
            var alphaT = Math.Atan2(rt.X, rt.Y);
            var alphaL = Math.Atan2(lt.X, lt.Y);
            var alphaB = Math.Atan2(lb.X, lb.Y);
            var alphaR = Math.Atan2(rb.X, rb.Y);
            var alpha = Math.Atan2(vector.X, vector.Y);
            if (alpha >= alphaT && alpha <= alphaL) {
                // v crosses the top edge
                x = box.X / 2;
                y = x * Math.Tan(Math.PI / 2 - alpha);
            } else if (alpha > alphaL || alpha < alphaB) {
                // v crosses the left edge
                y = -box.Y / 2;
                x = y * Math.Tan(Math.PI + alpha);
            } else if (alpha <= alphaR && alpha >= alphaB) {
                // v crosses the bottom edge
                x = -box.X / 2;
                y = x * Math.Tan(Math.PI / 2 - alpha);
            } else {
                // v crosses the right edge
                y = box.Y / 2;
                x = y * Math.Tan(alpha);
            }
            return new VectorD(x, y);
        }
    }
}