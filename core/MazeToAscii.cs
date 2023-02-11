using System.Text;

public class MazeToAscii {
    public string Convert(MazeGrid maze) {
        var buffer = new StringBuilder();
        for (int row = 0; row < maze.Rows; row++) {
            buffer.Append("|");
            for (int col = 0; col < maze.Cols; col++) {
                var cell = maze[row, col];
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
        for (int col = 0; col < maze.Cols; col++) {
            buffer.Append("‾‾");
        }
        buffer.Append("‾");
        return buffer.ToString();
    }
}