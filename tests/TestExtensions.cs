using System;
using System.Collections.Generic;
using System.Linq;
using Nour.Play;
using Nour.Play.Areas;

public static class TestExtensions {
    public static T ElementAt<T>(this ICollection<T> collection, Vector vector, Vector size) {
        return collection.ElementAt(vector.ToIndex(size));
    }

    public static int ToIndex(this Vector vector, Vector size) {
        return vector.X * size.Y + vector.Y;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serialized"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static MapArea ToArea(this string serialized) {
        var parts = serialized.Split(new char[] { ';', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) {
            throw new FormatException($"Could not parse string as MapArea: {serialized}");
        }
        var position = VectorD.Parse(parts[0]).RoundToInt();
        var size = VectorD.Parse(parts[1]).RoundToInt();
        return new MapArea(AreaType.None, size, position);
    }
}