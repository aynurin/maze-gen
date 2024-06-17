using System;
using System.Linq;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Serializer {
    public class AreaSerializer : IStringSerializer<Area> {
        public Area Deserialize(string str) {
            var stringReader = new BasicStringReader(typeof(Area), str);
            var cellSerializer = new CellSerializer();
            var size = Vector.Parse(stringReader.ReadValue());
            var positionString = stringReader.ReadValue();
            var position = string.IsNullOrEmpty(positionString) ? Vector.Empty : Vector.Parse(positionString);
            var grid = new Grid(position, size);
            var isPositionFixed = bool.Parse(stringReader.ReadValue());
            var type = (AreaType)Enum.Parse(typeof(AreaType), stringReader.ReadValue());
            var tags = stringReader.ReadEnumerable().ToArray();
            var cellsArray = stringReader.ReadEnumerable().ToArray();
            var cells = cellsArray.Select(cell => cellSerializer.Deserialize(cell));
            var childAreas = stringReader.ReadEnumerable().Select(Deserialize).ToArray();
            var area = new Area(grid, cells, isPositionFixed, type, childAreas, tags);
            return area;
        }

        public string Serialize(Area obj) {
            var cellSerializer = new CellSerializer();
            return new BasicStringWriter()
                .WriteObjectStart(obj.GetType())
                .WriteValue(obj.Size.ToString())
                .WriteValue(obj.Grid.Position.IsEmpty ? "" : obj.Grid.Position.ToString())
                .WriteValue(obj.IsPositionFixed.ToString())
                .WriteValue(obj.Type.ToString())
                .WriteEnumerable(obj.Tags)
                .WriteEnumerable(obj.Grid.Select(cell => cellSerializer.Serialize(obj[cell])))
                .WriteEnumerable(obj.ChildAreas().Select(Serialize))
                .WriteObjectEnd();
        }
    }
}