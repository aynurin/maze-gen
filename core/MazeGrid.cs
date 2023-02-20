using System.Collections.Generic;
using System.Linq;

public class MazeGrid {
    private readonly List<MazeCell> _cells;
    private readonly int[] _dimentions;

    public List<MazeCell> Cells { get => _cells; }

    public MazeCell this[int row, int col] {
        get {
            var index = row * _dimentions[1] + col;
            return _cells[index];
        }
        set {
            var index = row * _dimentions[1] + col;
            _cells[index] = value;
        }
    }

    public int Rows { get => _dimentions[0]; }

    public int Cols { get => _dimentions[1]; }

    public int Size { get => _dimentions.Aggregate((a, b) => a * b); }

    public MazeGrid(int rows, int cols) {
        _dimentions = new int[] { rows, cols };
        _cells = new List<MazeCell>(rows * cols);
        for (int i = 0; i < rows * cols; i++) {
            _cells.Add(new MazeCell(i / rows, i % rows));
        }
        for (int i = 0; i < rows * cols; i++) {
            if (_cells[i].Row > 0) {
                _cells[i].Neighbours.Add(this[_cells[i].Row - 1, _cells[i].Col]);
            }
            if (_cells[i].Row < this.Rows - 1) {
                _cells[i].Neighbours.Add(this[_cells[i].Row + 1, _cells[i].Col]);
            }
            if (_cells[i].Col > 0) {
                _cells[i].Neighbours.Add(this[_cells[i].Row, _cells[i].Col - 1]);
            }
            if (_cells[i].Col < this.Cols - 1) {
                _cells[i].Neighbours.Add(this[_cells[i].Row, _cells[i].Col + 1]);
            }
        }
    }
}