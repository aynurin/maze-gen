using System;
using System.Collections.Generic;
using System.Linq;
using Nour.Play;
using Nour.Play.Areas;
using Nour.Play.Maze;

public class AreaDistributor {
    private Log _log;

    public AreaDistributor(Log log) {
        _log = log;
    }

    class EpochData {
        public int Epoch { get; set; }
        public double AvgMagnitude { get; internal set; }
        public double TotalMagnitude { get; internal set; }
        public bool LayoutIsValid { get; internal set; }
        public List<Vector> RoomPositions { get; internal set; }
        public List<Vector> RoomsShifts { get; internal set; }
        public Vector TotalRoomShift { get; internal set; }
        public VectorD AvgRoomShift { get; internal set; }
    }

    public List<MapArea> DistributePlacedRooms2(Maze2D maze,
                                                List<MapArea> areas,
                                                int maxEpochs) {
        Draw(_log, maze.Size, areas);
        var simulator = new EvolvingSimulator(maxEpochs, 10);
        var system = new MapAreasSystem(_log, maze.Size,
            areas,
            r => { },
            r => OnEpoch(maze, areas, r));
        simulator.Evolve(system);
        return areas;
    }

    private void OnEpoch(Maze2D maze,
                         List<MapArea> areas,
                         EpochResult _) {
        Draw(_log, maze.Size, areas);
    }

    // TODO: Create a separate class for drawing.
    public static void Draw(Log log, Vector envSize, IEnumerable<IObject2D> rooms) {
        var bufferSize = new Vector(envSize.X * 2, envSize.Y * 2 * 2);
        var buffer = new AsciiBuffer(bufferSize.X, bufferSize.Y, true);
        var offset = new Vector(envSize.X / 2, envSize.Y / 2);
        DrawRect(buffer, new Vector(offset.X, offset.Y), envSize, mazeChars);
        // transpile room positions to reflect reversed X in Terminal
        rooms.ForEach(room => DrawRect(buffer,
            new Vector(envSize.X - room.GetPosition().X - room.GetSize().X + offset.X,
                       room.GetPosition().Y + offset.Y),
                       room.GetSize(),
                       roomChars));
        log.Buffered.D(4, buffer.ToString());
    }

    private static void DrawRect(AsciiBuffer buffer, Vector pos, Vector size, char[] wallChars) {
        size = new Vector(size.X, size.Y * 2);
        pos = new Vector(pos.X, pos.Y * 2);
        buffer.PutC(pos.X, pos.Y, wallChars[2]);
        buffer.PutC(pos.X, pos.Y + size.Y, wallChars[3]);
        buffer.PutC(pos.X + size.X, pos.Y, wallChars[4]);
        buffer.PutC(pos.X + size.X, pos.Y + size.Y, wallChars[5]);
        for (int row = 1; row < size.X; row++) {
            buffer.PutC(pos.X + row, pos.Y, wallChars[1]);
            buffer.PutC(pos.X + row, pos.Y + size.Y, wallChars[1]);
        }
        for (int col = 1; col < size.Y; col++) {
            buffer.PutC(pos.X, pos.Y + col, wallChars[0]);
            buffer.PutC(pos.X + size.X, pos.Y + col, wallChars[0]);
        }
    }
    private static char[] mazeChars = new char[] { '═', '║', '╔', '╗', '╚', '╝' };
    private static char[] roomChars = new char[] { '─', '│', '┌', '┐', '└', '┘' };

    public static Vector RandomRoomPosition(Vector maxPosition) {
        var size = new Vector(
            GlobalRandom.Next(0, maxPosition.X),
            GlobalRandom.Next(0, maxPosition.Y)
        );
        return size;
    }

    // TODO: swap x and y
    // TODO: change VectorD to Single
    // TODO: I still have X and Y all messed up.

    // TODO: make a struct? Get rid in favor of MapArea?
    public class Room : IObject2D {
        public VectorD Position { get; set; }
        // TODO: Size is integer
        public VectorD Size { get; private set; }

        public VectorD Center => Position + Size / 2D;
        public VectorD OpposingForce { get; set; } = VectorD.Zero2D;

        public double LowX => Position.X;
        public double HighX => Position.X + Size.X;
        public double LowY => Position.Y;
        public double HighY => Position.Y + Size.Y;

        public Room(VectorD position, VectorD size) {
            Position = position;
            Size = size;
        }

        public override string ToString() {
            return $"P{Position};S{Size}";
        }

        internal static Room Parse(string s) {
            var parameters = s.Trim(' ').Split(';').Select(x => {
                VectorD v;
                try {
                    v = VectorD.Parse(x.Trim('P', 'S'));
                } catch (System.FormatException ex) {
                    throw new FormatException($"AreaDistributorTest.Room.Parse (\"{x}\")", ex);
                }
                return v;
            }).ToArray();
            return new Room(parameters[0], parameters[1]);
        }

        internal bool Overlaps(Room other) {
            if (this == other)
                throw new InvalidOperationException("Can't compare with self");
            var noOverlap = this.HighX <= other.LowX || this.LowX >= other.HighX;
            noOverlap |= this.HighY <= other.LowY || this.LowY >= other.HighY;
            return !noOverlap;
        }
        internal bool Contains(VectorD point) {
            return point.X >= Size.X && point.X <= Size.X + Position.X &&
                   point.Y >= Size.Y && point.Y <= Size.Y + Position.Y;
        }

        internal bool Fits(Room other) {
            // Check if the inner rectangle is completely within the outer rectangle.
            return this.LowX >= other.LowX &&
                   this.HighX <= other.HighX &&
                   this.LowY >= other.LowY &&
                   this.HighY <= other.HighY;
        }

        Vector IObject2D.GetPosition() => Position.RoundToInt();

        Vector IObject2D.GetSize() => Size.RoundToInt();
    }
}