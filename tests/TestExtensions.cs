using System;
using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps;
using PlayersWorlds.Maps.Areas;

public static class TestExtensions {
    public static T ElementAt<T>(this ICollection<T> collection, Vector vector, Vector areaSize) {
        return collection.ElementAt(vector.ToIndex(areaSize));
    }

    public static int ToIndex(this Vector vector, Vector size) {
        return vector.Y * size.X + vector.X;
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

    public static Vector Position(this Cell cell, Map2D area) {
        return Vector.FromIndex(area.Cells.IndexOf(cell), area.Size.X);
    }
}