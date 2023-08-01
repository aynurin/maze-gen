using System;
using System.Collections.Generic;

// ? The issue is that Halls can be entered, so they should act as cells,
// ? and Fills can't be entered, so they are invisible to all algorithms. 
// ? For Dijkstra, Halls should act as regular cells, but with increased passing
// ? complexity.
// Zone size definition: Given that the mazes can be autogenerated and of
// different sizes, we can't easily predefine zones counts and sizes, so we
// might want to calculate zones based on some kind of rules. E.g.,
// X% of the maze areas is allocated for zones.
// zones will be added based on probability to best fill the X%, e.g.:
// 2x2 : 60% (means 60% of mazes will have a zone of 2 rows x 2 columns)
// 2x3 : 60%
// 3x4 : 40%
// 3x6 : 40%
// 4x8 : 20%
// 5x6 : 20%
// 7x7 : 5%
// TODO: Create a zone generator
// TODO: Update generators to honor zones
// TODO: Update Dijkstra to use hall sizes
public class MazeZone {
    public ZoneType Type { get; private set; }
    public string[] Tags { get; private set; }
    public Dimensions Size { get; private set; }

    public MazeZone(ZoneType type, Dimensions size, params string[] tags) {
        Type = type;
        Size = size;
        Tags = tags;
    }

    public MazeZone Rotate2d() {
        return new MazeZone(Type, Size.Rotate2d(), Tags);
    }

    private static Dictionary<Dimensions,float> _dimensionsProbabilities = new Dictionary<Dimensions, float>() {
        { new Dimensions(2, 2), 0.25f },
        { new Dimensions(2, 3), 0.25f },
        { new Dimensions(3, 4), 0.15f },
        { new Dimensions(3, 6), 0.15f },
        { new Dimensions(4, 8), 0.075f },
        { new Dimensions(5, 6), 0.075f },
        { new Dimensions(7, 7), 0.005f }
    };

    private static Dictionary<ZoneType,float> _zoneTypeProbabilities = new Dictionary<ZoneType, float>() {
        { ZoneType.Fill, 0.7f },
        { ZoneType.Hall, 0.3f }
    };


    private static Dictionary<String,float> _tagsProbabilities = new Dictionary<String, float>() {
        { "room", 0.5f },
        { "lake", 0.15f },
        { "dirt", 0.15f },
        { "swamp", 0.15f },
        { "void", 0.05f }
    };

    // 2.45 = 1
    // 0.6 = x

    public static MazeZone GetRandomZone() => new MazeZone(
            PickRandom(_zoneTypeProbabilities),
            PickRandom(_dimensionsProbabilities),
            PickRandom(_tagsProbabilities)
        ).RandomRotate();

    private static T PickRandom<T>(IDictionary<T, float> distribution) {
        var random = RandomExtensions.RandomSingle();
        var cumulativeProb = 0f;
        T lastItem = default(T);
        foreach (var couple in distribution) {
            cumulativeProb += couple.Value;
            if (random < cumulativeProb) {
                return couple.Key;
            }
            lastItem = couple.Key;
        }
        return lastItem;
    }

    public enum ZoneType {
        /// E.g., a hall with walls around it or a valley with a lake and a
        /// shore around the lake, the player can enter and walk the hall or
        /// the shores.
        Hall,
        /// E.g., a lake or a mount the player can not enter.
        Fill,
    }
}