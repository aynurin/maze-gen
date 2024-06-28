using NUnit.Framework;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Serializer;

namespace PlayersWorlds.Maps {

    [TestFixture]
    internal class CellTest : Test {
        [Test]
        public void CellTagEqualityTest() {
            var tag = new Cell.CellTag("test");
            Assert.That(tag.GetHashCode(), Is.EqualTo("test".GetHashCode()));
            Assert.That(tag.Equals("test"), Is.True);
            Assert.That(tag.ToString(), Is.EqualTo("test"));
        }

        [Test]
        public void Serialize() {
            var env = Area.CreateMaze(new Vector(5, 5));
            var actual = new CellSerializer(AreaType.None)
                            .Serialize(env[new Vector(3, 0)]);
            var expected = "Cell:{Maze;;}";
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}