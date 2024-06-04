using NUnit.Framework;
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
            var env = Area.CreateEnvironment(new Vector(5, 5));
            var actual = new CellSerializer().Serialize(env.Cells[3]);
            var expected = "Cell:{3x0;;}";
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}