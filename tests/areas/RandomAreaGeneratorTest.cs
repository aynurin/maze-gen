using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps;
using PlayersWorlds.Maps.Areas;

[TestFixture]
public class RandomAreaGeneratorTest {

    [Test]
    public void ZoneGenerator_CanGenerateZones() {
        var zonesGenerator = new RandomAreaGenerator(
            RandomAreaGenerator.RandomAreaGeneratorSettings.Default);
        var sizes = new Dictionary<Vector, int>();
        var tags = new Dictionary<string, int>();
        var types = new Dictionary<AreaType, int>();
        var count = 1000;
        foreach (var area in zonesGenerator.Generate(count)) {
            if (--count < 0) break;
            //Assert.Greater(area.Cells.Count, 0);
            Assert.Greater(area.Tags.Length, 0);

            if (sizes.ContainsKey(area.Size)) {
                sizes[area.Size] += +1;
            } else {
                sizes.Add(area.Size, 1);
            }

            foreach (var tag in area.Tags) {
                Assert.IsNotNull(tag);
                Assert.IsNotEmpty(tag);
                if (tags.ContainsKey(tag)) {
                    tags[tag] += +1;
                } else {
                    tags.Add(tag, 1);
                }
            }

            Assert.AreNotEqual(area.Type, AreaType.None);
            if (types.ContainsKey(area.Type)) {
                types[area.Type] += 1;
            } else {
                types.Add(area.Type, 1);
            }
        }

        Assert.AreEqual(1000, sizes.Values.Sum());
        Assert.AreEqual(7, sizes.Count);
        Assert.AreEqual(1000, types.Values.Sum());
        Assert.AreEqual(2, types.Count);
        Assert.AreEqual(1000, tags.Values.Sum());
        Assert.AreEqual(5, tags.Count);
    }

    [Test]
    public void ZoneGenerator_CustomSettings() {
        var zonesGenerator = new RandomAreaGenerator(
            new RandomAreaGenerator.RandomAreaGeneratorSettings(
                0.3f,
                new Dictionary<Vector, float>() { { new Vector(1, 2), 1 } },
                new Dictionary<AreaType, float>() { { AreaType.Hall, 1 } },
                new Dictionary<string, float>() { { "some_tag", 1 } }
        ));
        var sizes = new Dictionary<Vector, int>();
        var tags = new Dictionary<string, int>();
        var types = new Dictionary<AreaType, int>();
        var count = 10;
        foreach (var area in zonesGenerator.Generate(count)) {
            if (--count < 0) break;
            //Assert.Greater(area.Cells.Count, 0);
            Assert.Greater(area.Tags.Length, 0);

            if (sizes.ContainsKey(area.Size)) {
                sizes[area.Size] += +1;
            } else {
                sizes.Add(area.Size, 1);
            }

            foreach (var tag in area.Tags) {
                Assert.IsNotNull(tag);
                Assert.IsNotEmpty(tag);
                if (tags.ContainsKey(tag)) {
                    tags[tag] += +1;
                } else {
                    tags.Add(tag, 1);
                }
            }

            Assert.AreNotEqual(area.Type, AreaType.None);
            if (types.ContainsKey(area.Type)) {
                types[area.Type] += 1;
            } else {
                types.Add(area.Type, 1);
            }
        }

        Assert.AreEqual(10, sizes.Values.Sum());
        Assert.AreEqual(1, sizes.Count);
        Assert.AreEqual(10, types.Values.Sum());
        Assert.AreEqual(1, types.Count);
        Assert.AreEqual(10, tags.Values.Sum());
        Assert.AreEqual(1, tags.Count);
    }
}