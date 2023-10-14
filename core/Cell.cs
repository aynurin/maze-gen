using System.Collections.Generic;

namespace Nour.Play {
    public class Cell {
        private readonly List<Cell> _links = new List<Cell>();
        private readonly List<Cell> _neighbors = new List<Cell>();

        public List<Cell> Neighbors { get => _neighbors; }

        public Vector Coordinates { get; private set; }

        public int X => Coordinates.X;
        public int Y => Coordinates.Y;
        public int Z => Coordinates.Z;

        public List<Cell> Links {
            get => _links;
        }

        public Cell(int x, int y) {
            Coordinates = new Vector(x, y);
        }

        public void Link(Cell cell) {
            _links.Add(cell);
            cell._links.Add(this);
        }

        public void Unlink(Cell cell) {
            _links.Remove(cell);
            cell._links.Remove(this);
        }

        public Optional<Cell> NorthNeighbor {
            get => GetNeighbor(-1, 0);
        }

        public Optional<Cell> EastNeighbor {
            get => GetNeighbor(0, 1);
        }

        public Optional<Cell> SouthNeighbor {
            get => GetNeighbor(1, 0);
        }

        public Optional<Cell> WestNeighbor {
            get => GetNeighbor(0, -1);
        }

        public Optional<Cell> NorthGate {
            get => GetGate(-1, 0);
        }

        public Optional<Cell> EastGate {
            get => GetGate(0, 1);
        }

        public Optional<Cell> SouthGate {
            get => GetGate(1, 0);
        }

        public Optional<Cell> WestGate {
            get => GetGate(0, -1);
        }

        public bool IsDeadEnd => _links.Count == 1;

        public Optional<Cell> GetNeighbor(int dX, int dY) {
            return new Optional<Cell>(_neighbors.Find(cell => cell.Coordinates == this.Coordinates + new Vector(dX, dY)));
        }

        public Optional<Cell> GetGate(int dX, int dY) {
            return new Optional<Cell>(_links.Find(cell => cell.Coordinates == this.Coordinates + new Vector(dX, dY)));
        }

        public string ToLongString() => $"{Coordinates}: {(NorthGate.HasValue ? "N" : "-")}{(EastGate.HasValue ? "E" : "-")}{(SouthGate.HasValue ? "S" : "-")}{(WestGate.HasValue ? "W" : "-")}";

        public override string ToString() => Coordinates.ToString();
    }
}