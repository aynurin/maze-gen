using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Nour.Play;
using Nour.Play.Maze;

public class AreaDistributor {
    private Log _log;

    public AreaDistributor(Log log) {
        _log = log;
    }

    public List<Room> DistributePlacedRooms(Maze2D maze, List<Room> rooms) {
        Draw(maze, rooms);
        var forces = new List<VectorD>();

        for (int j = 0; j < 100; j++) {
            forces.Clear();
            foreach (var room in rooms) {
                forces.Add((GetOtherRoomsForce(room, maze.Size, rooms) +
                             GetMapForce(room, new Room(new Vector(0, 0), maze.Size))));
            }
            if (forces.All(f => f.MagnitudeSq < 1.5)) {
                _log.Buffered.D($"Total magnitude after {j} iterations = " + forces.Aggregate((acc, a) => acc + a).MagnitudeSq);
                break;
            };
            for (int i = 0; i < rooms.Count; i++) {
                rooms[i].Position += forces[i].RoundToInt();
            }
        }

        Draw(maze, rooms);

        return rooms;
    }

    private void Draw(Maze2D maze, List<Room> rooms) {
        var buffer = new AsciiBuffer(Math.Max(maze.XHeightRows + 2, rooms.Max(room => room.Position.X + room.Size.X)), Math.Max(maze.YWidthColumns + 2, rooms.Max(room => room.Position.Y + room.Size.Y)), true);
        DrawRect(buffer, new Vector(0, 0), maze.Size, mazeChars);
        rooms.ForEach(room => DrawRect(buffer, room.Position + 1, room.Size, roomChars));
        _log.Buffered.D(buffer);
    }

    private void DrawRect(AsciiBuffer buffer, Vector pos, Vector size, char[] wallChars) {
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

    private VectorD GetMapForce(Room room, Room env) {
        VectorD f = VectorD.Zero2D;

        var north = 0D;
        if (room.HighX > env.HighX) {
            // the box is on the edge of the field or above the field
            north = env.HighX - room.HighX - room.Size.Average;
            _log.Buffered.D($"north force 1: {north}");
        } else if (room.HighX < env.LowX) {
            // the box is off the field on its other side
            north = 0;
            _log.Buffered.D($"north force 2: {north}");
        } else {
            // the box is in the field (maybe touching its either side)
            north = (env.HighX - room.HighX) * (env.HighX - room.HighX);
            north = north == 0 ? 1 : north;
            north = -room.Size.Average / north;
            // 2 * Math.Sqrt(10) / ((13 - 12) * (13 - 12))
            _log.Buffered.D($"north force 3: {north}");
        }

        var south = 0D;
        if (room.LowX < env.LowX) {
            // the box is on the edge of the field or below the field
            south = env.LowX - room.LowX + room.Size.Average;
            _log.Buffered.D($"south force 1: {south}");
        } else if (room.LowX >= env.HighX) {
            // the box is off the field on its other side
            south = 0;
            _log.Buffered.D($"south force 2: {south}");
        } else {
            // the box is in the field (maybe touching its either side)
            south = (room.LowX - env.LowX) * (room.LowX - env.LowX);
            south = south == 0 ? 1 : south;
            south = room.Size.Average / south;
            _log.Buffered.D($"south force 3: {south}");
        }

        var east = 0D;
        if (room.HighY > env.HighY) {
            // the box is on the edge of the field or above the field
            east = env.HighY - room.HighY - room.Size.Average;
            _log.Buffered.D($"east force 1: {east} ({east} = {room.HighY} - {env.HighY})");
        } else if (room.HighY < env.LowY) {
            // the box is off the field on its other side
            east = 0;
            _log.Buffered.D($"east force 2: {east}");
        } else {
            // the box is in the field (maybe touching its either side)
            east = (env.HighY - room.HighY) * (env.HighY - room.HighY);
            east = east == 0 ? 1 : east;
            east = -room.Size.Average / east;
            _log.Buffered.D($"east force 3: {east}");
        }

        var west = 0D;
        if (room.LowY < env.LowY) {
            // the box is on the edge of the field or below the field
            west = env.LowY - room.LowY + room.Size.Average;
            _log.Buffered.D($"west force 1: {west}");
        } else if (room.LowY >= env.HighY) {
            // the box is off the field on its other side
            west = 0;
            _log.Buffered.D($"west force 2: {west}");
        } else {
            // the box is in the field (maybe touching its either side)
            west = (room.LowY - env.LowY) * (room.LowY - env.LowY);
            west = west == 0 ? 1 : west;
            west = room.Size.Average / west;
            _log.Buffered.D($"west force 3: {west}");
        }

        var nForce = new VectorD(new double[] { north, 0 });
        var sForce = new VectorD(new double[] { south, 0 });
        var eForce = new VectorD(new double[] { 0, east });
        var wForce = new VectorD(new double[] { 0, west });
        var force = nForce + sForce + eForce + wForce;
        _log.Buffered.D($"{room} is receiving force {force} ({nForce},{sForce},{eForce},{wForce}) from the environment");
        return force;
    }
    private VectorD GetOtherRoomsForce(Room thisRoom, Vector environment, List<Room> rooms) {
        var allForces = rooms.Where(otherRoom => otherRoom != thisRoom)
            .Select(otherRoom =>
                (thisRoom.Center - otherRoom.Center)
                .WithMagnitude(Math.Abs(
                    (thisRoom.Size.Average + otherRoom.Size.Average) /
                    (2 * (thisRoom.Center - otherRoom.Center).Magnitude * (thisRoom.Center - otherRoom.Center).Magnitude)
                )));
        var force = allForces
            .Aggregate(VectorD.Zero2D, (acc, f) => acc + f);
        _log.Buffered.D($"{thisRoom} is receiving force {force} (" + String.Join(", ", allForces) + ") from all other rooms");
        return force;
    }

    public class Room {
        public Vector Position { get; set; }
        public Vector Size { get; private set; }

        public VectorD Center => Position + Size / 2D;

        public int LowX => Position.X;
        public int HighX => Position.X + Size.X;
        public int LowY => Position.Y;
        public int HighY => Position.Y + Size.Y;

        public Room(Vector position, Vector size) {
            Position = position;
            Size = size;
        }

        public override string ToString() {
            return $"P{Position};S{Size}";
        }

        internal static Room Parse(string s) {
            var parameters = s.Trim(' ').Split(';').Select(x => Vector.Parse(x.Trim('P', 'S'))).ToArray();
            return new Room(parameters[0], parameters[1]);
        }
    }
}