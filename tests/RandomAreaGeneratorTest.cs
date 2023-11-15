using System;
using System.Collections.Generic;
using System.Linq;
using Nour.Play;
using Nour.Play.Areas;
using NUnit.Framework;

[TestFixture]
public class RandomAreaGeneratorTest {

    [Test]
    public void ZoneGenerator_CanGenerateZones() {
        var zonesGenerator = new RandomAreaGenerator(RandomAreaGenerator.GeneratorSettings.Default);
        var enumerator = zonesGenerator.GetEnumerator();

        var sizes = new Dictionary<Vector, int>();
        var tags = new Dictionary<String, int>();
        var types = new Dictionary<AreaType, int>();

        int count = 1000;
        foreach (var area in zonesGenerator) {
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
        var zonesGenerator = new RandomAreaGenerator(new RandomAreaGenerator.GeneratorSettings(
            new Dictionary<Vector, float>() { { new Vector(1, 2), 1 } },
            new Dictionary<AreaType, float>() { { AreaType.Hall, 1 } },
            new Dictionary<string, float>() { { "some_tag", 1 } }
        ));
        var enumerator = zonesGenerator.GetEnumerator();

        var sizes = new Dictionary<Vector, int>();
        var tags = new Dictionary<String, int>();
        var types = new Dictionary<AreaType, int>();

        int count = 10;
        foreach (var area in zonesGenerator) {
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