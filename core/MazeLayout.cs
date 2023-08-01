using System;
using System.Linq;
using System.Collections.Generic;

public class MazeLayout {
    private readonly Dimensions _size;
    private readonly List<MazeCell> _cells;
    private readonly Dictionary<Dimensions, MazeCell> _cellCoordinates;
    private readonly Dictionary<Dimensions, MazeZone> _zones;

    public IList<MazeCell> Cells => _cells.AsReadOnly();

    public MazeCell CellAt(Dimensions coords) => _cellCoordinates[coords];

    public Dimensions Size => _size;

    public static MazeLayout GenerateRandom(int rows, int cols) =>
        GenerateRandom(new Dimensions(rows, cols));

    public static MazeLayout GenerateRandom(Dimensions dimensions) {
        MazeLayoutManager manager = new MazeLayoutManager(dimensions);
        var zones = manager.GenerateZones(dimensions);
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