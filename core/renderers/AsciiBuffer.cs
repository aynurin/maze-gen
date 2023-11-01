using System;
using System.Text;

namespace Nour.Play {
    public class AsciiBuffer {
        private int _rows;
        private int _cols;
        private bool _hideOverflow;
        private char[][] _buffer;

        public AsciiBuffer(int rows, int cols, bool hideOverflow) {
            _rows = rows;
            _cols = cols;
            _hideOverflow = hideOverflow;
            _buffer = new char[rows][];
            for (int i = 0; i < rows; i++) {
                _buffer[i] = new String(' ', cols).ToCharArray();
            }
        }

        internal void PutC(int row, int col, char v) {
            if (row < 0 || row >= _buffer.Length || col < 0 || col >= _buffer[row].Length) {
                if (_hideOverflow) {
                    return;
                } else {
                    throw new InvalidOperationException($"Position {row}x{col} is off the grid {_buffer.Length}x{_buffer[row].Length}");
                }
            }
            _buffer[row][col] = v;
        }

        public override string ToString() {
            var buffer = new StringBuilder();
            foreach (var line in _buffer) {
                buffer.AppendLine(new String(line));
            }
            return buffer.ToString();
        }
    }
}