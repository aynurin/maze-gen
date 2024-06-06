using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PlayersWorlds.Maps.Areas.Evolving {
    internal class MapAreasSystem : SimulatedSystem {
        private readonly Log _log = Log.ToConsole<MapAreasSystem>();
        private static readonly string[] s_nicknames = new[] {
            "BEAR", "LION", "WOLF", "FOXY", "DEER", "MOOS", "ELKK", "HARE", "RABB", "OTTR", "PUMA", "HYNA", "PNDA", "CHTA", "RINO", "BSON", "ZEBR", "ORCA", "PENG"
        };
        private readonly RandomSource _random;
        private readonly Vector _envSize;
        private readonly IList<FloatingArea> _areas;
        private readonly Action<GenerationImpact> _onGeneration;
        private readonly Action<EpochResult> _onEpoch;

        internal static IEnumerable<(Area area, string label)>
            GetNicknames(IEnumerable<Area> areas) =>
                areas.Select((a, i) => (area: a, label: s_nicknames[i]));

        public MapAreasSystem(
            RandomSource random,
            Vector envSize,
            IEnumerable<Area> areas,
            Action<GenerationImpact> onGeneration,
            Action<EpochResult> onEpoch) {
            _random = random;
            _envSize = envSize;
            _areas = areas.Select((area, i) => {
                var fa = FloatingArea.FromMapArea(area, envSize);
                fa.Nickname = s_nicknames[i];
                return fa;
            }).ToList();
            _onGeneration = impact => onGeneration?.Invoke(impact);
            _onEpoch = epoch => onEpoch?.Invoke(epoch);
        }

        public override GenerationImpact Evolve(double fragment) {
            var areaForceProducer = new DirectedDistanceForceProducer(
                _random, fragment);
            var envForceProducer = areaForceProducer;
            var areasForces = new List<VectorD>();
            // get epoch forces
            foreach (var area in _areas) {
                if (area.IsPositionFixed) {
                    areasForces.Add(VectorD.Zero2D);
                    continue;
                }
                var areaForces = _areas.Where(other => other != area)
                  .Select(other => areaForceProducer.GetAreaForce(area, other));
                var overallAreaForce = areaForces
                  .Aggregate(VectorD.Zero2D, (acc, f) => acc + f);
                var envForce =
                    envForceProducer.GetEnvironmentForce(area, _envSize);
                areasForces.Add(overallAreaForce + envForce);
                _log.D(5, $"{area.Nickname}: {area}, {overallAreaForce}, {envForce}");
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
                    !block.FitsInto(envArea))
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
                roomsShifts.Count == 0 ? Vector.Zero2D :
                    roomsShifts.Aggregate((acc, a) => acc + a),
                roomsShifts.Select(shift => shift.MagnitudeSq).Stats(),
                IsLayoutValid());
            // complete evolution when the room shifts are minimal
            epochResult.CompleteEvolution =
                epochResult.Stats.Mode == 0 && epochResult.Stats.Variance <= 0.1;

            // TODO(#37): Trace: _log?.Buffered.D(4, $"CompleteEpoch(): {epochResult.DebugString()}, {epochResult.Stats.DebugString()}");

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