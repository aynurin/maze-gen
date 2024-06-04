using System;
using System.Linq;

namespace PlayersWorlds.Maps.Serializer {
    public class CellSerializer : IStringSerializer<Cell> {
        public Cell Deserialize(string str) {
            var serializer = new BasicStringReader(typeof(Cell), str);
            var position = Vector.Parse(serializer.ReadValue());
            var cell = new Cell(position, null);
            if (cell.Position != position) {
                throw new ArgumentException(
                    $"Position mismatch: expected {cell.Position}, got " +
                    $"{position} in {str}");
            }
            foreach (var link in serializer.ReadEnumerable()) {
                cell.Link(Vector.Parse(link));
            }
            foreach (var tag in serializer.ReadEnumerable()) {
                cell.Tags.Add(new Cell.CellTag(tag));
            }
            return cell;
        }

        public void Deserialize(Cell obj, string str) {
            var serializer = new BasicStringReader(obj.GetType(), str);
            var position = Vector.Parse(serializer.ReadValue());
            if (obj.Position != position) {
                throw new ArgumentException(
                    $"Position mismatch: expected {obj.Position}, got " +
                    $"{position} in {str}");
            }
            foreach (var link in serializer.ReadEnumerable()) {
                obj.Link(Vector.Parse(link));
            }
            foreach (var tag in serializer.ReadEnumerable()) {
                obj.Tags.Add(new Cell.CellTag(tag));
            }
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
                .WriteEnumerable(obj.Links().Select(v => v.ToString()))
                .WriteEnumerable(obj.Tags.Select(v => v.ToString()))
                .WriteObjectEnd();
        }
    }

}