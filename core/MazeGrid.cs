using System.Collections.Generic;
using System.Linq;

public class MazeGrid {
    private readonly List<MazeCell> _cells;
    private readonly Dimensions _dimensions;

    public IList<MazeCell> Cells => _cells.AsReadOnly();

    public List<MazeCell> FindDeadEnds() {
        return _cells.FindAll(cell => cell.IsDeadEnd);
    }

    public MazeCell this[int row, int col] {
        get {
            var index = row * _dimensions.Columns + col;
            return _cells[index];
        }
        set {
            var index = row * _dimensions.Columns + col;
            _cells[index] = value;
        }
    }

    public int Rows { get => _dimensions.Rows; }

    public int Cols { get => _dimensions.Columns; }

    public int Size { get => _dimensions.Product; }

    public MazeGrid(Dimensions mazeSize) {
        _dimensions = mazeSize;
        _cells = new List<MazeCell>(_dimensions.Product);
        // TODO: 1. Figure out the placement
        //      pack areas together to mimic the proportions of the map
        //      spread areas on the map
        // TODO: 2. Place no cell for Fill, and the same cell repeatedly for Hall
        // TODO: 3. Hall cells can have many neighbors on any side.
        // ? P'haps the direction is a property of the gate, not it's identity.
        for (int i = 0; i < _cells.Capacity; i++) {
            _cells.Add(new MazeCell(i / _dimensions.Columns, i % _dimensions.Columns));
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