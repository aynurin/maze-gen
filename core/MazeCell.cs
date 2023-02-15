

public class MazeCell {
    private readonly Dictionary<GatePosition, MazeCell> _links = new Dictionary<GatePosition, MazeCell>();

    public int Row { get; private set; }
    public int Col { get; private set; }
    public Dictionary<GatePosition, MazeCell> Links {
        get => _links;
    }

    public MazeCell(int row, int col) {
        Row = row;
        Col = col;
    }

    public void Link(GatePosition position, MazeCell cell) {
        _links[position] = cell;
    }

    public void Unlink(GatePosition position) {
        if (_links.ContainsKey(position)) {
            _links.Remove(position);
        }
    }

    public MazeCell? NorthGate {
        get => _links.ContainsKey(GatePosition.North) ? _links[GatePosition.North] : null;
    }

    public MazeCell? EastGate {
        get => _links.ContainsKey(GatePosition.East) ? _links[GatePosition.East] : null;
    }

    public MazeCell? SouthGate {
        get => _links.ContainsKey(GatePosition.South) ? _links[GatePosition.South] : null;
    }

    public MazeCell? WestGate {
        get => _links.ContainsKey(GatePosition.West) ? _links[GatePosition.West] : null;
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