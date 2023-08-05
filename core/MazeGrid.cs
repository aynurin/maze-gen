using System.Collections.Generic;
using System.Linq;

namespace Nour.Play.Maze {
    public class MazeGrid {
        private readonly List<MazeCell> _cells;
        private readonly Size _size;

        public IList<MazeCell> Cells => _cells.AsReadOnly();

        public List<MazeCell> FindDeadEnds() {
            return _cells.FindAll(cell => cell.IsDeadEnd);
        }

        public MazeCell this[int row, int col] {
            get {
                var index = row * _size.Columns + col;
                return _cells[index];
            }
            set {
                var index = row * _size.Columns + col;
                _cells[index] = value;
            }
        }

        public int Rows { get => _size.Rows; }

        public int Cols { get => _size.Columns; }

        public int Size { get => _size.Area; }

        public MazeGrid(Size mazeSize) {
            _size = mazeSize;
            _cells = new List<MazeCell>(_size.Area);
            // TODO: 1. Figure out the placement
            //      pack areas together to mimic the proportions of the map
            //      spread areas on the map
            // TODO: 2. Place no cell for Fill, and the same cell repeatedly for Hall
            // TODO: 3. Hall cells can have many neighbors on any side.
            // ? P'haps the direction is a property of the gate, not it's identity.
            for (int i = 0; i < _cells.Capacity; i++) {
                _cells.Add(new MazeCell(i / _size.Columns, i % _size.Columns));
            }
            for (int i = 0; i < _cells.Capacity; i++) {
                if (_cells[i].Row > 0) {
                    _cells[i].Neighbors.Add(this[_cells[i].Row - 1, _cells[i].Col]);
                }
                if (_cells[i].Row < this.Rows - 1) {
                    _cells[i].Neighbors.Add(this[_cells[i].Row + 1, _cells[i].Col]);
                }
                if (_cells[i].Col > 0) {
                    _cells[i].Neighbors.Add(this[_cells[i].Row, _cells[i].Col - 1]);
                }
                if (_cells[i].Col < this.Cols - 1) {
                    _cells[i].Neighbors.Add(this[_cells[i].Row, _cells[i].Col + 1]);
                }
            }
        }
    }
}