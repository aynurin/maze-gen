using System;
using System.Collections.Generic;
using System.Linq;
using Nour.Play.Maze;
using NUnit.Framework;

[TestFixture]
public class RandomZoneGeneratorTest {

    [Test]
    public void ZoneGenerator_CanGenerateZones() {
        var zonesGenerator = new RandomZoneGenerator();
        var enumerator = zonesGenerator.GetEnumerator();

        var sizes = new Dictionary<Size, int>();
        var tags = new Dictionary<String, int>();
        var types = new Dictionary<MazeZone.ZoneType, int>();

        for (int i = 0; i < 1000; i++) {
            Assert.True(enumerator.MoveNext());
            var zone = enumerator.Current;

            int count;

            Assert.Greater(zone.Size.Rows, 0);
            Assert.Greater(zone.Size.Columns, 0);
            if (sizes.TryGetValue(zone.Size, out count)) {
                sizes[zone.Size] = count + 1;
            } else {
                sizes.Add(zone.Size, 1);
            }

            Assert.Greater(zone.Tags.Length, 0);
            foreach (var tag in zone.Tags) {
                Assert.IsNotNullOrEmpty(tag);
                if (tags.TryGetValue(tag, out count)) {
                    tags[tag] = count + 1;
                } else {
                    tags.Add(tag, 1);
                }
            }

            Assert.AreNotEqual(zone.Type, MazeZone.ZoneType.None);
            if (types.TryGetValue(zone.Type, out count)) {
                types[zone.Type] = count + 1;
            } else {
                types.Add(zone.Type, 1);
            }
        }

        Assert.AreEqual(1000, sizes.Values.Sum());
        Assert.AreEqual(7, sizes.Count, 7);
        Assert.AreEqual(1000, types.Values.Sum());
        Assert.AreEqual(2, types.Count, 2);
        Assert.AreEqual(1000, tags.Values.Sum());
        Assert.AreEqual(5, tags.Count, 5);
    }
}