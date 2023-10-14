using System;
using System.Collections.Generic;

namespace Nour.Play.Maze {
    public class MazeLayout {
        private readonly Vector _size;
        private readonly List<Cell> _cells;
        private readonly Dictionary<Vector, Cell> _cellCoordinates;
        private readonly Dictionary<Vector, MazeZone> _zones;

        public IList<Cell> Cells => _cells.AsReadOnly();

        public Cell CellAt(Vector coords) => _cellCoordinates[coords];

        public Vector Vector => _size;

        public static MazeLayout GenerateRandom(int x, int y) =>
            GenerateRandom(new Vector(x, y));

        /// <summary>
        /// Creates a random MazeLayout of the given size.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static MazeLayout GenerateRandom(Vector size) {
            MazeLayoutManager manager = new MazeLayoutManager(size, new RandomZoneGenerator());
            // 1. Generate maze areas for the given size:
            var areas = manager.GenerateZones();
            // 2. Place the generated areas in the maze layout
            // 3. 
            // TODO: Complete implementation of maze layout
            // TODO: Design a test for maze layout validation, e.g. generate
            // TODO: 2. Place the same cell repeatedly for Halls
            // TODO: 3. Halls can have many neighbors on any side.
            //       1000 mazes and check on some rules.
            throw new NotImplementedException();
        }

        // getAllCells()
        //      needs to have only one item per cell
        //          
        // getCellAt(Coords)
        //      needs to map to areas when necessary. E.g. if several x,y fall
        //      to the same Area, the same cell belonging to that area will be retrieved
        //      car return Null because of Fill areas
        // Internal data store: Dictionary
        //          key -> element Coord
        //          value -> cell (Values getter is a O(1) op)
    }
}