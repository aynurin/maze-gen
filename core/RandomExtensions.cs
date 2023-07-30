using System.Collections.Generic;

internal static class RandomExtensions {
    private static System.Random _random = new System.Random();
    public static T GetRandom<T>(this IList<T> items) {
        return items[_random.Next(items.Count)];
    }
    public static MazeCell GetRandom<T>(this IList<MazeCell> items) {
        return items[_random.Next(items.Count)];
    }
}