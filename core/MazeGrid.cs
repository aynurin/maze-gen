using System.Text;

public class MazeGrid {
    private readonly MazeCell[] _cells;
    private readonly int[] _dimentions;

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
        _cells = new MazeCell[rows * cols];
        for (int i = 0; i < rows * cols; i++) {
            _cells[i] = new MazeCell(i / rows, i % rows);
        }
    }
}