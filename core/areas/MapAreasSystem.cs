

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nour.Play.Areas {
    public class MapAreasSystem : SimulatedSystem {
        private static readonly string[] NICKNAMES = new[] {
            "BEAR", "LION", "WOLF", "FOXY", "DEER", "MOOS", "ELKK", "HARE", "RABB", "OTTR", "PUMA", "HYNA", "PNDA", "CHTA", "RINO", "BSON", "ZEBR", "ORCA", "PENG"
        };
        private readonly Dictionary<FloatingArea, string> _nicknames =
            new Dictionary<FloatingArea, string>();
        private readonly VectorD _envSize;
        private readonly IList<FloatingArea> _areas;
        private readonly Log _log;
        private readonly Action<GenerationImpact> _onGeneration;
        private readonly Action<EpochResult> _onEpoch;
        private readonly Dictionary<FloatingArea, Dictionary<FloatingArea, VectorD>> _opposingForces = new Dictionary<FloatingArea, Dictionary<FloatingArea, VectorD>>();

        public MapAreasSystem(
            Log log,
            Vector envSize,
            IEnumerable<MapArea> areas,
            Action<GenerationImpact> onGeneration,
            Action<EpochResult> onEpoch) {
            _envSize = new VectorD(envSize);
            _areas = areas.Select(area => FloatingArea.FromMapArea(area))
                          .ToList();
            foreach (var area in _areas) {
                _nicknames.Add(area, NICKNAMES[_nicknames.Count]);
            }
            _log = log;
            _onGeneration = impact => onGeneration?.Invoke(impact);
            _onEpoch = epoch => onEpoch?.Invoke(epoch);
        }

        public override GenerationImpact Evolve(double fragment) {
            var areasForces = new List<VectorD>();
            // get epoch forces
            foreach (var area in _areas) {
                var force = GetOtherAreasForce(area, fragment);
                // opposing force
                force /= 2;
                force += GetEnvironmentForce2(area, fragment);
                // compensate opposing force
                areasForces.Add(force);
                _log.Buffered.D(5, $"OverallForce({area}): {force}");
            }
            // apply epoch force in this generation
            for (var i = 0; i < _areas.Count; i++) {
                _areas[i].AdjustPosition(areasForces[i] * fragment);
            }
            _opposingForces.Clear();
            var impact = new _GenerationImpact(
                IsLayoutValid(),
                areasForces,
                _areas.Select(area => area.Position));
            _onGeneration(impact);
            return impact;
        }

        public bool IsLayoutValid() {
            var envArea = FloatingArea.Unlinked(VectorD.Zero2D, _envSize);
            var overlapping =
                _areas.Where(block =>
                    _areas.Any(other =>
                        block != other && block.Overlaps(other)))
                .ToList();
            var outOfBounds =
                _areas.Where(block =>
                    !block.Fits(envArea))
                .ToList();

            return overlapping.Count == 0 && outOfBounds.Count == 0;
        }

        public override EpochResult CompleteEpoch(
            EpochResult[] previousEpochsResults,
            GenerationImpact[] thisEpochGenerationsImpacts) {
            // snap all areas to grid at the end of each epoch:
            for (var i = 0; i < _areas.Count; i++) {
                _areas[i].AdjustPosition();
            }
            var roomPositions = _areas.Select(
                area => area.Position.RoundToInt()).ToList();
            // measure impact by comparing new area positions to previous area
            // positions
            var roomsShifts = roomPositions.Select((position, i) =>
                (!(previousEpochsResults.LastOrDefault() is
                    _EpochResult previousImpact)) ?
                    position :
                    previousImpact.RoomPositions[i] - position).ToList();
            var epochResult = new _EpochResult(
                previousEpochsResults.Length,
                roomPositions,
                roomsShifts.Aggregate((acc, a) => acc + a),
                roomsShifts.Select(shift => shift.MagnitudeSq).Stats(),
                IsLayoutValid());
            // complete evolution when there room shifts are minimal
            epochResult.CompleteEvolution =
                epochResult.Stats.mode == 0 && epochResult.Stats.variance <= 0.1;

            _log.Buffered.D(4, $"CompleteEpoch(): {epochResult.DebugString()}, {epochResult.Stats.DebugString()}");

            _onEpoch(epochResult);

            return epochResult;
        }

        private const double K = 1;

        public static double NormalForceImpl2(double distance) {
            var sign = Math.Sign(distance);
            // cap distance between 0 and 10
            distance = Math.Min(Math.Abs(distance), 10);
            // the force is capped between 0 and 3
            var force = (3.3 / (distance + 1)) - 0.3;
            return force * sign;
        }
        public static double NormalForceImpl1(double distance) => Math.Abs(distance) < 1 ? Math.Sign(distance) * K : K / distance;
        public static double NormalForce(double distance) => NormalForceImpl2(distance);

        /// <summary>
        /// Gets force between colliding objects. 
        /// </summary>
        /// <param name="sign">Indicates the collision sign.</param>
        /// <param name="fragment">Fragment size to boost time.</param>
        /// <returns></returns>
        public static double CollideForce(double sign, double fragment) => K / fragment * Math.Sign(sign);

        /// <summary>
        /// Gets force between overlapping objects. 
        /// </summary>
        /// <param name="distance">Indicates how much the objects overlap in system units.</param>
        /// <param name="fragment">Fragment size to boost time.</param>
        public static double OverlapForce(double distance, double fragment) => (distance + Math.Sign(distance)) / fragment;

        public VectorD GetEnvironmentForce2(FloatingArea area, double fragment) {
            // direction is always to the center of the area
            // the area touches env edge from the inside, force = 1/0.1
            // the area is somewhere inside the env, force = 1/distance
            // the area is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost
            var env = FloatingArea.Unlinked(VectorD.Zero2D, _envSize);

            var (forceX, distanceX, caseX) =
                GetEnvAxisForce(area.HighX - env.HighX, area.LowX - env.LowX, fragment);
            var (forceY, distanceY, caseY) =
                GetEnvAxisForce(area.HighY - env.HighY, area.LowY - env.LowY, fragment);

            var distance = new VectorD(distanceX, distanceY);
            var force = new VectorD(forceX, forceY);
            _log.Buffered.D(5, $"GetMapForce ({_nicknames[area]}({area}), {env.Size}): {caseX},{caseY},distance={distance},force={force}");
            return force;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distanceTop">Distance between high edges of the area and env</param>
        /// <param name="distanceBottom">Distance between low edges of the area and env</param>
        /// <param name="fragment"></param>
        /// <returns></returns>
        public (double force, double distance, string caseName)
        GetEnvAxisForce(double distanceHigh, double distanceLow, double fragment) {
            var distance = Math.Abs(distanceHigh) < Math.Abs(distanceLow) ?
                distanceHigh : distanceLow;
            if (Math.Abs(distanceLow + distanceHigh) < 0.1) {
                return (0D, 0D, "CENTER");
            }
            double force;
            string caseName;
            // todo: implement test for the failing scenario
            if (distanceHigh >= -VectorD.MIN && distanceLow <= VectorD.MIN) {
                // the area is larger than env, won't do anything.
                caseName = "OVERSIZE";
                distance = 0;
                force = 0;
            } else if (distanceHigh >= VectorD.MIN || distanceLow <= -VectorD.MIN) {
                // the area is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost
                caseName = "OVERLAP";
                force = OverlapForce(-distance, fragment);
            } else if (Math.Abs(distance) <= VectorD.MIN) {
                // TODO: test area size = env size, centers shifted less than Epsilon, VectorD.MIN, 0.1
                caseName = "COLLIDE";
                distance = 0;
                if (Math.Abs(distanceHigh) < Math.Abs(distanceLow)) {
                    force = CollideForce(-1, fragment);
                } else {
                    force = CollideForce(1, fragment);
                }
            } else {
                caseName = "NORMAL";
                force = NormalForce(distance);
            }
            return (force, distance, caseName);
        }

        public VectorD GetEnvironmentForce(FloatingArea area, double fragment) {
            // direction is always to the center of the area
            // the area touches env edge from the inside, force = 1/0.1
            // the area is somewhere inside the env, force = 1/distance
            // the area is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost
            var envArea = FloatingArea.Unlinked(VectorD.Zero2D, _envSize);
            var distanceCenterToCenter = area.Center - _envSize / 2;
            var envCenterRay = distanceCenterToCenter.CropWithBox2D(_envSize);
            var roomCenterRay = envCenterRay.CropWithBox2D(area.Size);
            string xCase = "NONE";
            string yCase = "NONE";
            VectorD distance, force;
            double forceX, forceY;
            double distanceX, distanceY;
            if (area.HighX > envArea.HighX + VectorD.MIN && area.LowX < envArea.LowX - VectorD.MIN) {
                // the area is larger than env, won't do anything.
                xCase = "OVERSIZE";
                distanceX = 0;
                forceX = 0;
            } else if (area.HighX + 0.1 <= envArea.HighX && area.LowX + 0.1 >= envArea.LowX) {
                xCase = "NORMAL";
                // the distance is the distance between env edge and area boundaries
                var distanceHigh = area.HighX - envArea.HighX;
                var distanceLow = area.LowX - envArea.LowX;
                if (-distanceHigh < distanceLow) {
                    distanceX = distanceHigh;
                } else {
                    distanceX = distanceLow;
                }
                forceX = NormalForce(distanceX);
            } else if (Math.Abs(area.HighX - envArea.HighX) < 0.1) {
                // the area touches env edge from the inside, force = 1/0.1
                xCase = "COLLIDE";
                distanceX = area.HighX - envArea.HighX;
                forceX = CollideForce(-1, fragment);
            } else if (Math.Abs(area.LowX - envArea.LowX) < 0.1) {
                // the area touches env edge from the inside, force = 1/0.1
                xCase = "COLLIDE";
                distanceX = area.LowX - envArea.LowX;
                forceX = CollideForce(1, fragment);
            } else if (area.HighX > envArea.HighX) {
                // the area is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost
                xCase = "OVERLAP";
                distanceX = envArea.HighX - area.HighX;
                forceX = OverlapForce(distanceX, fragment);
            } else if (area.LowX < envArea.LowX) {
                // the area is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost
                xCase = "OVERLAP";
                distanceX = envArea.LowX - area.LowX;
                forceX = OverlapForce(distanceX, fragment);
            } else {
                throw new InvalidOperationException($"Unknown X case: {area}, {envArea}");
            }

            if (area.HighY > envArea.HighY + VectorD.MIN && area.LowY < envArea.LowY - VectorD.MIN) {
                // the area is larger than env, won't do anything.
                xCase = "OVERSIZE";
                distanceY = 0;
                forceY = 0;
            } else if (area.HighY < envArea.HighY - VectorD.MIN && area.LowY > envArea.LowY + VectorD.MIN) {
                yCase = "NORMAL";
                // the distance is the distance between env edge and area boundaries
                var distanceHigh = area.HighY - envArea.HighY;
                var distanceLow = area.LowY - envArea.LowY;
                if (-distanceHigh < distanceLow) {
                    distanceY = distanceHigh;
                } else {
                    distanceY = distanceLow;
                }
                forceY = NormalForce(distanceY);
            } else if (Math.Abs(area.HighY - envArea.HighY) < 0.1) {
                // the area touches env edge from the inside, force = 1/0.1
                yCase = "COLLIDE";
                distanceY = area.HighY - envArea.HighY;
                forceY = CollideForce(-1, fragment);
            } else if (Math.Abs(area.LowY - envArea.LowY) < 0.1) {
                // the area touches env edge from the inside, force = 1/0.1
                yCase = "COLLIDE";
                distanceY = area.LowY - envArea.LowY;
                forceY = CollideForce(1, fragment);
            } else if (area.HighY > envArea.HighY) {
                // the area is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost
                yCase = "OVERLAP";
                distanceY = envArea.HighY - area.HighY;
                forceY = OverlapForce(distanceY, fragment);
            } else if (area.LowY < envArea.LowY) {
                // the area is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost
                yCase = "OVERLAP";
                distanceY = envArea.LowY - area.LowY;
                forceY = OverlapForce(distanceY, fragment);
            } else {
                throw new InvalidOperationException($"Unknown Y case: {area}, {envArea}");
            }
            distance = new VectorD(distanceX, distanceY);
            force = new VectorD(forceX, forceY);
            _log.Buffered.D(5, $"GetMapForce ({_nicknames[area]}({area}), {envArea.Size}): {xCase},{yCase},distance={distance},force={force}");
            return force;
        }

        public VectorD GetOtherAreasForce(FloatingArea area, double fragment) =>
            _areas.Where(other => other != area)
                  .Select(other => GetAreaForce2(area, other, fragment))
                  .Aggregate(VectorD.Zero2D, (acc, f) => acc + f);

        public VectorD GetAreaForce2(FloatingArea area, FloatingArea other, double fragment) {
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
            var fX = GetAxisForce(dX, "X", dY, fragment);
            var fY = GetAxisForce(dY, "Y", dX, fragment);
            // if the rooms are diagonal from each other, i.e.,
            // dX.overlap = false && dY.overlap = false, then both fX and fY
            // will be 0. So we need to get the force separately:
            var distance = new VectorD(dX.distance * dX.sign, dY.distance * dY.sign);
            if (!dX.overlap && !dY.overlap) {
                if (distance.MagnitudeSq < VectorD.MIN) {
                    fX.caseName = fY.caseName = "COLLIDE";
                    fX.force = CollideForce(dX.sign, fragment);
                    fY.force = CollideForce(dY.sign, fragment);
                } else {
                    fX.caseName = fY.caseName = "NORMAL";
                    fX.force = NormalForce(dX.distance * dX.sign);
                    fY.force = NormalForce(dY.distance * dY.sign);
                }
            }
            var fXo = fX.force;
            var fYo = fY.force;

            var opposingForce = _opposingForces.ContainsKey(area) && _opposingForces[area].ContainsKey(other) ? _opposingForces[area][other] : VectorD.Zero2D;
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
            if (otherOpposingForceX != 0 || otherOpposingForceY != 0) {
                var otherOpposingForce = new VectorD(otherOpposingForceX, otherOpposingForceY);
                if (!_opposingForces.ContainsKey(other))
                    _opposingForces.Add(other, new Dictionary<FloatingArea, VectorD> { [area] = otherOpposingForce });
                else _opposingForces[other].Add(area, otherOpposingForce);
            }
            var force = new VectorD(fX.force, fY.force);

            _log.Buffered.D(5, $"GetRoomForce ({_nicknames[area]}({area}), {_nicknames[other]}({other})): {fX.caseName},{fY.caseName},f={fXo:F2}x{fYo:F2},thisForce={thisForceX:F2}x{thisForceY:F2},distance={distance},opposingForce={opposingForceX:F2}x{opposingForceY:F2},force={force}");
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
        public static (double thisForce, double opposingForce)
            GetOpposingForce(
            double thisPosition, double thisSize, double thisOpposingForce,
            double otherPosition, double otherSize, double newForce) {
            double thisForce, opposingForce;
            var centerDX = thisPosition + thisSize / 2 - otherPosition - otherSize / 2;
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
        public static (double force, string caseName) GetAxisForce(
            (double distance, double sign, bool overlap) thisAxisDistance,
            string thisAxisName,
            (double distance, double sign, bool overlap) otherAxisDistance,
            double fragment) {
            var force = 0D;
            var caseName = "NONE";
            if (otherAxisDistance.overlap) {
                if (thisAxisDistance.overlap && thisAxisDistance.distance <= otherAxisDistance.distance) {
                    caseName = "OVERLAP";
                    force = OverlapForce(thisAxisDistance.distance * thisAxisDistance.sign, fragment);
                } else if (thisAxisDistance.distance < VectorD.MIN) {
                    caseName = "COLLIDE";
                    force = CollideForce(thisAxisDistance.sign, fragment);
                } else {
                    caseName = "NORMAL";
                    force = NormalForce(thisAxisDistance.distance * thisAxisDistance.sign);
                }
            }
            return (force, caseName + "_" + thisAxisName);
        }

        /// <summary>
        /// Finds the shortest distance between two area sides.
        /// </summary>
        /// <param name="onePosition"></param>
        /// <param name="oneSize"></param>
        /// <param name="otherPosition"></param>
        /// <param name="otherSize"></param>
        /// <returns><li>absolute <code>distance</code> between the sides;
        /// <li><code>sign</code> to apply force to increase the distance;
        /// <li>and detects if the areas <code>overlap</code>.</returns>
        public static (double distance, double sign, bool overlap) GetAxisDistance(double onePosition, double oneSize, double otherPosition, double otherSize) {
            var overlap = false;
            var sign = 1;
            var dx = Math.Max(onePosition - (otherPosition + otherSize), otherPosition - (onePosition + oneSize));
            if (dx < 0) {
                overlap = true;
                dx = -dx;
            }
            if (onePosition + oneSize / 2 < otherPosition + otherSize / 2) {
                sign = -1;
            }
            return (dx, sign, overlap);
        }

        public VectorD GetAreaForce(FloatingArea area, FloatingArea other, double fragment) {
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

            string caseName = "NONE";
            VectorD force, distance;
            if (VectorD.MIN > Math.Abs((thisV + otherV).MagnitudeSq - direction.MagnitudeSq)) {
                // the rooms are touching each other, force = 1/0.1
                distance = new VectorD(direction.Value.Select(v => v > 0.1 ? 0.1 : v < -0.1 ? -0.1 : v));
                force = distance.WithMagnitude(CollideForce(distance.Magnitude, fragment));
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
                force = distance.WithMagnitude(NormalForce(distance.Magnitude));
                caseName = "NORMAL";
            } else {
                // rooms overlap
                distance = thisV - direction + otherV;
                // boost the rooms away from each other, force = (distance + 1) / timeBoost
                var forceX = OverlapForce(distance.X, fragment);
                var forceY = OverlapForce(distance.Y, fragment);
                force = new VectorD(forceX, forceY);
                caseName = "OVERLAP";
            }
            _log.Buffered.D(5, $"GetRoomForce({area}, {other}): {caseName},thisV={thisV},otherV={otherV},direction={direction},distance={distance},distance.Magnitude={distance.Magnitude},force={force}");
            return force;
        }

        public class _GenerationImpact : GenerationImpact {
            public _GenerationImpact(bool layoutIsValid,
                IEnumerable<VectorD> forces, IEnumerable<VectorD> positions) {
                LayoutIsValid = layoutIsValid;
                Forces = new List<VectorD>(forces);
                Positions = new List<VectorD>(positions);
            }

            public bool LayoutIsValid { get; private set; }
            public IList<VectorD> Forces { get; private set; }
            public IList<VectorD> Positions { get; private set; }
        }

        public class _EpochResult : EpochResult {
            public _EpochResult(
                int epoch,
                List<Vector> roomPositions,
                Vector totalRoomsShift, BaseStats stats,
                bool layoutIsValid) {
                Epoch = epoch;
                RoomPositions = roomPositions;
                TotalRoomsShift = totalRoomsShift;
                Stats = stats;
                LayoutIsValid = layoutIsValid;
            }

            public int Epoch { get; }
            public List<Vector> RoomPositions { get; }
            public Vector TotalRoomsShift { get; }
            public BaseStats Stats { get; }
            public bool LayoutIsValid { get; }

            public override string ToString() => this.DebugString();
        }

        public class FloatingArea {
            private readonly MapArea _linkedArea;
            public VectorD Position { get; private set; }
            public VectorD Size { get; }

            public VectorD Center => Position + Size / 2D;
            public VectorD OpposingForce { get; set; } = VectorD.Zero2D;

            public double LowX => Position.X;
            public double HighX => Position.X + Size.X;
            public double LowY => Position.Y;
            public double HighY => Position.Y + Size.Y;

            private FloatingArea(MapArea area, VectorD position, VectorD size) {
                _linkedArea = area;
                Position = position;
                Size = size;
            }

            public static FloatingArea FromMapArea(MapArea area) {
                return new FloatingArea(
                    area,
                    new VectorD(area.Position),
                    new VectorD(area.Size));
            }

            public static FloatingArea Unlinked(VectorD position, VectorD size) {
                return new FloatingArea(null, position, size);
            }

            public static FloatingArea Parse(string s) {
                var parameters = s.Trim(' ').Split(';').Select(x =>
                    VectorD.Parse(x.Trim('P', 'S'))).ToArray();
                return new FloatingArea(null, parameters[0], parameters[1]);
            }

            public void AdjustPosition(VectorD d) {
                this.Position += d;
                if (this._linkedArea != null) {
                    this._linkedArea.Position = this.Position.RoundToInt();
                }
            }

            /// <summary>
            /// Snaps the position to the integer grid.
            /// </summary>
            public void AdjustPosition() {
                var roundedPosition = this.Position.RoundToInt();
                this.Position = new VectorD(roundedPosition);
                if (this._linkedArea != null) {
                    this._linkedArea.Position = roundedPosition;
                }
            }

            public bool Overlaps(FloatingArea other) {
                if (this == other)
                    throw new InvalidOperationException("Can't compare with self");
                var noOverlap = this.HighX <= other.LowX || this.LowX >= other.HighX;
                noOverlap |= this.HighY <= other.LowY || this.LowY >= other.HighY;
                return !noOverlap;
            }

            public bool Contains(VectorD point) {
                return point.X >= Position.X && point.X <= Size.X + Position.X &&
                    point.Y >= Position.Y && point.Y <= Size.Y + Position.Y;
            }

            public bool Fits(FloatingArea other) {
                // Check if the inner rectangle is completely within the outer rectangle.
                return this.LowX >= other.LowX &&
                    this.HighX <= other.HighX &&
                    this.LowY >= other.LowY &&
                    this.HighY <= other.HighY;
            }

            public override string ToString() {
                return $"P{Position};S{Size}";
            }
        }
    }
}