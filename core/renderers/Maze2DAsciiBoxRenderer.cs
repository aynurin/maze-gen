using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nour.Play.Maze;
using Nour.Play.Maze.PostProcessing;

namespace Nour.Play.Renderers {
    public class Maze2DAsciiBoxRenderer {

        private readonly Maze2D _maze;
        private readonly int _cellInnerHeight;
        private readonly int _cellInnerWidth;
        private readonly int _asciiMazeHeight;
        private readonly int _asciiMazeWidth;
        private readonly Border.Type[] _buffer;
        private readonly Dictionary<int, string> _data =
            new Dictionary<int, string>();

        public Maze2DAsciiBoxRenderer(Maze2D maze,
                                      int cellInnerHeight = 1,
                                      int cellInnerWidth = 3) {
            _maze = maze; // 4x4
            _cellInnerWidth = cellInnerWidth;
            _cellInnerHeight = cellInnerHeight;
            _asciiMazeWidth = _maze.XWidthColumns * (_cellInnerWidth + 1) + 1; // 17
            _asciiMazeHeight = _maze.YHeightRows * (_cellInnerHeight + 1) + 1; // 9
            _buffer = new Border.Type[_asciiMazeWidth * _asciiMazeHeight]; // 17x9
        }

        public string WithTrail() {
            var trail = _maze.LongestPath.HasValue ?
                _maze.LongestPath.Value :
                new List<MazeCell>();
            var solutionCells = new HashSet<MazeCell>(trail);
            // print cells from top to bottom
            for (var y = _maze.YHeightRows - 1; y >= 0; y--) {
                for (var x = 0; x < _maze.XWidthColumns; x++) {
                    var mazeIndex = new Vector(x, y).ToIndex(_maze.XWidthColumns);
                    var mazeCell = _maze.AllCells[mazeIndex];
                    var cellData = solutionCells.Contains(mazeCell) ?
                        Convert.ToString(trail.IndexOf(mazeCell), 16) :
                        string.Empty;
                    PrintCell(mazeCell, cellData);
                }
            }

            // render cells in a string buffer
            var strBuffer = new StringBuilder();
            for (var y = 0; y < _asciiMazeHeight; y++) {
                for (var x = 0; x < _asciiMazeWidth; x++) {
                    var index = new Vector(x, y).ToIndex(_asciiMazeWidth);
                    if (_data.ContainsKey(index)) {
                        strBuffer.Append(_data[index]);
                        x += _data[index].Length - 1;
                    } else {
                        strBuffer.Append(Border.Char(_buffer[index]));
                    }
                }
                strBuffer.Append(Environment.NewLine);
            }
            return strBuffer.ToString();
        }

        private void PrintCell(MazeCell cell, string cellData) {
            var asciiCoords = GetCellCoords(cell);
            if (!string.IsNullOrEmpty(cellData)) {
                _data.Add(I(asciiCoords.Center), cellData);
            }
            if (!cell.IsVisited) return;
            _buffer[I(asciiCoords.Northeast)] |= Border.Type.X;
            _buffer[I(asciiCoords.Northwest)] |= Border.Type.X;
            _buffer[I(asciiCoords.Southeast)] |= Border.Type.X;
            _buffer[I(asciiCoords.Southwest)] |= Border.Type.X;
            if (!cell.Links(Vector.North2D).HasValue) {
                _buffer[I(asciiCoords.Northeast)] |= Border.Type.Left;
                _buffer[I(asciiCoords.Northwest)] |= Border.Type.Right;
                foreach (var x in asciiCoords.North) _buffer[I(x)] |= Border.Type.South;
            }
            if (!cell.Links(Vector.South2D).HasValue) {
                _buffer[I(asciiCoords.Southeast)] |= Border.Type.Left;
                _buffer[I(asciiCoords.Southwest)] |= Border.Type.Right;
                foreach (var x in asciiCoords.South) _buffer[I(x)] |= Border.Type.North;
            }
            if (!cell.Links(Vector.East2D).HasValue) {
                _buffer[I(asciiCoords.Southeast)] |= Border.Type.Top;
                _buffer[I(asciiCoords.Northeast)] |= Border.Type.Bottom;
                foreach (var x in asciiCoords.East) _buffer[I(x)] |= Border.Type.East;
            }
            if (!cell.Links(Vector.West2D).HasValue) {
                _buffer[I(asciiCoords.Southwest)] |= Border.Type.Top;
                _buffer[I(asciiCoords.Northwest)] |= Border.Type.Bottom;
                foreach (var x in asciiCoords.West) _buffer[I(x)] |= Border.Type.West;
            }
        }

        private CellCoords GetCellCoords(MazeCell cell) {
            // we have to conversions here:
            // 1. maze cell X,Y is rendered at X,(_maze.Size.Y - cell.Y - 1)
            // 2. ascii coord of maze cell X,Y is rendered at X,(_asciiMazeHeight - Y - 1)
            var scaledCell = new {
                x = cell.X * (_cellInnerWidth + 1),
                y = (_maze.Size.Y - cell.Y) * (_cellInnerHeight + 1)
            };
            // I will print cell 0x3
            // it's ascii coordinates will be:
            // NW: 0x0 which converts to index 0
            // SW: 0x3 which converts to index 0 + 17*2 = 34
            // NE: 4x0 which converts to index 4
            // SE: 4x3 which converts to index 4 + 17*2 = 38

            // maze cell 0x0 = ascii coords 0x(asciiHeight-0)
            // Northwest = 0 + (_maze.XWidthColumns * (_cellInnerWidth + 1) + 1) * (_cellInnerHeight + 1) = 
            Vector CellCoord(int dY, int dX) =>
                new Vector(scaledCell.x + dX, scaledCell.y - dY);
            var c = new CellCoords {
                Northwest = CellCoord(_cellInnerHeight + 1, 0),
                Southwest = CellCoord(0, 0),
                Northeast = CellCoord(_cellInnerHeight + 1, _cellInnerWidth + 1),
                Southeast = CellCoord(0, _cellInnerWidth + 1),
                Center = CellCoord(1, 2),
                West = Enumerable.Range(0, _cellInnerHeight).Select(i => CellCoord(i + 1, 0)).ToArray(),
                East = Enumerable.Range(0, _cellInnerHeight).Select(i => CellCoord(i + 1, _cellInnerWidth + 1)).ToArray(),
                North = Enumerable.Range(0, _cellInnerWidth).Select(i => CellCoord(_cellInnerHeight + 1, i + 1)).ToArray(),
                South = Enumerable.Range(0, _cellInnerWidth).Select(i => CellCoord(0, i + 1)).ToArray(),
            };
            return c;
        }

        private int I(Vector v) => v.ToIndex(_asciiMazeWidth);

        struct CellCoords {
            public Vector Northwest;
            public Vector Southwest;
            public Vector Northeast;
            public Vector Southeast;
            public Vector Center;

            public Vector[] West;
            public Vector[] East;
            public Vector[] North;
            public Vector[] South;
        }

        private static class Border {
            [Flags]
            public enum Type {
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
                None = 0b000000000000000,
            }

            private static readonly Dictionary<Type, char> s_chars =
                new Dictionary<Type, char>() {
                    {Type.North, '─'},
                    {Type.South, '─'},
                    {Type.North | Type.South, '─'},
                    {Type.X | Type.Left | Type.Right, '─'},
                    {Type.West, '│'},
                    {Type.East, '│'},
                    {Type.West | Type.East, '│'},
                    {Type.X | Type.Top | Type.Bottom, '│'},

                    {Type.X | Type.Left, '╴'},
                    {Type.X | Type.Right, '╶'},
                    {Type.X | Type.Top, '╵'},
                    {Type.X | Type.Bottom, '╷'},

                    {Type.X | Type.Right | Type.Bottom, '┌'},
                    {Type.X | Type.Left | Type.Bottom, '┐'},
                    {Type.X | Type.Right | Type.Top, '└'},
                    {Type.X | Type.Left | Type.Top, '┘'},

                    {Type.X | Type.Left | Type.Right | Type.Top, '┴'},
                    {Type.X | Type.Left | Type.Right | Type.Bottom, '┬'},
                    {Type.X | Type.Right | Type.Top | Type.Bottom, '├'},
                    {Type.X | Type.Left | Type.Top | Type.Bottom, '┤'},

                    {Type.X, '┼'},
                    {Type.X | Type.Left | Type.Right | Type.Top | Type.Bottom, '┼'},
                    {Type.Mark, '*'},
                    {Type.None, ' '},
                };

            public static char Char(Type t) => s_chars[t];
        }
    }
}