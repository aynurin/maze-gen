using Nour.Play.Maze.PostProcessing;

namespace Nour.Play.Maze {
    public abstract class MazeGenerator {
        public abstract void GenerateMaze(Maze2D map);

        public static Maze2D Generate<T>(Vector size)
            where T : MazeGenerator, new() {
            var maze = new Maze2D(size);
            (new T()).GenerateMaze(maze);
            maze.Attributes.Set(DeadEnd.DeadEndAttribute, DeadEnd.Find(maze));
            maze.Attributes.Set(DijkstraDistance.LongestTrailAttribute,
                DijkstraDistance.FindLongestTrail(maze));
            return maze;
        }
    }
}