
using System;
using System.Linq;

namespace PlayersWorlds.Maps.Areas.Evolving {
    internal class FloatingArea {
        private readonly MapArea _linkedArea;
        public VectorD Position { get; private set; }
        public VectorD Size { get; }
        public bool IsPositionFixed { get; }

        public VectorD Center => Position + (Size / 2D);
        public VectorD OpposingForce { get; set; } = VectorD.Zero2D;

        public double LowX => Position.X;
        public double HighX => Position.X + Size.X;
        public double LowY => Position.Y;
        public double HighY => Position.Y + Size.Y;

        public string Nickname { get; set; }

        public MapArea LinkedArea => _linkedArea;

        private FloatingArea(MapArea area, VectorD position, bool isPositionFixed, VectorD size) {
            _linkedArea = area;
            Position = position;
            IsPositionFixed = isPositionFixed;
            Size = size;
        }

        public static FloatingArea FromMapArea(MapArea area, Vector envSize) {
            return new FloatingArea(
                area,
                area.IsPositionFixed || !area.IsPositionEmpty ?
                    new VectorD(area.Position) :
                    new VectorD(envSize) / 2D - new VectorD(area.Size) / 2D,
                area.IsPositionFixed,
                new VectorD(area.Size));
        }

        public static FloatingArea Unlinked(VectorD position, VectorD size) {
            return new FloatingArea(null, position, false, size);
        }

        public static FloatingArea Parse(string s) {
            var parameters = s.Trim(' ').Split(';').Select(x =>
                VectorD.Parse(x.Trim('P', 'S'))).ToArray();
            return new FloatingArea(null, parameters[0], false, parameters[1]);
        }

        public void AdjustPosition(VectorD d) {
            Position += d;
            if (_linkedArea != null) {
                _linkedArea.Position = Position.RoundToInt();
            }
        }

        /// <summary>
        /// Snaps the position to the integer grid.
        /// </summary>
        public void AdjustPosition() {
            var roundedPosition = Position.RoundToInt();
            Position = new VectorD(roundedPosition);
            if (_linkedArea != null) {
                _linkedArea.Position = roundedPosition;
            }
        }

        [Obsolete("Use Overlap(FloatingArea other) instead.")]
        public bool Overlaps(FloatingArea other) {
            return this.Overlap(other).MagnitudeSq > VectorD.MIN;
        }

        public VectorD Overlap(FloatingArea other) {
            if (this == other)
                throw new InvalidOperationException("Can't compare with self");

            // Calculate the overlap rectangle coordinates
            var lowX = (double)Math.Max(this.LowX, other.LowX);
            var highX = (double)Math.Min(this.HighX, other.HighX);
            var lowY = (double)Math.Max(this.LowY, other.LowY);
            var highY = (double)Math.Min(this.HighY, other.HighY);

            // Check if there is no overlap
            if (lowX >= highX || lowY >= highY) {
                return VectorD.Zero2D;
            }

            // Calculate the size of the overlap area
            return new VectorD(highX - lowX, highY - lowY);
        }

        public bool Contains(VectorD point) {
            return point.X >= Position.X && point.X <= Size.X + Position.X &&
                point.Y >= Position.Y && point.Y <= Size.Y + Position.Y;
        }

        public bool FitsInto(FloatingArea other) {
            // Check if the inner rectangle is completely within the outer rectangle.
            return LowX >= other.LowX &&
                HighX <= other.HighX &&
                LowY >= other.LowY &&
                HighY <= other.HighY;
        }

        public FloatingArea Intersection(FloatingArea other) {
            if (this == other) {
                throw new InvalidOperationException("Can't intersect with self");
            }

            // Calculate the overlap rectangle coordinates
            var lowX = Math.Max(this.LowX, other.LowX);
            var lowY = Math.Max(this.LowY, other.LowY);

            var highX = Math.Min(this.HighX, other.HighX);
            var highY = Math.Min(this.HighY, other.HighY);

            if (lowX >= highX || lowY >= highY) {
                return Unlinked(VectorD.Zero2D, VectorD.Zero2D);
            } else {
                return Unlinked(
                    new VectorD(lowX, lowY),
                    new VectorD(highX - lowX, highY - lowY));
            }
        }

        public (VectorD d, bool overlap) DistanceTo(FloatingArea other) {
            // areas can overlap, in which case the distance is reversed, and
            // the overlap flag is set to true.

            // case one, other high is higher than this low, but other low is MUCH lower than this high.
            // distance = LowX - other.HighX (negative), overlap = true

            // case two, other high is lower than this low
            // distance = LowX - other.HighX (positive), overlap = false 

            // case three, other low is lower than this high, but other high is MUCH higher than this low.
            // distance = HighX - other.LowY (positive), overlap = true

            // case four, other low is higher than this high
            // distance = HighX - other.LowY (negative), overlap = false

            var (overlapX, dX) = Distance1D(LowX, HighX, other.LowX, other.HighX);
            var (overlapY, dY) = Distance1D(LowY, HighY, other.LowY, other.HighY);

            var d = new VectorD(dX, dY);

            var overlap = overlapX && overlapY;

            return (d, overlap);

            (bool o, double d) Distance1D(double aLow, double aHigh, double bLow, double bHigh) {
                var m1 = Math.Max(aLow - bHigh, aHigh - bLow);
                var m2 = Math.Min(aLow - bHigh, aHigh - bLow);
                return (o: m1 > 0 && m2 < 0,
                 d: Math.Abs(m1) < Math.Abs(m2) ? m1 : (m1 + m2 == 0) ? 0 : m2);
            }
        }

        public override string ToString() {
            return $"P{Position};S{Size}";
        }
    }
}