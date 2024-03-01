using System;
using System.Collections.Generic;

namespace PlayersWorlds.Maps.Areas.Evolving {
    internal class SideToSideForceProducer :
        IAreaForceProducer, IEnvironmentForceProducer {
        private readonly IForceFormula _forceFormula;
        private readonly double _overlapFactor;
        private readonly Dictionary<(FloatingArea, FloatingArea), VectorD>
            _opposingForces = new Dictionary<(FloatingArea, FloatingArea), VectorD>();

        public SideToSideForceProducer(
            IForceFormula forceFormula, double overlapFactor) {
            _forceFormula = forceFormula;
            _overlapFactor = overlapFactor;
        }

        public VectorD GetAreaForce(FloatingArea area, FloatingArea other) {
            // the rooms are touching each other, force = 1/0.1
            // the rooms are at distance from each other, force = 1/distance
            // the rooms overlap, boost the rooms away from each other, force = (distance + 1) / timeBoost
            // !! We can't rely on just a center-to-center vector to determine
            // !! the distance between two areas. E.g., consider the following
            // !! example:
            // !!            ┌─┐
            // !!            │B│
            // !!            │ │
            // !!            │*│
            // !!            │ │
            // !! ┌──────────┼─┼┐
            // !! │    A *   └─┘│
            // !! └─────────────┘
            // !! The A(center)->B(center) vector shows a large distance which
            // !! is not the case.
            // !! In case of an overlap, we can take a vector from the center of
            // !! area A to the center of the overlap.
            // !! At the same time, if the box is fully contained by another box,
            // !! cropping will result in competing between two similar boxes, which
            // !! is less effective than we can do, e.g., we can check the
            // !! centers to not  be inside the other box.
            // !! But if the areas don't overlap, measuring the distance using
            // !! vectors is still not optimal. Consider this example:
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
            // !! We can fix this by measuring the distance between sides on one
            // !! axis. This is not optimal as well, consider this example:
            // !! ╔════════════════════════════════════════════════════════════════════════════════════════╗                                                 
            // !! ║                       ┌─────────────────┐   ┌─────────────────┐                        ║                                                 
            // !! ║                       └─────────────────┘───────────┐─────────┘                        ║                                                 
            // !! ║                           └─────────────────────────┘                                  ║                                                 
            // !! ╚════════════════════════════════════════════════════════════════════════════════════════╝ 
            // !! if the areas are pushed away taking only the vertical distance,
            // !! this layout cannot be enhanced. On the other hand, vector
            // !! distance would allow spreading the areas wider in the env.
            // the rooms are at distance from each other, force = 1/distance
            var dX = GetAxisDistance(area.Position.X, area.Size.X, other.Position.X, other.Size.X);
            var dY = GetAxisDistance(area.Position.Y, area.Size.Y, other.Position.Y, other.Size.Y);
            var fX = GetAxisForce(dX, "X", dY, _overlapFactor);
            var fY = GetAxisForce(dY, "Y", dX, _overlapFactor);
            // if the rooms are diagonal from each other, i.e.,
            // dX.overlap = false && dY.overlap = false, then both fX and fY
            // will be 0. So we need to get the force separately:
            var distance = new VectorD(dX.distance * dX.sign, dY.distance * dY.sign);
            if (!dX.overlap && !dY.overlap) {
                if (distance.MagnitudeSq < VectorD.MIN) {
                    fX.caseName = fY.caseName = "COLLIDE";
                    fX.force = _forceFormula.CollideForce(dX.sign, _overlapFactor);
                    fY.force = _forceFormula.CollideForce(dY.sign, _overlapFactor);
                } else {
                    fX.caseName = fY.caseName = "NORMAL";
                    fX.force = _forceFormula.NormalForce(dX.distance * dX.sign);
                    fY.force = _forceFormula.NormalForce(dY.distance * dY.sign);
                }
            }
            var opposingForce = VectorD.Zero2D;
            if (_opposingForces.ContainsKey((area, other))) {
                opposingForce = _opposingForces[(area, other)];
                _opposingForces.Remove((area, other));
            }
            var otherOpposingForceX = 0D;
            var (thisForceX, opposingForceX) = GetOpposingForce(
                area.Position.X, area.Size.X, opposingForce.X,
                other.Position.X, other.Size.X, fX.force);
            fX.force = thisForceX;
            if (Math.Abs(opposingForceX) > VectorD.MIN) {
                otherOpposingForceX = opposingForceX;
            }
            var otherOpposingForceY = 0D;
            var (thisForceY, opposingForceY) = GetOpposingForce(
                area.Position.Y, area.Size.Y, opposingForce.Y,
                other.Position.Y, other.Size.Y, fY.force);
            fY.force = thisForceY;
            if (Math.Abs(opposingForceY) > VectorD.MIN) {
                otherOpposingForceY = opposingForceY;
            }
            if (!otherOpposingForceX.IsZero() || !otherOpposingForceY.IsZero()) {
                var otherOpposingForce = new VectorD(otherOpposingForceX, otherOpposingForceY);
                if (_opposingForces.ContainsKey((other, area))) {
                    _opposingForces[(other, area)] = otherOpposingForce;
                } else {
                    _opposingForces.Add((other, area), otherOpposingForce);
                }
            }
            var force = new VectorD(fX.force, fY.force);

            // Console.WriteLine($"GetRoomForce (({area.Nickname}), ({other.Nickname})): {fX.caseName},{fY.caseName},thisForce={thisForceX:F2}x{thisForceY:F2},distance={distance},opposingForce={opposingForceX:F2}x{opposingForceY:F2},force={force}");
            return force;
        }

        public VectorD GetEnvironmentForce(FloatingArea area, Vector environmentSize) {
            // direction is always to the center of the area
            // the area touches env edge from the inside, force = 1/0.1
            // the area is somewhere inside the env, force = 1/distance
            // the area is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost
            var env = FloatingArea.Unlinked(VectorD.Zero2D,
                new VectorD(environmentSize));
            var forceX =
                GetEnvAxisForce(area.HighX - env.HighX, area.LowX - env.LowX);
            var forceY =
                GetEnvAxisForce(area.HighY - env.HighY, area.LowY - env.LowY);

            // TODO: Trace: var distance = new VectorD(distanceX, distanceY);
            var force = new VectorD(forceX, forceY);
            // TODO: Trace: _log?.Buffered.D(5, $"GetMapForce (({area}), {env.Size}): {caseX},{caseY},distance={distance},force={force}");
            // Console.WriteLine($"GetEnvironmentForce ({area.Nickname}, {area}, {environmentSize}): force={force}");
            return force;
        }

        /// <summary>
        /// Returns force calculated using the provided <see
        /// cref="IForceFormula"/>.
        /// </summary>
        /// <param name="distanceHigh">Distance between high edges of the area and env</param>
        /// <param name="distanceLow">Distance between low edges of the area and env</param>
        public double
        GetEnvAxisForce(double distanceHigh, double distanceLow) {
            var distance = Math.Abs(distanceHigh) < Math.Abs(distanceLow) ?
                distanceHigh : distanceLow;
            if (Math.Abs(distanceLow + distanceHigh) < 0.1) {
                // TODO: Trace: caseName = "CENTER";
                return 0D;
            }
            double force;
            if (distanceHigh >= -VectorD.MIN && distanceLow <= VectorD.MIN) {
                // the area is larger than env, won't do anything.
                // TODO: Trace: caseName = "OVERSIZE";
                // TODO: Trace: distance = 0;
                force = 0;
            } else if (distanceHigh >= VectorD.MIN || distanceLow <= -VectorD.MIN) {
                // the area is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost
                // TODO: Trace: caseName = "OVERLAP";
                force = _forceFormula.OverlapForce(-distance, _overlapFactor);
            } else if (Math.Abs(distance) <= VectorD.MIN) {
                // TODO: Trace: caseName = "COLLIDE";
                // TODO: Trace: distance = 0;
                force = Math.Abs(distanceHigh) < Math.Abs(distanceLow) ?
                    _forceFormula.CollideForce(-1, _overlapFactor) :
                    _forceFormula.CollideForce(1, _overlapFactor);
            } else {
                // TODO: Trace: caseName = "NORMAL";
                force = _forceFormula.NormalForce(distance);
            }
            return force;
        }

        /// <summary>
        /// If this area has an opposing force set, use it as it's force.
        /// Otherwise, use new force and set the opposing force of the other
        /// area to the new force.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// When two areas centers match, we can't determine the direction of the
        /// force applied to any of those areas. We want them to move in
        /// different direction. We can't use random direction, because the areas
        /// will have a chance to move in the same direction. So we need to
        /// pick a direction for one area, and then choose an opposite direction
        /// for the other area.
        /// </remarks>
        public (double thisForce, double opposingForce)
            GetOpposingForce(
            double thisPosition, double thisSize, double thisOpposingForce,
            double otherPosition, double otherSize, double newForce) {
            double thisForce, opposingForce;
            var centerDX = thisPosition + (thisSize / 2) - otherPosition - (otherSize / 2);
            if (Math.Abs(centerDX) < VectorD.MIN) {
                if (Math.Abs(thisOpposingForce) > VectorD.MIN) {
                    thisForce = -thisOpposingForce;
                    opposingForce = -thisOpposingForce;
                } else {
                    thisForce = newForce;
                    opposingForce = newForce;
                }
            } else {
                thisForce = newForce;
                opposingForce = 0;
            }
            return (thisForce, opposingForce);
        }

        public (double force, string caseName) GetAxisForce(
            (double distance, double sign, bool overlap) thisAxisDistance,
            string thisAxisName,
            (double distance, double sign, bool overlap) otherAxisDistance,
            double fragment) {
            var force = 0D;
            var caseName = "NONE";
            if (otherAxisDistance.overlap) {
                if (thisAxisDistance.overlap && thisAxisDistance.distance <= otherAxisDistance.distance) {
                    caseName = "OVERLAP";
                    force = _forceFormula.OverlapForce(thisAxisDistance.distance * thisAxisDistance.sign, fragment);
                } else if (thisAxisDistance.distance < VectorD.MIN) {
                    caseName = "COLLIDE";
                    force = _forceFormula.CollideForce(thisAxisDistance.sign, fragment);
                } else {
                    caseName = "NORMAL";
                    force = _forceFormula.NormalForce(thisAxisDistance.distance * thisAxisDistance.sign);
                }
            }
            // Console.WriteLine($"GetAxisForce(): ({force}, {caseName}_{thisAxisName})");
            return (force, caseName + "_" + thisAxisName);
        }

        /// <summary>
        /// Finds the shortest distance between two area sides.
        /// </summary>
        /// <param name="onePosition"></param>
        /// <param name="oneSize"></param>
        /// <param name="otherPosition"></param>
        /// <param name="otherSize"></param>
        /// <returns><li>absolute <c>distance</c> between the sides;</li>
        /// <li><c>sign</c> to apply force to increase the distance;</li>
        /// <li>and detects if the areas <c>overlap</c>.</li></returns>
        public (double distance, double sign, bool overlap) GetAxisDistance(double onePosition, double oneSize, double otherPosition, double otherSize) {
            var overlap = false;
            var sign = 1;
            var dx = Math.Max(onePosition - (otherPosition + otherSize), otherPosition - (onePosition + oneSize));
            if (dx < 0) {
                overlap = true;
                dx = -dx;
            }
            if (onePosition + (oneSize / 2) < otherPosition + (otherSize / 2)) {
                // if centers match, the direction is always 1
                // the opposing force is managed up the stack which will cause
                // the other area to move in the opposite direction.
                sign = -1;
            }
            // Console.WriteLine($"GetAxisDistance({onePosition}, {oneSize}, {otherPosition}, {otherSize}): ({dx}, {sign}, {overlap})");
            return (dx, sign, overlap);
        }
    }
}