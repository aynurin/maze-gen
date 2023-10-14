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

        for (int i = 0; i < 1000; i++) {
            Assert.True(enumerator.MoveNext());
            var area = enumerator.Current;

            //Assert.Greater(area.Cells.Count, 0);
            Assert.Greater(area.Tags.Length, 0);

            foreach (var tag in area.Tags) {
                Assert.IsNotNullOrEmpty(tag);
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

        Assert.AreEqual(1000, types.Values.Sum());
        Assert.AreEqual(2, types.Count, 2);
        Assert.AreEqual(1000, tags.Values.Sum());
        Assert.AreEqual(5, tags.Count, 5);
    }
}