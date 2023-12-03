using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Nour.Play.Renderers;

namespace Nour.Play {
    public class Map2D {
        private readonly List<Cell> _cells;

        // TBD
        public Vector Size { get; private set; }

        public IList<Cell> Cells => _cells.AsReadOnly();

        public Map2D(Vector size) {
            Size = size;
            _cells = new List<Cell>(size.Area);
            for (var i = 0; i < size.Area; i++) {
                _cells.Add(new Cell());
            }
        }

        public Cell CellAt(Vector xy) => _cells[xy.ToIndex(Size.X)];

        public IEnumerable<Cell> CellsAt(Vector xy, Vector areaSize) {
            if (xy.X + areaSize.X > Size.X || xy.Y + areaSize.Y > Size.Y) {
                throw new IndexOutOfRangeException($"Can't retrieve area of size {areaSize} at {xy} in map of size {Size}");
            }
            return AnyCellsAt(xy, areaSize);
        }

        public IEnumerable<Cell> AnyCellsAt(Vector xy, Vector areaSize) {
            for (var x = Math.Max(0, xy.X); x < Math.Min(xy.X + areaSize.X, Size.X); x++) {
                for (var y = Math.Max(0, xy.Y); y < Math.Min(xy.Y + areaSize.Y, Size.Y); y++) {
                    yield return _cells[new Vector(x, y).ToIndex(Size.X)];
                }
            }
        }

        public override string ToString() {
            return new Map2DAsciiRenderer().Render(this);
        }
    }
}