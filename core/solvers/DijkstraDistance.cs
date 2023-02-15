public class DijkstraDistances {
    private readonly Dictionary<MazeCell, int> _distances;
    private List<MazeCell>? _solution;

    public int this[MazeCell cell] { get => _distances[cell]; }
    public List<MazeCell>? Solution { get => _solution; }

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
            foreach (var neighbour in nextCell.Links) {
                if (!distances.ContainsKey(neighbour)) {
                    distances.Add(neighbour, distance + 1);
                    stack.Push(neighbour);
                }
            }
        }
        return new DijkstraDistances(distances);
    }

    public List<MazeCell> Solve(MazeCell targetCell) {
        var solution = new List<MazeCell>() { targetCell };
        while (_distances[targetCell] > 0) {
            targetCell = targetCell.Links.OrderBy(cell => _distances[cell]).First();
            solution.Add(targetCell);
        }
        return _solution = solution;
    }

    public static DijkstraDistances FindLongest(MazeCell arbitrary) {
        var distances = Find(arbitrary);
        var startingPoint = distances._distances.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).First();
        distances = Find(startingPoint);
        var targetPoint = distances._distances.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).First();
        distances.Solve(targetPoint);
        return distances;
    }
}