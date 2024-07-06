using System;
using PlayersWorlds.Maps.MapFilters;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Hunt-and-kill algorithm implementation.
    /// </summary>
    public class MazeAreaStyleConverter {
        public Area ConvertMazeBorderToBlock(
            Area mazeArea,
            Maze2DRendererOptions options = null) {
            if (mazeArea.X<Maze2DBuilder>() == null) {
                throw new InvalidOperationException(
                    "Can't convert non Block style maze Areas.");
            }
            var map = Maze2DRenderer.CreateMapForMaze(mazeArea, options);
            new Maze2DRenderer(mazeArea, options)
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail }, Cell.CellTag.MazeWall, options.WallCellSize))
                .With(new Map2DSmoothCorners(Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner, options.WallCellSize))
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner }, Cell.CellTag.MazeWall, options.WallCellSize))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeVoid }, true, Cell.CellTag.MazeWall, 5, 5))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeWall, Cell.CellTag.MazeWallCorner }, false, Cell.CellTag.MazeTrail, 3, 3))
                .Render(map);
            return map;
        }
    }
}