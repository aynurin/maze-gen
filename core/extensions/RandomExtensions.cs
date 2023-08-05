using System.Collections.Generic;

namespace Nour.Play.Maze {
    internal static class RandomExtensions {
        private static System.Random _random = new System.Random();
        public static T GetRandom<T>(this IList<T> items) {
            return items[_random.Next(items.Count)];
        }
        public static MazeCell GetRandom<T>(this IList<MazeCell> items) {
            return items[_random.Next(items.Count)];
        }
        public static MazeZone RandomRotate(this MazeZone zone) {
            if (_random.Next() % 2 == 0)
                return zone;
            return zone.Rotate2d();
        }

        public static float RandomSingle() => _random.Next(99999) / 100000f;
    }
}