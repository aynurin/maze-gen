using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Maze {
    public class MazeCell {
        private readonly List<MazeCell> _links = new List<MazeCell>();
        private readonly List<MazeCell> _neighbors = new List<MazeCell>();
        private ReadOnlyCollection<MazeCell> _mapAreaCells;
        private MapArea _mapArea;

        public Dictionary<string, string> Attributes { get; }
            = new Dictionary<string, string>();

        public Vector Coordinates { get; private set; }

        public bool IsVisited { get; private set; }

        public int X => Coordinates.X;
        public int Y => Coordinates.Y;

        public MapArea MapArea => _mapArea;

        // If the map area is not visitable, then we can't visit this cell, thus
        // it can't have neighbors and links.
        // This method will not propagate the MapArea to the neighbors.
        public void AssignMapArea(MapArea area, IList<MazeCell> mapAreaCells) {
            if (_mapArea != null) {
                throw new InvalidOperationException(
                    $"Map area already assigned to this cell {this}");
            }
            _mapArea = area;
            _mapAreaCells = new ReadOnlyCollection<MazeCell>(mapAreaCells);
            if (_mapArea.Type == AreaType.Fill) {
                foreach (var neighbor in _neighbors) {
                    neighbor._neighbors.Remove(this);
                    neighbor._links.Remove(this);
                    if (neighbor._links.Count == 0) {
                        neighbor.IsVisited = false;
                    }
                }
                Log.WriteImmediate(_mapArea.DebugString());
                _neighbors.Clear();
                _links.Clear();
                this.IsVisited = false;
            } else if (_mapArea.Type == AreaType.Hall) {
                // We don't add back references because they will be added by
                // upstream.
                _links.AddRange(_neighbors.Where(n => _mapAreaCells.Contains(n)));
                IsVisited = true;
            }
        }


        public MazeCell(int x, int y) : this(new Vector(x, y)) { }

        public MazeCell(Vector coordinates) {
            Coordinates = coordinates;
        }

        public void Link(MazeCell cell) {
            // skip the cells in the same map area as they are already linked.
            if (_mapAreaCells?.Contains(cell) == true) return;
            // check if the cell is a neighbor.
            if (!_neighbors.Contains(cell))
                throw new NotImplementedException(
                    "Linking with non-adjacent cells is not supported yet");
            // fool-proof to avoid double linking.
            if (_links.Contains(cell))
                throw new InvalidOperationException($"This link already exists ({this}->{cell})");

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