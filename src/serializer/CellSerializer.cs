using System;
using System.Linq;

namespace PlayersWorlds.Maps.Serializer {
    public class CellSerializer : IStringSerializer<Cell> {
        public Cell Deserialize(string str) {
            var serializer = new BasicStringReader(typeof(Cell), str);
            var position = Vector.Parse(serializer.ReadValue());
            var cell = new Cell(position);
            foreach (var link in serializer.ReadEnumerable()) {
                cell.HardLinks.Add(Vector.Parse(link));
            }
            foreach (var tag in serializer.ReadEnumerable()) {
                cell.Tags.Add(new Cell.CellTag(tag));
            }
            return cell;
        }

        /// <summary>
        /// Serializes the specified cell into a string of the form:
        /// <c>{POSITION;[LINK,[LINK,...]];[TAG,[TAG,...]]}</c>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string Serialize(Cell obj) {
            return new BasicStringWriter()
                .WriteObjectStart(obj.GetType())
                .WriteValue(obj.Position.ToString())
                .WriteEnumerable(obj.HardLinks.Select(v => v.ToString()))
                .WriteEnumerable(obj.Tags.Select(v => v.ToString()))
                .WriteObjectEnd();
        }
    }

}