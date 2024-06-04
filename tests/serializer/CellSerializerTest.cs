using NUnit.Framework;
using PlayersWorlds.Maps;
using PlayersWorlds.Maps.Serializer;

namespace PlayersWorlds.Maps.Serializer {

    [TestFixture]
    internal class CellSerializerTest : Test {
        [Test]
        public void CanSerializeEmptyCell() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var cell = env[new Vector(1, 1)];
            var actual = new CellSerializer().Serialize(cell);
            var expected = "Cell:{1x1;;}";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeCellWithLinks() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var cell = env[new Vector(1, 1)];
            cell.Link(cell.Position + Vector.North2D);
            cell.Link(cell.Position + Vector.East2D);
            var actual = new CellSerializer().Serialize(cell);
            var expected = "Cell:{1x1;[1x2,2x1];}";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeCellWithTags() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var cell = env[new Vector(1, 1)];
            cell.Tags.Add(new Cell.CellTag("foo"));
            cell.Tags.Add(new Cell.CellTag("bar"));
            var actual = new CellSerializer().Serialize(cell);
            var expected = "Cell:{1x1;;[foo,bar]}";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CanSerializeCellWithLinksAndTags() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var cell = env[new Vector(1, 1)];
            cell.Link(cell.Position + Vector.North2D);
            cell.Link(cell.Position + Vector.East2D);
            cell.Tags.Add(new Cell.CellTag("foo"));
            cell.Tags.Add(new Cell.CellTag("bar"));
            var actual = new CellSerializer().Serialize(cell);
            var expected = "Cell:{1x1;[1x2,2x1];[foo,bar]}";
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CanDeserializeEmptyCell() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var cell = env[new Vector(1, 1)];
            var serializer = new CellSerializer();
            serializer.Deserialize(cell, "Cell:{1x1;;}");
            Assert.That(cell.Position, Is.EqualTo(new Vector(1, 1)));
            Assert.That(cell.Links().Count, Is.EqualTo(0));
            Assert.That(cell.Tags.Count, Is.EqualTo(0));
        }

        [Test]
        public void CanDeserializeCellWithLinks() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var cell = env[new Vector(1, 1)];
            var serializer = new CellSerializer();
            serializer.Deserialize(cell, "Cell:{1x1;[1x2,2x1];}");
            Assert.That(cell.Position, Is.EqualTo(new Vector(1, 1)));
            Assert.That(cell.Links().Count, Is.EqualTo(2));
            Assert.That(cell.Links().Contains(cell.Position + Vector.North2D), Is.True);
            Assert.That(cell.Links().Contains(cell.Position + Vector.East2D), Is.True);
            Assert.That(cell.Tags.Count, Is.EqualTo(0));
        }

        [Test]
        public void CanDeserializeCellWithTags() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var cell = env[new Vector(1, 1)];
            var serializer = new CellSerializer();
            serializer.Deserialize(cell, "Cell:{1x1;;[foo,bar]}");
            Assert.That(cell.Position, Is.EqualTo(new Vector(1, 1)));
            Assert.That(cell.Links().Count, Is.EqualTo(0));
            Assert.That(cell.Tags.Count, Is.EqualTo(2));
            Assert.That(cell.Tags.Contains(new Cell.CellTag("foo")), Is.True);
            Assert.That(cell.Tags.Contains(new Cell.CellTag("bar")), Is.True);
        }

        [Test]
        public void CanDeserializeCellWithLinksAndTags() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var cell = env[new Vector(1, 1)];
            var serializer = new CellSerializer();
            serializer.Deserialize(cell, "Cell:{1x1;[1x2,2x1];[foo,bar]}");
            Assert.That(cell.Position, Is.EqualTo(new Vector(1, 1)));
            Assert.That(cell.Links().Count, Is.EqualTo(2));
            Assert.That(cell.Links().Contains(cell.Position + Vector.North2D), Is.True);
            Assert.That(cell.Links().Contains(cell.Position + Vector.East2D), Is.True);
            Assert.That(cell.Tags.Count, Is.EqualTo(2));
            Assert.That(cell.Tags.Contains(new Cell.CellTag("foo")), Is.True);
            Assert.That(cell.Tags.Contains(new Cell.CellTag("bar")), Is.True);
        }

        [Test]
        public void ThrowsIfDeserializingAWrongCell() {
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var cell = env[new Vector(1, 1)];
            var serializer = new CellSerializer();
            Assert.That(() => serializer.Deserialize(cell, "Cell:{0x0;;}"), Throws.ArgumentException);
        }
    }
}