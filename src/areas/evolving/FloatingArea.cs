
using System;
using System.Linq;

namespace PlayersWorlds.Maps.Areas.Evolving {
    public class FloatingArea {
        private readonly MapArea _linkedArea;
        public VectorD Position { get; private set; }
        public VectorD Size { get; }

        public VectorD Center => Position + (Size / 2D);
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

        public bool Overlaps(FloatingArea other) {
            if (this == other) {
                throw new InvalidOperationException("Can't compare with self");
            }

            var noOverlap = HighX <= other.LowX || LowX >= other.HighX;
            noOverlap |= HighY <= other.LowY || LowY >= other.HighY;
            return !noOverlap;
        }

        public bool Contains(VectorD point) {
            return point.X >= Position.X && point.X <= Size.X + Position.X &&
                point.Y >= Position.Y && point.Y <= Size.Y + Position.Y;
        }

        public bool Fits(FloatingArea other) {
            // Check if the inner rectangle is completely within the outer rectangle.
            return LowX >= other.LowX &&
                HighX <= other.HighX &&
                LowY >= other.LowY &&
                HighY <= other.HighY;
        }

        public override string ToString() {
            return $"P{Position};S{Size}";
        }
    }
}