using System.Collections.Generic;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Serializer {
    [TestFixture]
    public class AreaSerializerTest {
        private AreaSerializer _serializer;

        [SetUp]
        public void SetUp() {
            _serializer = new AreaSerializer();
        }

        [Test]
        public void CanSerializeAndDeserializeAnEmptyArea() {
            var area = Area.Create(new Vector(0, 0), new Vector(1, 1), AreaType.Maze);
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.Empty);
            Assert.That(deserializedArea.ChildAreas, Is.Empty);
        }

        [Test]
        public void CanSerializeAndDeserializeAnAreaWithCells() {
            var area = Area.Create(new Vector(0, 0), new Vector(2, 2), AreaType.Maze);
            var tag1 = new Cell.CellTag("tag1");
            var tag2 = new Cell.CellTag("tag2");
            area[new Vector(0, 0)].Tags.Add(tag1);
            area[new Vector(1, 1)].Tags.Add(tag2);
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(2, 2)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.Empty);
            Assert.That(deserializedArea.ChildAreas, Is.Empty);
            Assert.That(deserializedArea[new Vector(0, 0)].Tags, Is.EquivalentTo(new[] { tag1 }));
            Assert.That(deserializedArea[new Vector(1, 1)].Tags, Is.EquivalentTo(new[] { tag2 }));
        }

        [Test]
        public void CanSerializeAndDeserializeAnAreaWithChildAreas() {
            var childArea1 = Area.Create(new Vector(0, 0), new Vector(1, 1), AreaType.Maze);
            var childArea2 = Area.Create(new Vector(1, 1), new Vector(1, 1), AreaType.Maze);
            var area = Area.Create(new Vector(0, 0), new Vector(2, 2), AreaType.Maze);
            area.CreateChildArea(childArea1);
            area.CreateChildArea(childArea2);
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(2, 2)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.Empty);
            var childAreas = new List<Area>(deserializedArea.ChildAreas);
            Assert.That(childAreas.Count, Is.EqualTo(2));
            Assert.That(childAreas[0].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[0].Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(childAreas[0].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[0].Tags, Is.Empty);
            Assert.That(childAreas[1].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[1].Position, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[1].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[1].Tags, Is.Empty);
        }

        [Test]
        public void CanSerializeAndDeserializeAnAreaWithPosition() {
            var area = Area.Create(new Vector(1, 2), new Vector(2, 2), AreaType.Maze);
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(2, 2)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(1, 2)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.Empty);
            Assert.That(deserializedArea.ChildAreas, Is.Empty);
        }

        [Test]
        public void CanSerializeAndDeserializeAnAreaWithTags() {
            var area = Area.Create(new Vector(0, 0), new Vector(1, 1), AreaType.Maze, "tag1", "tag2");
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.EquivalentTo(new[] { "tag1", "tag2" }));
            Assert.That(deserializedArea.ChildAreas, Is.Empty);
        }

        [Test]
        public void CanSerializeAndDeserializeAnAreaWithUnpositionedChildAreas() {
            var childArea1 = Area.CreateUnpositioned(new Vector(1, 1), AreaType.Maze);
            var childArea2 = Area.CreateUnpositioned(new Vector(1, 1), AreaType.Maze);
            var area = Area.Create(new Vector(0, 0), new Vector(2, 2), AreaType.Maze);
            area.CreateChildArea(childArea1);
            area.CreateChildArea(childArea2);
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(2, 2)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.Empty);
            var childAreas = new List<Area>(deserializedArea.ChildAreas);
            Assert.That(childAreas.Count, Is.EqualTo(2));
            Assert.That(childAreas[0].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[0].IsPositionEmpty, Is.True);
            Assert.That(childAreas[0].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[0].Tags, Is.Empty);
            Assert.That(childAreas[1].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[1].IsPositionEmpty, Is.True);
            Assert.That(childAreas[1].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[1].Tags, Is.Empty);
        }

        [Test]
        public void CanSerializeAndDeserializeAnAreaWithPositionedChildAreas() {
            var childArea1 = Area.Create(new Vector(1, 1), new Vector(1, 1), AreaType.Maze);
            var childArea2 = Area.Create(new Vector(2, 2), new Vector(1, 1), AreaType.Maze);
            var area = Area.Create(new Vector(0, 0), new Vector(3, 3), AreaType.Maze);
            area.CreateChildArea(childArea1);
            area.CreateChildArea(childArea2);
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(3, 3)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.Empty);
            var childAreas = new List<Area>(deserializedArea.ChildAreas);
            Assert.That(childAreas.Count, Is.EqualTo(2));
            Assert.That(childAreas[0].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[0].Position, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[0].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[0].Tags, Is.Empty);
            Assert.That(childAreas[1].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[1].Position, Is.EqualTo(new Vector(2, 2)));
            Assert.That(childAreas[1].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[1].Tags, Is.Empty);
        }

        [Test]
        public void CanSerializeAndDeserializeAnAreaWithMixedChildAreas() {
            var childArea1 = Area.Create(new Vector(1, 1), new Vector(1, 1), AreaType.Maze);
            var childArea2 = Area.CreateUnpositioned(new Vector(1, 1), AreaType.Maze);
            var area = Area.Create(new Vector(0, 0), new Vector(3, 3), AreaType.Maze);
            area.CreateChildArea(childArea1);
            area.CreateChildArea(childArea2);
            var serializedArea = _serializer.Serialize(area);
            var deserializedArea = _serializer.Deserialize(serializedArea);
            Assert.That(deserializedArea.Size, Is.EqualTo(new Vector(3, 3)));
            Assert.That(deserializedArea.Position, Is.EqualTo(new Vector(0, 0)));
            Assert.That(deserializedArea.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(deserializedArea.Tags, Is.Empty);
            var childAreas = new List<Area>(deserializedArea.ChildAreas);
            Assert.That(childAreas.Count, Is.EqualTo(2));
            Assert.That(childAreas[0].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[0].Position, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[0].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[0].Tags, Is.Empty);
            Assert.That(childAreas[1].Size, Is.EqualTo(new Vector(1, 1)));
            Assert.That(childAreas[1].IsPositionEmpty, Is.True);
            Assert.That(childAreas[1].Type, Is.EqualTo(AreaType.Maze));
            Assert.That(childAreas[1].Tags, Is.Empty);
        }
    }
}
