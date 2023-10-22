using System;
using System.Collections.Generic;

namespace Nour.Play.Maze {
    public abstract class MazeGenerator {
        public abstract void GenerateMaze(Maze2D map);

        public static Maze2D Generate<T>(Vector size)
            where T : MazeGenerator, new() {
            var map = new Maze2D(size);
            (new T()).GenerateMaze(map);
            return map;
        }
    }
}