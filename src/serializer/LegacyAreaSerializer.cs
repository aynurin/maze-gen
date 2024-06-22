using System;
using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Areas.Evolving;

namespace PlayersWorlds.Maps.Serializer {
    [Obsolete]
    public class LegacyAreaSerializer {
        /// <summary>
        /// Parses a string representation of a Area as serialized by pre-v0.2.
        /// </summary>
        /// <remarks>
        /// Note the rounding that occurs in <see cref="VectorD.ToString()" />.
        /// </remarks>
        /// <param name="value">A <see cref="string" /> of the form
        /// "P{Position};S{Size};{Type}[;tags]".</param>
        /// <param name="isPositionFixed"><c>true</c> to indicate that this area
        /// shouldn't be repositioned,
        /// otherwise <c>false</c> (default)</param>
        /// <returns></returns>
        public static Area ParseV01AreaString(string value, bool isPositionFixed = false) {
            var parts = value.Split(';');
            var type = parts.Length > 2 ?
                (AreaType)Enum.Parse(typeof(AreaType), parts[2]) :
                AreaType.Maze;
            var size = VectorD.Parse(parts[1]).RoundToInt();
            var position = VectorD.Parse(parts[0]).RoundToInt();
            return new Area(position, size, isPositionFixed,
                            type,
                            /*childAreas=*/null,
                            parts.Skip(3).ToArray());
        }

        /// <summary>
        /// Parses a string into a <see cref="Area" />.
        /// </summary>
        /// <param name="value">A string of the form
        /// <c>Vector; cell:link,link,...; cell:link,link,...; ...</c>, where
        /// <c>Vector</c> is a string representation of a 2D
        /// <see cref="Vector" /> defining the size of the maze, <c>cell</c> is
        /// the index of the cell in the maze, and <c>link</c> is the index of 
        /// a cell linked to this cell.</param>
        /// <returns></returns>
        public static Area ParseV01MazeString(string value) {
            if (value.IndexOf('|') == -1) {
                // TODO: Migrate all serialization to the other format.
                var parts = value.Split(';', '\n');
                var size = new Vector(parts[0].Split('x').Select(int.Parse));
                var maze = Area.CreateMaze(size);
                for (var i = 1; i < parts.Length; i++) {
                    var part = parts[i].Split(':', ',').Select(int.Parse).ToArray();
                    for (var j = 1; j < part.Length; j++) {
                        maze[Vector.FromIndex(part[0], maze.Size)]
                            .HardLinks.Add(Vector.FromIndex(part[j], maze.Size));
                        maze[Vector.FromIndex(part[j], maze.Size)]
                            .HardLinks.Add(Vector.FromIndex(part[0], maze.Size));
                    }
                }
                return maze;
            } else {
                var linksAdded = new HashSet<string>();
                var parts = value.Split('|');
                var size = Vector.Parse(parts[0]);
                var maze = Area.CreateMaze(size);
                parts[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ForEach(
                    areaStr => maze.AddChildArea(ParseV01AreaString(areaStr)));
                maze.BakeChildAreas();
                parts[2].Split(
                    new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .ForEach(cellStr => {
                        var part = cellStr.Split(':', ' ')
                                          .Select(int.Parse).ToArray();
                        for (var j = 1; j < part.Length; j++) {
                            if (linksAdded.Contains($"{part[0]}|{part[j]}")) {
                                continue;
                            }
                            maze[Vector.FromIndex(part[0], maze.Size)]
                                .HardLinks.Add(Vector.FromIndex(part[j], maze.Size));
                            linksAdded.Add($"{part[0]}|{part[j]}");
                            maze[Vector.FromIndex(part[j], maze.Size)]
                                .HardLinks.Add(Vector.FromIndex(part[0], maze.Size));
                            linksAdded.Add($"{part[j]}|{part[0]}");
                        }
                    });
                return maze;
            }
        }
    }
}