

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

    public MazeCell? NorthNeighbour {
        get => _neighbours.Find(cell => cell.Row == this.Row - 1 && cell.Col == this.Col);
    }

    public MazeCell? EastNeighbour {
        get => _neighbours.Find(cell => cell.Row == this.Row && cell.Col == this.Col + 1);
    }

    public MazeCell? SouthNeighbour {
        get => _neighbours.Find(cell => cell.Row == this.Row + 1 && cell.Col == this.Col);
    }

    public MazeCell? WestNeighbour {
        get => _neighbours.Find(cell => cell.Row == this.Row && cell.Col == this.Col - 1);
    }

    public MazeCell? NorthGate {
        get => _links.Find(cell => cell.Row == this.Row - 1 && cell.Col == this.Col);
    }

    public MazeCell? EastGate {
        get => _links.Find(cell => cell.Row == this.Row && cell.Col == this.Col + 1);
    }

    public MazeCell? SouthGate {
        get => _links.Find(cell => cell.Row == this.Row + 1 && cell.Col == this.Col);
    }

    public MazeCell? WestGate {
        get => _links.Find(cell => cell.Row == this.Row && cell.Col == this.Col - 1);
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