using System.Collections.Generic;

namespace Nour.Play {
    public class Map2D {
        private readonly Vector _size;
        private readonly List<Cell> _cells;

        public IList<Cell> Cells => _cells.AsReadOnly();

        public Cell this[int x, int y] {
            get {
                var index = x * _size.Y + y;
                return _cells[index];
            }
            set {
                var index = x * _size.Y + y;
                _cells[index] = value;
            }
        }

        public int XHeightRows { get => _size.X; }

        public int YWidthColumns { get => _size.Y; }

        public Vector Size { get => _size; }

        public int Area { get => _size.Area; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">rows</param>
        /// <param name="y">columns</param>
        public Map2D(int x, int y) : this(new Vector(x, y)) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">Map two-dimensional size, where X - rows (height), and Y - columns (width)</param>
        public Map2D(Vector size) {
            _size = size;
            _cells = new List<Cell>(_size.Area);
            // ? P'haps the direction is a property of the gate, not it's identity.
            for (int i = 0; i < _cells.Capacity; i++) {
                var cell = new Cell(i / _size.Y, i % _size.Y);
                if (cell.X > 0) {
                    cell.Neighbors.Add(this[cell.X - 1, cell.Y]);
                    this[cell.X - 1, cell.Y].Neighbors.Add(cell);
                }
                if (cell.Y > 0) {
                    cell.Neighbors.Add(this[_cells[i].X, _cells[i].Y - 1]);
                    this[_cells[i].X, _cells[i].Y - 1].Neighbors.Add(cell);
                }
                _cells.Add(cell);
            }
        }
    }
}