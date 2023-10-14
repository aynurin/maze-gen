using System;
using System.Collections.Generic;

namespace Nour.Play.Maze {
    public abstract class MazeGenerator {
        public abstract void GenerateMaze(Map2D map);

        public static Map2D Generate<T>(Vector size)
            where T : MazeGenerator, new() {
            var map = new Map2D(size);
            (new T()).GenerateMaze(map);
            return map;
        }
    }
}