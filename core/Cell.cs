namespace Nour.Play {
    public class Cell {
        public CellType Type { get; set; }

        public enum CellType {
            None,
            Trail,
            Wall,
            Edge
        }
    }
}