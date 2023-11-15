using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Nour.Play;
using Nour.Play.Maze;

public class AreaDistributor {
    private Log _log;
    private bool _drawEachEpoch = false;
    private double _timeBoost = 0.1D;

    public AreaDistributor(Log log, bool drawEachEpoch = false) {
        _log = log;
        _drawEachEpoch = drawEachEpoch;
    }

    public List<Room> DistributePlacedRooms(Maze2D maze, List<Room> rooms, int maxEpochs) {
        // option 1: apply env forces, then room forces.
        //      room forces will push the boxes out
        // option 2: apply room forces, then env forces
        //      maybe a little better in terms of reducing the number of epochs
        // option 3: calc all forces, adjust to env boundaries
        //      difficult to calculate
        // option 4: when calculating room forces, take into account env boundaries
        //      seems a little artificial
        // option 5: calc all forces, but apply only partially, e.g. 10%...
        //      i like this one. this will let all forces adjust as epochs go.

        var env = new Room(VectorD.Zero2D, new VectorD(maze.Size));
        Draw(_log, maze, rooms);
        var forces = new List<VectorD>();

        if (maxEpochs < 0) maxEpochs = 100;
        for (int j = 0; j < maxEpochs; j++) {
            forces.Clear();

            foreach (var room in rooms) {
                var force = GetOtherRoomsForce(room, env, rooms);
                // opposing force
                force = force / 2;
                force += GetMapForce(room, env);
                _log.Buffered.D($"OverallForce({room}): {force}");
                // compensate opposing force
                forces.Add(force);
            }
            var averageMagnitudeSq = forces.Select(f => f.MagnitudeSq).Average();
            if (averageMagnitudeSq < 0.1) {
                _log.Buffered.D($"{j}: mag: {forces.Aggregate((acc, a) => acc + a).MagnitudeSq}, avg: {forces.Select(f => f.MagnitudeSq).Average()}");
                _log.D($"{j}: Average magnitude: {averageMagnitudeSq}");
                break;
            };
            for (int i = 0; i < rooms.Count; i++) {
                rooms[i].Position += (forces[i] * _timeBoost);
            }
            // for (int i = 0; i < rooms.Count; i++) {
            //     rooms[i].Position += BoostFromEdge(rooms[i], env);
            // }

            if (_drawEachEpoch) Draw(_log, maze, rooms);
        }

        Draw(_log, maze, rooms);

        return rooms;
    }

    public static void Draw(Log log, Maze2D maze, List<Room> rooms) {
        Vector bufferSize = new Vector(maze.XHeightRows * 2, maze.YWidthColumns * 2 * 2);
        var buffer = new AsciiBuffer(bufferSize.X, bufferSize.Y, true);
        var offset = new Vector(maze.Size.X / 2, maze.Size.Y / 2);
        DrawRect(buffer, new Vector(offset.X, offset.Y), maze.Size, mazeChars);
        // transpile room positions to reflect reversed X in Terminal
        rooms.ForEach(room => DrawRect(buffer,
            new Vector(
                maze.Size.X - room.Position.RoundToInt().X - room.Size.RoundToInt().X + offset.X,
                room.Position.RoundToInt().Y + offset.Y),
            room.Size.RoundToInt(),
            roomChars));
        log.Buffered.D(buffer.ToString());
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

    private VectorD BoostFromEdge(Room room, Room env) {
        var debugLog = new List<String>();

        // the box is on the edge of the field or below the field
        // send the box into the env, and add the force as if the box was there

        var north = room.HighX < env.HighX ? 0 : env.HighX - room.HighX;

        var south = room.LowX > env.LowX ? 0 : env.LowX - room.LowX;

        var east = room.HighY > env.HighY ? 0 : env.HighY - room.HighY;

        var west = room.LowY < env.LowY ? 0 : env.LowY - room.LowY;

        var force = new VectorD(new double[] { north + south, east + west });
        _log.Buffered.D($"BoostFromEdge({room},{env}): {force}");
        return force;
    }

    private VectorD GetMapForce(Room room, Room env) {
        // direction is always to the center of the room
        // the room touches env edge from the inside, force = 1/0.1
        // the room is somewhere inside the env, force = 1/distance
        // the room is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost
        var box = env.Size;
        var distanceCenterToCenter = room.Center - env.Center;
        var envCenterRay = CropWithBox(_log, distanceCenterToCenter, env.Size);
        var roomCenterRay = CropWithBox(_log, envCenterRay, room.Size);
        string xCase = "NONE";
        string yCase = "NONE";
        VectorD distance, force;
        double forceX, forceY;
        double distanceX, distanceY;
        if ((room.HighX < env.HighX - VectorD.MIN && room.LowX > env.LowX + VectorD.MIN) ||
            (room.HighX > env.HighX + VectorD.MIN && room.LowX < env.LowX - VectorD.MIN)) {
            xCase = "NORMAL";
            // the distance is the distance between env edge and room boundaries
            var distanceHigh = env.HighX - room.HighX;
            var distanceLow = room.LowX - env.LowX;
            distanceX = (distanceHigh - distanceLow) / 2;
            forceX = Math.Abs(distanceX) < 1 ? 0 : 2 / distanceX;
        } else if (Math.Abs(room.HighX - env.HighX) < VectorD.MIN) {
            // the room touches env edge from the inside, force = 1/0.1
            xCase = "COLLIDE_HIGH_X";
            distanceX = -0.1;
            forceX = 2 / distanceX;
        } else if (Math.Abs(room.LowX - env.LowX) < VectorD.MIN) {
            // the room touches env edge from the inside, force = 1/0.1
            xCase = "COLLIDE_LOW_X";
            distanceX = 0.1;
            forceX = 2 / distanceX;
        } else if (room.HighX > env.HighX) {
            // the room is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost
            xCase = "OVERLAP_HIGH_X";
            distanceX = env.HighX - room.HighX - 1;
            forceX = distanceX / _timeBoost;
        } else if (room.LowX < env.LowX) {
            // the room is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost
            xCase = "OVERLAP_HIGH_X";
            distanceX = env.LowX - room.LowX + 1;
            forceX = distanceX / _timeBoost;
        } else {
            throw new InvalidOperationException("Unknown X case: {room}, {env}");
        }
        if ((room.HighY < env.HighY - VectorD.MIN && room.LowY > env.LowY + VectorD.MIN) ||
            (room.HighY > env.HighY + VectorD.MIN && room.LowY < env.LowY - VectorD.MIN)) {
            yCase = "NORMAL";
            // the distance is the distance between env edge and room boundaries
            var distanceHigh = env.HighY - room.HighY; // 
            var distanceLow = room.LowY - env.LowY;
            distanceY = (distanceHigh - distanceLow) / 2;
            forceY = Math.Abs(distanceY) < 1 ? 0 : 2 / distanceY;
        } else if (Math.Abs(room.HighY - env.HighY) < VectorD.MIN) {
            // the room touches env edge from the inside, force = 1/0.1
            yCase = "COLLIDE_HIGH_Y";
            distanceY = -0.1;
            forceY = 2 / distanceY;
        } else if (Math.Abs(room.LowY - env.LowY) < VectorD.MIN) {
            // the room touches env edge from the inside, force = 1/0.1
            yCase = "COLLIDE_LOW_Y";
            distanceY = 0.1;
            forceY = 2 / distanceY;
        } else if (room.HighY > env.HighY) {
            // the room is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost
            yCase = "OVERLAP_HIGH_Y";
            distanceY = env.HighY - room.HighY - 1;
            forceY = distanceY / _timeBoost;
        } else if (room.LowY < env.LowY) {
            // the room is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost
            yCase = "OVERLAP_HIGH_Y";
            distanceY = env.LowY - room.LowY + 1;
            forceY = distanceY / _timeBoost;
        } else {
            throw new InvalidOperationException("Unknown Y case: {room}, {env}");
        }
        distance = new VectorD(distanceX, distanceY);
        force = new VectorD(forceX, forceY);

        // if (envCenterRay.MagnitudeSq > (distanceCenterToCenter + roomCenterRay).MagnitudeSq) {
        //     // the room is somewhere inside the env, force = 1/distance
        //     caseName = "NORMAL";
        //     // the distance is the distance between env edge and room boundaries
        //     // because we need to mimic the revolting force of rooms to balance
        //     distance = envCenterRay - distanceCenterToCenter - roomCenterRay;
        //     // force = (VectorD.Zero2D - distance).WithMagnitude(envCenterRay.Magnitude - distance.Magnitude);
        //     // distance = distanceCenterToCenter;
        //     // force = VectorD.Zero2D - (1 / distance);
        //     // the more the distance, the less the force
        //     force = (VectorD.Zero2D - distance).WithMagnitude(2 / distance.Magnitude);
        // } else if (VectorD.MIN > Math.Abs((envCenterRay - distanceCenterToCenter - roomCenterRay).MagnitudeSq)) {
        //     // the room touches env edge from the inside, force = 1/0.1
        //     caseName = "COLLIDE";
        //     distance = new VectorD(distanceCenterToCenter.Value.Select(v => v > 0.1 ? 0.1 : v < -0.1 ? -0.1 : v));
        //     force = (VectorD.Zero2D - distance).WithMagnitude(2 / distance.Magnitude);
        // } else {
        //     // the room is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost
        //     caseName = "OVERLAP";
        //     distance = distanceCenterToCenter - envCenterRay + roomCenterRay;
        //     force = VectorD.Zero2D - distance.Inc(1) / _timeBoost;
        // }
        _log.Buffered.D($"GetMapForce ({room}, {env.Center}): {xCase},{yCase},envCenterRay={envCenterRay},distanceCenterToCenter={distanceCenterToCenter},roomCenterRay={roomCenterRay},distance={distance},force={force}");
        // var envCenterDirection = envCenterRay.UnitVector;
        // var reverseDirection = VectorD.Zero2D - envCenterDirection;
        // var centerToRemoteEdge = distanceCenterToCenter + roomCenterRay;
        // var distanceEdgeToEdge = envCenterRay - centerToRemoteEdge;
        // // ? Are we out of bounds?
        // var force = new double[2];
        // var outOfBounds = new bool[2];
        // for (int i = 0; i < distanceEdgeToEdge.Value.Length; i++) {
        //     outOfBounds[i] = room.Position.Value[i] < env.Position.Value[i] ||
        //         room.Position.Value[i] + room.Size.Value[i] > env.Position.Value[i] + env.Size.Value[i];
        //     var distance = distanceEdgeToEdge.Value[i];
        //     // what if distanceEdgeToEdge == 0.00x0.00? i.e., the box is touching the edge?
        //     if (distanceEdgeToEdge.MagnitudeSq < Double.Epsilon) {
        //         // push it one towards the center
        //         distance = envCenterDirection.Value[i] / _timeBoost;
        //     }
        //     // keep the distance at least 0.1
        //     if (distance < 0 && distance > -0.01) distance = -0.1;
        //     if (distance > 0 && distance < 0.01) distance = 0.1;
        //     if (Math.Abs(distance) < 1 || outOfBounds[i]) {
        //         // each step we only take .1 of force, but if we are out of bounds we want to jump immediately
        //         distance /= _timeBoost;
        //     }
        //     if (!outOfBounds[i]) {
        //         distance = -distance;
        //     }
        //     force[i] = distance;
        // }
        // var vForce = new VectorD(force);
        // _log.Buffered.D($"GetMapForce ({room}, {env.Center}): {String.Join(",", outOfBounds)}, {envCenterRay}, {distanceCenterToCenter}, {roomCenterRay}, {distanceEdgeToEdge}, {vForce}");
        return force;
    }

    private bool SignsReversed(VectorD one, VectorD another) {
        return SignsReversed(one.X, one.X) && SignsReversed(one.Y, another.Y);
    }

    private bool SignsReversed(double one, double another) {
        return one >= 0 && another < 0 || one < 0 && another >= 0;
    }

    private VectorD GetOtherRoomsForce(Room room, Room env, List<Room> allRooms) =>
        allRooms.Where(other => other != room)
                .Select(other => GetRoomForce(room, other, allRooms))
                .Aggregate(VectorD.Zero2D, (acc, f) => acc + f);

    private VectorD GetRoomForce(Room room, Room other, List<Room> allRooms) {
        // the rooms are touching each other, force = 1/0.1
        // the rooms are at distance from each other, force = 1/distance
        // the rooms overlap, boost the rooms away from each other, force = (distance + 1) / timeBoost
        // !! we can't determine if the rooms overlap or touch each other using 
        // !! just a center-to-center vector. E.g., imagine this:
        // !!            ┌─┐
        // !!            │B│
        // !!            │ │
        // !!            │*│
        // !!            │ │
        // !! ┌──────────┼─┼┐
        // !! │    A *   └─┘│
        // !! └─────────────┘
        // !! The A(center)->B(center) vector will have a gap in the center and
        // !! it won't show that the boxes overlap.
        // !! In case of an overlap, we will take a vector from the center of
        // !! this box to the center of the overlap.
        // Calculate the coordinates of the overlapping rectangle.
        if (room.Overlaps(other) && !room.Contains(other.Center) && !other.Contains(room.Center)) {
            var overlapHighX = Math.Min(room.HighX, other.HighX);
            var overlapHighY = Math.Min(room.HighY, other.HighY);
            var overlapLowX = Math.Max(room.LowX, other.LowX);
            var overlapLowY = Math.Max(room.LowY, other.LowY);
            other = new Room(
                new VectorD(overlapLowX, overlapLowY),
                new VectorD(
                    overlapHighX - overlapLowX,
                    overlapHighY - overlapLowY)
            );
            // var slopeHighX = room.HighX > other.HighX ? room.HighX - other.HighX : 0;
            // var slopeLowX = room.LowX < other.LowX ? room.LowX - other.LowX : 0;
            // var slopeHighY = room.HighY > other.HighY ? room.HighY - other.HighY : 0;
            // var slopeLowY = room.LowY < other.LowY ? room.LowY - other.LowY : 0;
            // var slopeX = slopeHighX - slopeLowX;
            // var slopeY = slopeHighY - slopeLowY;
            // overlapSlope = new VectorD(slopeX, slopeY);
            // _log.Buffered.D($"GetRoomForce({room}, {other}): overlap: {overlapArea}, {overlapSlope}");
        }
        // TODO: switch signs?
        var direction = room.Center - other.Center;
        if (direction.IsZero()) {
            // Room centers match
            if (!room.OpposingForce.IsZero()) {
                direction = VectorD.Zero2D - room.OpposingForce;
            } else {
                while (direction.IsZero()) {
                    direction = new VectorD(
                        GlobalRandom.Next(-1, 2) / 10D,
                        GlobalRandom.Next(-1, 2) / 10D
                    );
                }
                other.OpposingForce = direction;
            }
        }
        var thisV = CropWithBox(_log, direction, room.Size);
        var otherV = VectorD.Zero2D - CropWithBox(_log, VectorD.Zero2D - direction, other.Size);
        string caseName = "NONE";
        VectorD force, distance;
        if (VectorD.MIN > Math.Abs((thisV + otherV).MagnitudeSq - direction.MagnitudeSq)) {
            // the rooms are touching each other, force = 1/0.1
            distance = new VectorD(direction.Value.Select(v => v > 0.1 ? 0.1 : v < -0.1 ? -0.1 : v));
            force = distance.WithMagnitude(distance.MagnitudeSq < 1 ? 1 : (2 / distance.Magnitude));
            caseName = "COLLIDE";
        } else if (thisV.MagnitudeSq + otherV.MagnitudeSq < direction.MagnitudeSq) {
            // the rooms are at distance from each other, force = 1/distance
            distance = direction - thisV - otherV;
            force = distance.WithMagnitude(distance.MagnitudeSq < 1 ? 1 : (2 / distance.Magnitude));
            caseName = "NORMAL";
        } else {
            // rooms overlap
            distance = thisV - direction + otherV;
            // boost the rooms away from each other, force = (distance + 1) / timeBoost
            force = (distance.Inc(1)) / _timeBoost;
            caseName = "OVERLAP";
        }
        _log.Buffered.D($"GetRoomForce({room}, {other}): {caseName},thisV={thisV},otherV={otherV},direction={direction},distance={distance},distance.Magnitude={distance.Magnitude},force={force}");
        return force;
    }

    // TODO: swap x and y
    // TODO: change VectorD to Single

    // private VectorD GetMapForceBak(Room room, Room env) {
    //     var debugLog = new List<String>();

    //     // direction is always to the center of the room
    //     // force is a balance of distances to the edges

    //     var north = (double)(room.HighX - env.HighX);
    //     if (north > 0) {
    //         // the box is on the edge of the field or above the field
    //         // send the box into the env, and add the force as if the box was there
    //         north = -north - evalForce(env.Size, room.Size.Average, 0);
    //         debugLog.Add($"n1:{north:F2}");
    //     } else if (room.LowX > env.LowX) {
    //         // the box is in the field (maybe touching its either side)
    //         north = evalForce(env.Size, room.Size.Average, north);
    //         debugLog.Add($"n3:{north:F2}");
    //     } else {
    //         // the box is off the field on its other side
    //         north = 0D;
    //     }

    //     var south = (double)(room.LowX - env.LowX);
    //     if (south < 0) {
    //         // the box is on the edge of the field or below the field
    //         // send the box into the env, and add the force as if the box was there
    //         south = -south + evalForce(env.Size, room.Size.Average, 0);
    //         debugLog.Add($"s1:{south:F2}");
    //     } else if (room.HighX < env.HighX) {
    //         // the box is in the field (maybe touching its either side)
    //         south = evalForce(env.Size, room.Size.Average, south);
    //         debugLog.Add($"s3:{south:F2}");
    //     } else {
    //         // the box is off the field on its other side
    //         south = 0D;
    //     }

    //     var east = (double)(room.HighY - env.HighY);
    //     if (east > 0) {
    //         // the box is on the edge of the field or above the field
    //         // send the box into the env, and add the force as if the box was there
    //         east = -east - evalForce(env.Size, room.Size.Average, 0);
    //         debugLog.Add($"e1:{east:F2}");
    //     } else if (room.LowY > env.LowY) {
    //         // the box is in the field (maybe touching its either side)
    //         east = evalForce(env.Size, room.Size.Average, east);
    //         debugLog.Add($"e3:{east:F2}");
    //     } else {
    //         // the box is off the field on its other side
    //         east = 0D;
    //     }

    //     var west = (double)(room.LowY - env.LowY);
    //     if (west < 0) {
    //         // the box is on the edge of the field or below the field
    //         // send the box into the env, and add the force as if the box was there
    //         west = -west + evalForce(env.Size, room.Size.Average, 0);
    //         debugLog.Add($"w1:{west:F2}");
    //     } else if (room.HighY < env.HighY) {
    //         // the box is in the field (maybe touching its either side)
    //         west = evalForce(env.Size, room.Size.Average, west);
    //         debugLog.Add($"w3:{west:F2}");
    //     } else {
    //         // the box is off the field on its other side
    //         west = 0D;
    //     }

    //     var force = new VectorD(new double[] { north + south, east + west });
    //     _log.Buffered.D($"{room} environment: {force} ({String.Join(",", debugLog)})");
    //     return force;
    // }

    // // TODO: Measure distance between rooms, not their centers
    // // TODO: Environment can't compensate a force, it must collide and block the exit, causing the force vector to change.
    // private VectorD GetOtherRoomsForce(Room thisRoom, VectorD environment, List<Room> rooms) {
    //     var allForces = rooms.Where(otherRoom => otherRoom != thisRoom)
    //         .Select(otherRoom =>
    //             (thisRoom.Center - otherRoom.Center)
    //             .WithMagnitude(Math.Abs(
    //                 evalForce(environment, (thisRoom.Size.Average + otherRoom.Size.Average) / 2D,
    //                     Distance(_log, thisRoom, otherRoom))
    //             )));
    //     var force = allForces
    //         .Aggregate(VectorD.Zero2D, (acc, f) => acc + f);
    //     var debugString = String.Join(",", rooms
    //         .Where(otherRoom => otherRoom != thisRoom)
    //         .Zip(allForces, (otherRoom, f) => $"{otherRoom}({Distance(_log, thisRoom, otherRoom):F2}):{f:F2}"));
    //     _log.Buffered.D($"{thisRoom} rooms: {force:F2} ({debugString})");
    //     return force;
    // }

    /// <summary>
    /// Crops the given vector using the given box
    /// </summary>
    /// <param name="vector">The vector to crop will be treated as going out of the center of the box</param>
    /// <param name="box">The box to use to crop the vector</param>
    /// <returns></returns>
    public static VectorD CropWithBox(Log log, VectorD vector, VectorD box) {
        // TODO: I still have X and Y all messed up.
        double x, y;
        var rt = box / 2;
        var rb = new VectorD(-box.X / 2, box.Y / 2);
        var lb = VectorD.Zero2D - box / 2;
        var lt = new VectorD(box.X / 2, -box.Y / 2);
        var alphaT = Math.Atan2(rt.X, rt.Y);
        var alphaL = Math.Atan2(lt.X, lt.Y);
        var alphaB = Math.Atan2(lb.X, lb.Y);
        var alphaR = Math.Atan2(rb.X, rb.Y);
        var alpha = Math.Atan2(vector.X, vector.Y);
        string dir;
        if (alpha >= alphaT && alpha <= alphaL) {
            // v crosses the top edge
            dir = "top";
            x = box.X / 2;
            y = x * Math.Tan(Math.PI / 2 - alpha);
        } else if (alpha > alphaL || alpha < alphaB) {
            dir = "left";
            // v crosses the left edge
            y = -box.Y / 2;
            x = y * Math.Tan(Math.PI + alpha);
        } else if (alpha <= alphaR && alpha >= alphaB) {
            dir = "bottom";
            // v crosses the bottom edge
            x = -box.X / 2;
            y = x * Math.Tan(Math.PI / 2 - alpha);
        } else {
            dir = "right";
            // v crosses the right edge
            y = box.Y / 2;
            x = y * Math.Tan(alpha);
        }
        var v = new VectorD(x, y);
        // log.Buffered.D($"mag: {vector.Magnitude:F2}, dir: {dir}, alpha: {alpha}({alpha * 180D / Math.PI:F2}), part: {v},{v.Magnitude:F2}");
        return v;
    }

    /// <summary>
    /// Same as CropWithBox, but returns the magnitude instead of the actual vector.
    /// </summary>
    /// <param name="vector">The vector to crop will be treated as going out of the center of the box</param>
    /// <param name="box">The box to use to crop the vector</param>
    /// <returns></returns>
    public static double CroppedMagnitude(Log log, VectorD vector, VectorD box) {
        var rt = box / 2;
        var rb = new VectorD(-box.X / 2, box.Y / 2);
        var lb = VectorD.Zero2D - box / 2;
        var lt = new VectorD(box.X / 2, -box.Y / 2);
        var alphaT = Math.Atan2(rt.X, rt.Y);
        var alphaL = Math.Atan2(lt.X, lt.Y);
        var alphaB = Math.Atan2(lb.X, lb.Y);
        var alphaR = Math.Atan2(rb.X, rb.Y);
        // TODO: I still have X and Y all messed up.
        var a = Math.Atan2(vector.X, vector.Y);
        double croppedMag = Double.NaN;
        if (a >= alphaT && a <= alphaL) {
            // v crosses thisRoom.HighX
            croppedMag = (box.X / 2) / Math.Cos(Math.PI / 2 - a);
        } else if (a > alphaL || a < alphaB) {
            // v crosses thisRoom.LowY
            croppedMag = (box.Y / 2) / Math.Cos(Math.PI - a);
        } else if (a <= alphaR && a >= alphaB) {
            // v crosses thisRoom.LowX
            croppedMag = (box.X / 2) / Math.Cos(Math.PI / 2 - a);
        } else {
            // v crosses thisRoom.HighY
            croppedMag = (box.Y / 2) / Math.Cos(a);
        }
        return croppedMag;
    }

    public static double Distance(Log log, Room thisRoom, Room otherRoom) {
        var direction = otherRoom.Center - thisRoom.Center;
        var thisVMag = CroppedMagnitude(log, direction, thisRoom.Size);
        var thisV = CropWithBox(log, direction, thisRoom.Size);
        var otherVMag = CroppedMagnitude(log, VectorD.Zero2D - direction, otherRoom.Size);
        var otherV = CropWithBox(log, VectorD.Zero2D - direction, otherRoom.Size);
        // log.D($"mag: {direction.Magnitude:F2}, A part: {thisV},{thisVMag:F2}(d={thisV.Magnitude - Math.Abs(thisVMag):F2}), B part: {otherV},{otherVMag:F2}(d={otherV.Magnitude - Math.Abs(otherVMag):F2}), distance = {direction.Magnitude - Math.Abs(thisVMag) - Math.Abs(otherVMag):F2}");
        return direction.Magnitude - Math.Abs(thisVMag) - Math.Abs(otherVMag);
    }

    // private double evalForce(VectorD envSize, VectorD forceDirection, double thisMass, double otherMass, double distance) {
    //     if (distance >= 0 && distance < 1) {
    //         distance = 1;
    //     }
    //     if (distance <= 0 && distance > -1) {
    //         distance = -1;
    //     }
    //     // var force = Math.Pow(7 * mass / Math.Abs(distance), 0.25);
    //     // var force = -.3 + 1D / Math.Sqrt(Math.Abs(distance) + 0.05);
    //     var force = -.5 * Math.Abs(distance) + 5;
    //     if (force < 0) {
    //         force = 0;
    //     }
    //     if (distance < 0) force = -force;
    //     if (Double.IsNaN(force) || Double.IsInfinity(force) || true) {
    //         _log.Buffered.D($"evalForce({mass:F2},{distance:F2}) = {force:F2}");
    //     }
    //     return force;
    // }

    // TODO: make a struct
    public class Room {
        public VectorD Position { get; set; }
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
            var parameters = s.Trim(' ').Split(';').Select(x => VectorD.Parse(x.Trim('P', 'S'))).ToArray();
            return new Room(parameters[0], parameters[1]);
        }

        internal bool Overlaps(Room other) {
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
    }
}