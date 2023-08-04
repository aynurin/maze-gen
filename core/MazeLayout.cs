using System;
using System.Collections.Generic;

public class MazeLayout {
    private readonly Size _size;
    private readonly List<MazeCell> _cells;
    private readonly Dictionary<Point, MazeCell> _cellCoordinates;
    private readonly Dictionary<Point, MazeZone> _zones;

    public IList<MazeCell> Cells => _cells.AsReadOnly();

    public MazeCell CellAt(Point coords) => _cellCoordinates[coords];

    public Size Size => _size;

    public static MazeLayout GenerateRandom(int rows, int cols) =>
        GenerateRandom(new Size(rows, cols));

    public static MazeLayout GenerateRandom(Size size) {
        MazeLayoutManager manager = new MazeLayoutManager(size);
        var zones = manager.GenerateZones(size);
        // _size = dimensions;
        throw new NotImplementedException();
    }

    // getAllCells()
    //      needs to have only one item per cell
    //          
    // getCellAt(Coords)
    //      needs to map to zones when necessary. E.g. if several row,col fall
    //      to the same Zone, the same cell belonging to that zone will be retrieved
    //      car return Null because of Fill zones
    // Internal data store: Dictionary
    //          key -> element Coord
    //          value -> cell (Values getter is a O(1) op)
}