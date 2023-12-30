// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Areas.Evolving {
    public class MapAreasSystem : SimulatedSystem {
        private static readonly string[] s_nicknames = new[] {
            "BEAR", "LION", "WOLF", "FOXY", "DEER", "MOOS", "ELKK", "HARE", "RABB", "OTTR", "PUMA", "HYNA", "PNDA", "CHTA", "RINO", "BSON", "ZEBR", "ORCA", "PENG"
        };
        private readonly Dictionary<FloatingArea, string> _nicknames =
            new Dictionary<FloatingArea, string>();
        private readonly Vector _envSize;
        private readonly IList<FloatingArea> _areas;
        private readonly Log _log;
        private readonly Action<GenerationImpact> _onGeneration;
        private readonly Action<EpochResult> _onEpoch;

        public MapAreasSystem(
            Log log,
            Vector envSize,
            IEnumerable<MapArea> areas,
            Action<GenerationImpact> onGeneration,
            Action<EpochResult> onEpoch) {
            _envSize = envSize;
            _areas = areas.Select(area => FloatingArea.FromMapArea(area))
                          .ToList();
            foreach (var area in _areas) {
                _nicknames.Add(area, s_nicknames[_nicknames.Count]);
            }
            _log = log;
            _onGeneration = impact => onGeneration?.Invoke(impact);
            _onEpoch = epoch => onEpoch?.Invoke(epoch);
        }

        public override GenerationImpact Evolve(double fragment) {
            var areaForceProducer = new SideToSideForceProducer(_log, new ForceFormula(), fragment);
            var envForceProducer = areaForceProducer;
            var areasForces = new List<VectorD>();
            // get epoch forces
            foreach (var area in _areas) {
                var force = _areas.Where(other => other != area)
                  .Select(other => areaForceProducer.GetAreaForce(area, other))
                  .Aggregate(VectorD.Zero2D, (acc, f) => acc + f);
                // opposing force
                force /= 2;
                force += envForceProducer.GetEnvironmentForce(area, _envSize);
                // compensate opposing force
                areasForces.Add(force);
                _log?.Buffered.D(5, $"OverallForce({area}): {force}");
            }
            // apply epoch force in this generation
            for (var i = 0; i < _areas.Count; i++) {
                _areas[i].AdjustPosition(areasForces[i] * fragment);
            }
            var impact = new MasGenerationImpact(
                IsLayoutValid(),
                areasForces,
                _areas.Select(area => area.Position));
            _onGeneration(impact);
            return impact;
        }

        public bool IsLayoutValid() {
            var envArea = FloatingArea.Unlinked(
                VectorD.Zero2D, new VectorD(_envSize));
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
                    MasEpochResult previousImpact))
                    ? position
                    : previousImpact.RoomPositions[i] - position).ToList();
            var epochResult = new MasEpochResult(
                previousEpochsResults.Length,
                roomPositions,
                roomsShifts.Aggregate((acc, a) => acc + a),
                roomsShifts.Select(shift => shift.MagnitudeSq).Stats(),
                IsLayoutValid());
            // complete evolution when there room shifts are minimal
            epochResult.CompleteEvolution =
                epochResult.Stats.Mode == 0 && epochResult.Stats.Variance <= 0.1;

            _log?.Buffered.D(4, $"CompleteEpoch(): {epochResult.DebugString()}, {epochResult.Stats.DebugString()}");

            _onEpoch(epochResult);

            return epochResult;
        }


        private class MasGenerationImpact : GenerationImpact {
            public MasGenerationImpact(bool layoutIsValid,
                IEnumerable<VectorD> forces, IEnumerable<VectorD> positions) {
                LayoutIsValid = layoutIsValid;
                Forces = new List<VectorD>(forces);
                Positions = new List<VectorD>(positions);
            }

            public bool LayoutIsValid { get; private set; }
            public IList<VectorD> Forces { get; private set; }
            public IList<VectorD> Positions { get; private set; }
        }

        private class MasEpochResult : EpochResult {
            public MasEpochResult(
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
    }
}