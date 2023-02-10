using System.Text;

public class MazeDefinition {
    private readonly MazeCell[,] maze;

    private const int ROWS = 0;
    private const int COLUMNS = 0;

    public MazeDefinition(int x, int y) {
        this.maze = new MazeCell[x,y];
    }

    public void Randomize() {
        var cellStates = new byte[maze.GetUpperBound(ROWS) * maze.GetUpperBound(COLUMNS)];
        new Random().NextBytes(cellStates);
        for (int row = maze.GetLowerBound(ROWS); row < maze.GetUpperBound(ROWS); row++) {
            for (int col = maze.GetLowerBound(COLUMNS); col < maze.GetUpperBound(COLUMNS); col++) {
                var cellState = cellStates[row * maze.GetUpperBound(ROWS) + col];
                var cell = new MazeCell(
                    cellState % 2 == 0 && row > 0,
                    cellState % 2 == 1 && col < maze.GetUpperBound(COLUMNS)-1,
                    false,
                    false
                );
                maze.SetValue(cell, row, col);
            }
        }
    }

    public override string ToString()
    {
        var buffer = new StringBuilder();
        for (int row = maze.GetLowerBound(ROWS); row < maze.GetUpperBound(ROWS); row++) {
            buffer.Append("|");
            for (int col = maze.GetLowerBound(COLUMNS); col < maze.GetUpperBound(COLUMNS); col++) {
                var cell = maze.GetValue(row, col) as MazeCell;
                if (cell!.N) {
                    buffer.Append(" ");
                }
                else {
                    buffer.Append("‾");
                }
                if (cell!.E) {
                    buffer.Append(" ");
                }
                else {
                    buffer.Append("|");
                }
            }
            buffer.Append(Environment.NewLine);
        }
        for (int col = maze.GetLowerBound(COLUMNS); col < maze.GetUpperBound(COLUMNS); col++) {
            buffer.Append("‾‾");
        }
        buffer.Append("‾");
        return buffer.ToString();
    }

    class MazeCell {
        public readonly bool N;
        public readonly bool E;
        public readonly bool S;
        public readonly bool W;

        public MazeCell(bool n, bool e, bool s, bool w) {
            N = n;
            E = e;
            S = s;
            W = w;
        }
    }
}