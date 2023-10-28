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

        public MazeCell(int x, int y) {
            Coordinates = new Vector(x, y);
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

        public List<MazeCell> Neighbors() => _neighbors;

        public Optional<MazeCell> Neighbors(Vector unitVector) =>
            new Optional<MazeCell>(_neighbors.Find(cell => cell.Coordinates == this.Coordinates + unitVector));

        public List<MazeCell> Links() => _links;

        public Optional<MazeCell> Links(Vector unitVector) =>
            new Optional<MazeCell>(_links.Find(cell => cell.Coordinates == this.Coordinates + unitVector));

        public string ToLongString() => $"{Coordinates}: {(Links(Vector.West2D).HasValue ? "N" : "-")}{(Links(Vector.North2D).HasValue ? "E" : "-")}{(Links(Vector.East2D).HasValue ? "S" : "-")}{(Links(Vector.South2D).HasValue ? "W" : "-")}";

        public override string ToString() => Coordinates.ToString();
    }
}