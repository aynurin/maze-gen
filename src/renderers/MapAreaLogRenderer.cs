
using System.Collections.Generic;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Renderers {
    public class MapAreaLogRenderer {
        private readonly Log _log;
        public MapAreaLogRenderer(Log log) {
            _log = log;
        }
        public void Draw(Vector envSize, IEnumerable<MapArea> areas) {
            var bufferSize = new Vector(envSize.X * 2, envSize.Y * 2 * 2);
            var buffer = new AsciiBuffer(bufferSize.X, bufferSize.Y, true);
            var offset = new Vector(envSize.X / 2, envSize.Y / 2);
            DrawRect(buffer, new Vector(offset.X, offset.Y), envSize, s_mazeChars);
            // transpile room positions to reflect reversed X in Terminal
            areas.ForEach(area => DrawRect(buffer,
                new Vector(envSize.X - area.Position.X - area.Size.X + offset.X,
                           area.Position.Y + offset.Y),
                           area.Size,
                           s_roomChars));
            _log?.Buffered.D(4, buffer.ToString());
        }

        private void DrawRect(AsciiBuffer buffer, Vector pos, Vector size, char[] wallChars) {
            size = new Vector(size.X, size.Y * 2);
            pos = new Vector(pos.X, pos.Y * 2);
            buffer.PutC(pos.X, pos.Y, wallChars[2]);
            buffer.PutC(pos.X, pos.Y + size.Y, wallChars[3]);
            buffer.PutC(pos.X + size.X, pos.Y, wallChars[4]);
            buffer.PutC(pos.X + size.X, pos.Y + size.Y, wallChars[5]);
            for (var row = 1; row < size.X; row++) {
                buffer.PutC(pos.X + row, pos.Y, wallChars[1]);
                buffer.PutC(pos.X + row, pos.Y + size.Y, wallChars[1]);
            }
            for (var col = 1; col < size.Y; col++) {
                buffer.PutC(pos.X, pos.Y + col, wallChars[0]);
                buffer.PutC(pos.X + size.X, pos.Y + col, wallChars[0]);
            }
        }
        private static readonly char[] s_mazeChars = new char[] { '═', '║', '╔', '╗', '╚', '╝' };
        private static readonly char[] s_roomChars = new char[] { '─', '│', '┌', '┐', '└', '┘' };
    }
}