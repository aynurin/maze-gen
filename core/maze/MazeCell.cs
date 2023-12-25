using System;
using System.Collections.Generic;

namespace Nour.Play.Maze {
    public class MazeCell {
        private readonly List<MazeCell> _links = new List<MazeCell>();
        private readonly List<MazeCell> _neighbors = new List<MazeCell>();
        public Dictionary<string, string> Attributes { get; }
            = new Dictionary<string, string>();

        public Vector Coordinates { get; private set; }

        public bool IsVisited { get; private set; }

        public int X => Coordinates.X;
        public int Y => Coordinates.Y;

        public MazeCell(int x, int y) : this(new Vector(x, y)) { }

        public MazeCell(Vector coordinates) {
            Coordinates = coordinates;
        }

        public void Link(MazeCell cell) {
            if (_links.Contains(cell))
                throw new InvalidOperationException("This link already exists");
            IsVisited = true;
            cell.IsVisited = true;
            _links.Add(cell);
            cell._links.Add(this);
        }

        public void Unlink(MazeCell cell) {
            _links.Remove(cell);
            cell._links.Remove(this);
        }

        // TODO (MapArea): if the cell belongs to an area, use Area neighbors
        // TODO (MapArea): Choose only visitable areas.
        public List<MazeCell> Neighbors() => _neighbors;

        public Optional<MazeCell> Neighbors(Vector unitVector) =>
            new Optional<MazeCell>(_neighbors.Find(cell => cell.Coordinates == this.Coordinates + unitVector));

        // TODO (MapArea): if the cell belongs to an area, use Area links
        // TODO (MapArea): Choose only visitable areas.
        public List<MazeCell> Links() => _links;

        public Optional<MazeCell> Links(Vector unitVector) =>
            new Optional<MazeCell>(_links.Find(cell => cell.Coordinates == this.Coordinates + unitVector));

        public string GatesString() => string.Concat(
            Links(Vector.North2D).HasValue ? "N" : "-",
            Links(Vector.East2D).HasValue ? "E" : "-",
            Links(Vector.South2D).HasValue ? "S" : "-",
            Links(Vector.West2D).HasValue ? "W" : "-");

        public string ToLongString() => $"{ToString()}({GatesString()})";

        public override string ToString() => $"{Coordinates}{(IsVisited ? "V" : "")}";
    }
}