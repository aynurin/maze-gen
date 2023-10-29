using System.Collections.Generic;
using System.Linq;
using Nour.Play;

public static class TestExtensions {
    public static T ElementAt<T>(this ICollection<T> collection, Vector vector, Vector size) {
        return collection.ElementAt(vector.ToIndex(size));
    }

    public static int ToIndex(this Vector vector, Vector size) {
        return vector.X * size.Y + vector.Y;
    }
}