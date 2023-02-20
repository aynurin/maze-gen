using System;
using System.Collections.Generic;

public class MazeCell {
    private readonly List<MazeCell> _links = new List<MazeCell>();
    private readonly List<MazeCell> _neighbours = new List<MazeCell>();

    public List<MazeCell> Neighbours { get => _neighbours; }

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

    public Optional<MazeCell> NorthNeighbour {
        get => new Optional<MazeCell>(_neighbours.Find(cell => cell.Row == this.Row - 1 && cell.Col == this.Col));
    }

    public Optional<MazeCell> EastNeighbour {
        get => new Optional<MazeCell>(_neighbours.Find(cell => cell.Row == this.Row && cell.Col == this.Col + 1));
    }

    public Optional<MazeCell> SouthNeighbour {
        get => new Optional<MazeCell>(_neighbours.Find(cell => cell.Row == this.Row + 1 && cell.Col == this.Col));
    }

    public Optional<MazeCell> WestNeighbour {
        get => new Optional<MazeCell>(_neighbours.Find(cell => cell.Row == this.Row && cell.Col == this.Col - 1));
    }

    public Optional<MazeCell> NorthGate {
        get => new Optional<MazeCell>(_links.Find(cell => cell.Row == this.Row - 1 && cell.Col == this.Col));
    }

    public Optional<MazeCell> EastGate {
        get => new Optional<MazeCell>(_links.Find(cell => cell.Row == this.Row && cell.Col == this.Col + 1));
    }

    public Optional<MazeCell> SouthGate {
        get => new Optional<MazeCell>(_links.Find(cell => cell.Row == this.Row + 1 && cell.Col == this.Col));
    }

    public Optional<MazeCell> WestGate {
        get => new Optional<MazeCell>(_links.Find(cell => cell.Row == this.Row && cell.Col == this.Col - 1));
    }

    public enum GatePosition {
        None,
        North,
        East,
        South,
        West,
        Zenith,
        Nadir
    }
}