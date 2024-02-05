using System;
using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps.Areas;
using static PlayersWorlds.Maps.Maze.GeneratorOptions;

namespace PlayersWorlds.Maps.Maze {

    /// <summary>
    /// <p>This is a helper class that allows maze generators to manage maze
    /// cells during generation.</p>
    /// <p>It is not intended to be used by the user.</p>
    /// <p>The main point is that we can't pick just random cells during the 
    /// generation process because we can have different types of areas in the
    /// maze:</p>
    /// <ul>
    /// <li> <see cref="AreaType.Fill" /> cells are not be included in the maze.
    /// They will be avoided by maze generators in all cases.
    /// </li>
    /// <li> <see cref="AreaType.Hall" /> cells are included but should have
    /// only one entrance cell. They will be avoided by generators and connected
    /// later.</li>
    /// <li> <see cref="AreaType.Cave" /> cells are included with any number of
    /// entrances. They will be processed by the generators as regular maze
    /// areas with all internal walls removed afterwards.</li>
    /// </ul>
    /// </summary>
    // TODO: What's a better name for this class?
    internal class Maze2DBuilder {
        private readonly Maze2D _maze;
        private readonly GeneratorOptions _options;
        private readonly HashSet<MazeCell> _cellsToConnect;
        private readonly HashSet<MazeCell> _allConnectableCells;
        private readonly HashSet<MazeCell> _connectedCells =
            new HashSet<MazeCell>();

        // choose random cells from this list
        // there can be several areas associated with the same cell
        private readonly Dictionary<MazeCell, List<MazeCell>> _priorityCells;

        public HashSet<MazeCell> CellsToConnect => _cellsToConnect;
        public HashSet<MazeCell> ConnectedCells => _connectedCells;

        public Dictionary<MazeCell, List<MazeCell>> PriorityCells => _priorityCells;

        // a reverse map to cleanup _priorityCells

        public Maze2DBuilder(Maze2D maze, GeneratorOptions options) {
            _maze = maze;
            _options = options;

            // Find priority cells to connect first. _priorityCells are
            // associated with areas they relate to. When any of the given area
            // cells is processed, all "priority cells" of this area are removed
            // from the priority list (but stay in the regular pool so they can
            // be processed as normal.
            _priorityCells = new Dictionary<MazeCell, List<MazeCell>>();
            foreach (var areaInfo in _maze.MapAreas) {
                var area = areaInfo.Key;
                var areaCells = areaInfo.Value.ToList();
                if (area.Type == AreaType.Cave) {
                    // make sure all cave areas are linked.
                    areaCells.ForEach(c => _priorityCells.Set(c, areaCells));
                } else if (area.Type == AreaType.Hall) {
                    // halls will be connected later. BUT we need to make sure
                    // halls have at least one neighbor cell connected to the
                    // maze.
                    var cells = WalkInCells(area).ToList();
                    cells.ForEach(c => _priorityCells.Set(c, cells));
                }
            }

            // fill the unlinked cells skipping the halls and filled areas.
            _cellsToConnect = new HashSet<MazeCell>(_maze.AllCells
                .Where(c => c.MapAreas
                       .All(cellArea => cellArea.Key.Type != AreaType.Fill &&
                                        cellArea.Key.Type != AreaType.Hall)));
            _allConnectableCells = new HashSet<MazeCell>(_maze.AllCells
                .Where(c => c.MapAreas
                       .All(cellArea => cellArea.Key.Type != AreaType.Fill &&
                                        cellArea.Key.Type != AreaType.Hall)));
        }

        private IEnumerable<MazeCell> WalkInCells(
            MapArea area) {
            if (area.Type == AreaType.Hall) {
                // find all cells next to this hall that can be linked to the
                // hall.
                return _maze.AllCells
                    .IterateIntersection(
                        new Vector(area.Position.Value.Select(c => c - 1)),
                        new Vector(area.Size.Value.Select(c => c + 2)))
                    .Where(c => !c.xy.IsIn(area.Position, area.Size) &&
                                 c.cell.Neighbors().Any(
                                    n => n.MapAreas.ContainsKey(area)))
                    .Select(c => c.cell);
            }
            throw new InvalidOperationException(
                $"WalkInCells is applicable only to halls " +
                $"({Enum.GetName(typeof(AreaType), area.Type)} requested).");
        }

        public MazeCell PickRandomCellToLink() {
            // pick the next cell for the maze generator.
            // skip halls and filled areas.
            // prioritize cave areas over other cells to make sure they are
            // connected.
            // also make sure hall areas have at least one neighbor cell
            // connected to the maze so we can connect them later.
            if (_priorityCells.Count > 0) {
                return _priorityCells.Keys.GetRandom(_priorityCells.Count);
            } else {
                return _cellsToConnect.GetRandom(_cellsToConnect.Count);
            }
        }

        public Optional<MazeCell> PickRandomNeighborToLink(MazeCell cell) {
            // pick the next cell for the maze generator.
            // skip halls and filled areas.
            // prioritize cave areas over other cells to make sure they are
            // connected.
            // also make sure hall areas have at least one neighbor cell
            // connected to the maze so we can connect them later.
            var neighbors = cell.Neighbors();
            var cellsToConnect = _priorityCells.GetAll(neighbors)
                .Select(kv => kv.Item1).ToList();
            if (cellsToConnect.Count == 0) {
                cellsToConnect = _allConnectableCells.GetAll(neighbors).ToList();
            }
            return cellsToConnect.Count > 0 ?
                new Optional<MazeCell>(cellsToConnect.GetRandom()) :
                Optional<MazeCell>.Empty;
        }

        public void ConnectHalls() {
            // calls were avoided during the maze generation.
            // now is the time to see if there are maze corridors next
            // to any halls, and if there are, connect them.
            foreach (var areaInfo in _maze.MapAreas) {
                var area = areaInfo.Key;
                if (area.Type == AreaType.Hall) {
                    var walkway = WalkInCells(area)
                        .Where(c => _connectedCells.Contains(c)).GetRandom();
                    var entrance = walkway.Neighbors()
                        .First(c => c.MapAreas.ContainsKey(areaInfo.Key));
                    entrance.Link(walkway);
                }
            }
        }

        public void MarkConnected(MazeCell cell) {
            // mark the cell as visited so it's not picked again in the 
            // PickNextRandomUnlinkedCell
            if (_priorityCells.ContainsKey(cell)) {
                var cellsToRemove = _priorityCells[cell];
                cellsToRemove.ForEach(c => _priorityCells.Remove(c));
            } else {
                _priorityCells.Remove(cell);
            }
            // only the connected cell is removed from _cellsToConnect because
            // we still might want to connect other cells of the related areas.
            _cellsToConnect.Remove(cell);
            _connectedCells.Add(cell);
        }

        public IEnumerable<MazeCell> IterateUnlinkedCells() {
            // iterate the unlinked cells skipping the filled areas.
            // lowest xy to highest xy with row priority.
            return _cellsToConnect;
        }

        public bool IsVisited(MazeCell cell) {
            return _connectedCells.Contains(cell);
        }

        public bool IsFillComplete() {
            if (_cellsToConnect.Count == 0) {
                return true;
            }
            if (_connectedCells.Count == 0) {
                return false;
            }
            switch (_options.FillFactor) {
                case FillFactorOption.FullWidth: {
                        var minX = _connectedCells.Min(c => c.X);
                        var maxX = _connectedCells.Max(c => c.X);
                        return minX == 0 && maxX == _maze.Size.X - 1;
                    }

                case FillFactorOption.FullHeight: {
                        var minY = _connectedCells.Min(c => c.Y);
                        var maxY = _connectedCells.Max(c => c.Y);
                        return minY == 0 && maxY == _maze.Size.Y - 1;
                    }

                case FillFactorOption.Full:
                    return _cellsToConnect.Count == 0;

                default: {
                        var fillFactor =
                            _options.FillFactor ==
                                FillFactorOption.Quarter ? 0.25 :
                            _options.FillFactor ==
                                FillFactorOption.Half ? 0.5 :
                            _options.FillFactor ==
                                FillFactorOption.ThreeQuarters ? 0.75 :
                            0.9;
                        return _cellsToConnect.Count <=
                            _allConnectableCells.Count * (1 - fillFactor);
                    }
            }
        }
    }
}