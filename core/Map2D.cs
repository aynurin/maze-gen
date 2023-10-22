using System;
using System.Collections.Generic;
using System.Text;

namespace Nour.Play {
    public class Map2D {
        private List<Cell> _cells;

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

        public Cell CellAt(Vector xy) => _cells[xy.X * Size.Y + xy.Y];

        public IEnumerable<Cell> CellsAt(Vector xy, Vector areaSize) {
            for (var x = xy.X; x < xy.X + areaSize.X; x++) {
                for (var y = xy.Y; y < xy.Y + areaSize.Y; y++) {
                    var i = x * Size.Y + y;
                    yield return _cells[i];
                }
            }
        }

        public override string ToString() {
            var buffer = new StringBuilder();
            for (int x = 0; x < Size.X; x++) {
                for (int y = 0; y < Size.Y; y++) {
                    buffer.Append(
                        _cells[x * Size.Y + y].Type == Cell.CellType.Wall ? "▓" :
                        _cells[x * Size.Y + y].Type == Cell.CellType.Trail ? "░" :
                        _cells[x * Size.Y + y].Type == Cell.CellType.Edge ? "▒" :
                        "0");
                }
                buffer.Append("\n");
            }
            return buffer.ToString();
        }
    }
}