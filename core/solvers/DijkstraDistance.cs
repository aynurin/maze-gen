public class DijkstraDistances {
    private readonly Dictionary<MazeCell, int> _distances;

    public int this[MazeCell cell] { get => _distances[cell]; }

    private DijkstraDistances(Dictionary<MazeCell, int> distances) {
        _distances = distances;
    }

    public static DijkstraDistances Find(MazeCell startingCell) {
        var distances = new Dictionary<MazeCell, int>();
        distances.Add(startingCell, 0);
        var stack = new Stack<MazeCell>();
        stack.Push(startingCell);
        MazeCell nextCell;
        while (true) {
            try {
                nextCell = stack.Pop();
            } catch (InvalidOperationException) {
                break;
            }
            var distance = distances[nextCell];
            foreach (var neighbour in nextCell.Links.Values) {
                if (!distances.ContainsKey(neighbour)) {
                    distances.Add(neighbour, distance + 1);
                    stack.Push(neighbour);
                }
            }
        }
        return new DijkstraDistances(distances);
    }
}