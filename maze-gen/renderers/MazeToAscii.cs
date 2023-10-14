using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nour.Play.Maze.Solvers;

namespace Nour.Play.Maze {
    public class MazeToAscii {

        private readonly Map2D _maze;
        private readonly int _cellInnerHeight;
        private readonly int _cellInnerWidth;
        private readonly int _asciiMazeHeight;
        private readonly int _asciiMazeWidth;
        private readonly Border.Type[] _buffer;
        private readonly Dictionary<int, string> _data = new System.Collections.Generic.Dictionary<int, string>();

        public MazeToAscii(Map2D maze, int cellInnerHeight = 1, int cellInnerWidth = 3) {
            _maze = maze;
            _cellInnerHeight = cellInnerHeight;
            _cellInnerWidth = cellInnerWidth;
            _asciiMazeHeight = _maze.XHeightRows * (_cellInnerHeight + 1) + 1;
            _asciiMazeWidth = _maze.YWidthColumns * (_cellInnerWidth + 1) + 1;
            _buffer = new Border.Type[_asciiMazeHeight * _asciiMazeWidth];
            for (int x = 0; x < _asciiMazeHeight; x++) {
                for (int y = 0; y < _asciiMazeWidth; y++) {
                    var borderType = Border.Type.Inner;
                    if (x == 0 || y == 0 || x == _asciiMazeHeight - 1 || y == _asciiMazeWidth - 1) {
                        borderType |= Border.Type.Outer;
                    }
                    if (y % (_cellInnerWidth + 1) == 0 && x % (_cellInnerHeight + 1) == 0) {
                        borderType |= Border.Type.X;
                    }
                    _buffer[x * _asciiMazeWidth + y] = borderType;
                }
            }
        }
        public string Convert(DijkstraDistance distances) {
            var solutionCells = distances.Solution.HasValue ? new HashSet<Cell>(distances.Solution.Value) : new HashSet<Cell>();
            for (int i = 0; i < _maze.XHeightRows; i++) {
                for (int j = 0; j < _maze.YWidthColumns; j++) {
                    try {
                        var cell = _maze[i, j];
                        var cellData = solutionCells.Contains(cell) ? System.Convert.ToString(distances[cell], 16) : String.Empty;
                        PrintCell(cell, cellData);
                    } catch (System.ArgumentOutOfRangeException) {
                        Console.Error.WriteLine($"Index {i}x{j} was out of range {_maze.Cells.Count}");
                        throw;
                    }
                }
            }

            var strBuffer = new StringBuilder();
            for (int x = 0; x < _asciiMazeHeight; x++) {
                for (int y = 0; y < _asciiMazeWidth; y++) {
                    var index = x * _asciiMazeWidth + y;
                    if (_data.ContainsKey(index)) {
                        strBuffer.Append(_data[index]);
                        y += _data[index].Length - 1;
                    } else {
                        strBuffer.Append(Border.Char(_buffer[index]));
                    }
                }
                strBuffer.Append(Environment.NewLine);
            }
            return strBuffer.ToString();
        }

        private static class Border {
            [Flags]
            public enum Type {
                Outer = 0b000000000000001,
                Inner = 0b000000000000000,
                North = 0b000000000000100,
                East = 0b000000000001000,
                West = 0b000000000010000,
                South = 0b000000000100000,
                Exit1 = 0b000000010000000,
                Exit2 = 0b000000100000000,
                X = 0b000001000000000,
                Left = 0b000010000000000,
                Right = 0b000100000000000,
                Top = 0b001000000000000,
                Bottom = 0b010000000000000,
                Mark = 0b100000000000000,
            }

            private static readonly Dictionary<Type, char> _chars = new Dictionary<Type, char>() {
            {Type.Outer | Type.North, '═'},
            {Type.Outer | Type.South, '═'},
            {Type.Outer | Type.X | Type.Left | Type.Right, '═'},
            {Type.Outer | Type.West, '║'},
            {Type.Outer | Type.East, '║'},
            {Type.Outer | Type.X | Type.Top | Type.Bottom, '║'},

            {Type.Inner | Type.North, '─'},
            {Type.Inner | Type.South, '─'},
            {Type.Inner | Type.North | Type.South, '─'},
            {Type.Inner | Type.X | Type.Left | Type.Right, '─'},
            {Type.Inner | Type.West, '│'},
            {Type.Inner | Type.East, '│'},
            {Type.Inner | Type.West | Type.East, '│'},
            {Type.Inner | Type.X | Type.Top | Type.Bottom, '│'},

            {Type.Inner | Type.X | Type.Left, '╴'},
            {Type.Inner | Type.X | Type.Right, '╶'},
            {Type.Inner | Type.X | Type.Top, '╵'},
            {Type.Inner | Type.X | Type.Bottom, '╷'},

            {Type.Outer | Type.X | Type.Right | Type.Bottom, '╔'},
            {Type.Outer | Type.X | Type.Left | Type.Bottom, '╗'},
            {Type.Outer | Type.X | Type.Right | Type.Top, '╚'},
            {Type.Outer | Type.X | Type.Left | Type.Top, '╝'},

            {Type.Inner | Type.X | Type.Right | Type.Bottom, '┌'},
            {Type.Inner | Type.X | Type.Left | Type.Bottom, '┐'},
            {Type.Inner | Type.X | Type.Right | Type.Top, '└'},
            {Type.Inner | Type.X | Type.Left | Type.Top, '┘'},

            {Type.Outer | Type.X | Type.Left | Type.Right | Type.Top, '╧'},
            {Type.Outer | Type.X | Type.Left | Type.Right | Type.Bottom, '╤'},
            {Type.Outer | Type.X | Type.Right | Type.Top | Type.Bottom, '╟'},
            {Type.Outer | Type.X | Type.Left | Type.Top | Type.Bottom, '╢'},

            {Type.Inner | Type.X | Type.Left | Type.Right | Type.Top, '┴'},
            {Type.Inner | Type.X | Type.Left | Type.Right | Type.Bottom, '┬'},
            {Type.Inner | Type.X | Type.Right | Type.Top | Type.Bottom, '├'},
            {Type.Inner | Type.X | Type.Left | Type.Top | Type.Bottom, '┤'},

            {Type.Outer | Type.X | Type.North | Type.Exit1, '╕'},
            {Type.Outer | Type.X | Type.North | Type.Exit2, '╒'},
            {Type.Outer | Type.X | Type.South | Type.Exit1, '╛'},
            {Type.Outer | Type.X | Type.South | Type.Exit2, '╘'},

            {Type.Outer | Type.X | Type.East | Type.Exit1, '╜'},
            {Type.Outer | Type.X | Type.East | Type.Exit2, '╖'},
            {Type.Outer | Type.X | Type.West | Type.Exit1, '╙'},
            {Type.Outer | Type.X | Type.West | Type.Exit2, '╓'},

            {Type.Inner | Type.X, '┼'},
            {Type.Inner | Type.X | Type.Left | Type.Right | Type.Top | Type.Bottom, '┼'},
            {Type.Inner, ' '},
            {Type.Outer, ' '},
            {Type.Mark, '*'},
        };

            public static char Char(Type t) => _chars[t];
        }

        private void PrintCell(Cell cell, string cellData) {
            Console.WriteLine($"Cell {cell.X,2}x{cell.Y,2}: {(!cell.NorthGate.HasValue ? "-" : "N")}, {(!cell.EastGate.HasValue ? "-" : "E")}, {(!cell.SouthGate.HasValue ? "-" : "S")}, {(!cell.WestGate.HasValue ? "-" : "W")}");
            var asciiCoords = GetCellCoords(cell);
            if (!cell.NorthGate.HasValue) {
                _buffer[asciiCoords.Northeast] |= Border.Type.Left;
                _buffer[asciiCoords.Northwest] |= Border.Type.Right;
                foreach (var x in asciiCoords.North) _buffer[x] |= Border.Type.North;
            }
            if (!cell.SouthGate.HasValue) {
                _buffer[asciiCoords.Southeast] |= Border.Type.Left;
                _buffer[asciiCoords.Southwest] |= Border.Type.Right;
                foreach (var x in asciiCoords.South) _buffer[x] |= Border.Type.South;
            }
            if (!cell.EastGate.HasValue) {
                _buffer[asciiCoords.Southeast] |= Border.Type.Top;
                _buffer[asciiCoords.Northeast] |= Border.Type.Bottom;
                foreach (var x in asciiCoords.East) _buffer[x] |= Border.Type.East;
            }
            if (!cell.WestGate.HasValue) {
                _buffer[asciiCoords.Southwest] |= Border.Type.Top;
                _buffer[asciiCoords.Northwest] |= Border.Type.Bottom;
                foreach (var x in asciiCoords.West) _buffer[x] |= Border.Type.West;
            }
            if (!String.IsNullOrEmpty(cellData)) {
                _data.Add(asciiCoords.Center, cellData);
            }
        }

        private CellCoords GetCellCoords(Cell cell) {
            int scaledRow = cell.X * (_cellInnerHeight + 1); // each cell height = inner height + border
            int scaledCol = cell.Y * (_cellInnerWidth + 1); // each cell width = inner width + border
            var scaledGrid = new { rows = _maze.XHeightRows * (_cellInnerHeight + 1) + 1, cols = _maze.YWidthColumns * (_cellInnerWidth + 1) + 1 };
            var scaledCell = new { x = cell.X * (_cellInnerHeight + 1), y = cell.Y * (_cellInnerWidth + 1) };
            Func<int, int, int> cellCoord = (int dRow, int dCol) => (scaledCell.x + dRow) * scaledGrid.cols + (scaledCell.y + dCol);
            return new CellCoords {
                Northwest = cellCoord(0, 0),
                Southwest = cellCoord(_cellInnerHeight + 1, 0),
                Northeast = cellCoord(0, _cellInnerWidth + 1),
                Southeast = cellCoord(_cellInnerHeight + 1, _cellInnerWidth + 1),
                Center = cellCoord(1, 2),
                West = Enumerable.Range(0, _cellInnerHeight).Select(i => cellCoord(i + 1, 0)).ToArray(),
                East = Enumerable.Range(0, _cellInnerHeight).Select(i => cellCoord(i + 1, _cellInnerWidth + 1)).ToArray(),
                North = Enumerable.Range(0, _cellInnerWidth).Select(i => cellCoord(0, i + 1)).ToArray(),
                South = Enumerable.Range(0, _cellInnerWidth).Select(i => cellCoord(_cellInnerHeight + 1, i + 1)).ToArray(),
            };
        }

        struct CellCoords {
            public int Northwest;
            public int Southwest;
            public int Northeast;
            public int Southeast;
            public int Center;

            public int[] West;
            public int[] East;
            public int[] North;
            public int[] South;
        }
    }
}