using System;
using System.Collections.Generic;

public class MazeCell {
    private readonly List<MazeCell> _links = new List<MazeCell>();
    private readonly List<MazeCell> _neighbors = new List<MazeCell>();

    public List<MazeCell> Neighbors { get => _neighbors; }

    public int Row { get; private set; }
    public int Col { get; private set; }
    public List<MazeCell> Links {
        get => _links;
    }

    public MazeCell(int row, int col) {
        Row = row;
        Col = col;
    }

    public void Link(MazeCell cell) {
        _links.Add(cell);
        cell._links.Add(this);
    }

    public void Unlink(MazeCell cell) {
        _links.Remove(cell);
        cell._links.Remove(this);
    }

    public Optional<MazeCell> NorthNeighbor {
        get => GetNeighbor(0, -1);
    }

    public Optional<MazeCell> EastNeighbor {
        get => GetNeighbor(1, 0);
    }

    public Optional<MazeCell> SouthNeighbor {
        get => GetNeighbor(0, 1);
    }

    public Optional<MazeCell> WestNeighbor {
        get => GetNeighbor(-1, 0);
    }

    public Optional<MazeCell> NorthGate {
        get => GetGate(0, -1);
    }

    public Optional<MazeCell> EastGate {
        get => GetGate(1, 0);
    }

    public Optional<MazeCell> SouthGate {
        get => GetGate(0, 1);
    }

    public Optional<MazeCell> WestGate {
        get => GetGate(-1, 0);
    }

    public bool IsDeadEnd => _links.Count == 1;

    public Optional<MazeCell> GetNeighbor(int dx, int dy) {
        return new Optional<MazeCell>(_neighbors.Find(cell => cell.Row == this.Row + dy && cell.Col == this.Col + dx));
    }

    public Optional<MazeCell> GetGate(int dx, int dy) {
        return new Optional<MazeCell>(_links.Find(cell => cell.Row == this.Row + dy && cell.Col == this.Col + dx));
    }

    public string ToLongString() => $"{Row}x{Col}: {(NorthGate.HasValue ? "N" : "-")}{(EastGate.HasValue ? "E" : "-")}{(SouthGate.HasValue ? "S" : "-")}{(WestGate.HasValue ? "W" : "-")}";

    public override string ToString() => $"{Row}x{Col}";
}