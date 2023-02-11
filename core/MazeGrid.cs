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

    public MazeGrid(int x, int y) {
        _dimentions = new int[] { x, y };
        _cells = new MazeCell[x * y];
        for (int i = 0; i < x * y; i++) _cells[i] = new MazeCell();
    }

    public void Randomize(MazeGenerator generator) {
        generator.Generate(this);
    }

    public override string ToString() {
        var buffer = new StringBuilder();
        for (int row = 0; row < Rows; row++) {
            buffer.Append("|");
            for (int col = 0; col < Cols; col++) {
                var cell = this[row, col];
                if (cell.NorthGate != null) {
                    buffer.Append(" ");
                } else {
                    buffer.Append("‾");
                }
                if (cell.EastGate != null) {
                    buffer.Append("‾");
                } else {
                    buffer.Append("|");
                }
            }
            buffer.Append(Environment.NewLine);
        }
        for (int col = 0; col < Cols; col++) {
            buffer.Append("‾‾");
        }
        buffer.Append("‾");
        return buffer.ToString();
    }
}