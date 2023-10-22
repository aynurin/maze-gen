using System;
using System.Collections.Generic;

namespace Nour.Play {
    public class Cell {
        private readonly List<Cell> _links = new List<Cell>();
        private readonly List<Cell> _neighbors = new List<Cell>();

        public Vector Coordinates { get; private set; }

        public int X => Coordinates.X;
        public int Y => Coordinates.Y;
        public int Z => Coordinates.Z;

        public Cell(int x, int y) {
            Coordinates = new Vector(x, y);
        }

        public void Link(Cell cell) {
            if (_links.Contains(cell))
                throw new InvalidOperationException("This link already exists");
            _links.Add(cell);
            cell._links.Add(this);
        }

        public void Unlink(Cell cell) {
            _links.Remove(cell);
            cell._links.Remove(this);
        }

        public List<Cell> Neighbors() => _neighbors;

        public Optional<Cell> Neighbors(Vector unitVector) =>
            new Optional<Cell>(_neighbors.Find(cell => cell.Coordinates == this.Coordinates + unitVector));

        public List<Cell> Links() => _links;

        public Optional<Cell> Links(Vector unitVector) =>
            new Optional<Cell>(_links.Find(cell => cell.Coordinates == this.Coordinates + unitVector));

        public string ToLongString() => $"{Coordinates}: {(Links(Vector.West2D).HasValue ? "N" : "-")}{(Links(Vector.North2D).HasValue ? "E" : "-")}{(Links(Vector.East2D).HasValue ? "S" : "-")}{(Links(Vector.South2D).HasValue ? "W" : "-")}";

        public override string ToString() => Coordinates.ToString();
    }
}