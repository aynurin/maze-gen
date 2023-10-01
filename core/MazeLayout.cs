using System;
using System.Collections.Generic;

namespace Nour.Play.Maze {
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

        /// <summary>
        /// Creates a random MazeLayout of the given size.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static MazeLayout GenerateRandom(Size size) {
            MazeLayoutManager manager = new MazeLayoutManager(size, new RandomZoneGenerator());
            // 1. Generate maze zones for the given size:
            var zones = manager.GenerateZones();
            // 2. Place the generated zones in the maze layout
            // 3. 
            // TODO: Complete implementation of maze layout
            // TODO: Design a test for maze layout validation, e.g. generate
            //       1000 mazes and check on some rules.
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
}