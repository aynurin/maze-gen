using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze.PostProcessing;
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
    public class Maze2DBuilder {
        private readonly Maze2D _maze;
        private readonly GeneratorOptions _options;

        private readonly int _isFillCompleteAttempts;
        private int _isFillCompleteAttemptsMade;

        private readonly HashSet<MazeCell> _cellsToConnect;
        private readonly Dictionary<MazeCell, List<MazeCell>> _priorityCellsToConnect;
        private readonly HashSet<MazeCell> _allConnectableCells;
        private readonly HashSet<MazeCell> _connectedCells =
            new HashSet<MazeCell>();
        private readonly List<HashSet<MazeCell>> _cellGroups;

        /// <summary>
        /// Cells that can be connected and are not connected yet.
        /// </summary>
        /// <remarks>
        /// Used only in <see cref="AldousBroderMazeGenerator"/>.
        /// </remarks>
        internal IReadOnlyCollection<MazeCell> TestCellsToConnect =>
            _cellsToConnect;
        /// <summary>
        /// All connectable cells (connected or unconnected) in the
        /// lowest xy to highest xy order with row priority.
        /// </summary>
        /// <remarks>
        /// Used only in <see cref="BinaryTreeMazeGenerator"/> and 
        /// <see cref="SidewinderMazeGenerator"/>.
        /// </remarks>
        internal IReadOnlyCollection<MazeCell> AllCells => _allConnectableCells;
        /// <summary>
        /// Cells that are already have connections to other cells.
        /// </summary>
        /// <remarks>
        /// Used only in tests.
        /// </remarks>
        /// <testonly />
        internal IReadOnlyCollection<MazeCell> TestConnectedCells =>
            _connectedCells;
        /// <summary>
        /// Cells that are not yet connected, and we want to connect them first.
        /// <p>Each cell in the priority cells is associated with an area, and
        /// when any of that area cells is connected, all cells associated with
        /// that area lose their priority status.</p>
        /// </summary>
        /// <remarks>
        /// Used only in tests.
        /// </remarks>
        /// <testonly />
        internal Dictionary<MazeCell, List<MazeCell>> TestPriorityCells =>
            _priorityCellsToConnect;

        /// <summary>
        /// Contains groups of connectable cells. In case the maze field has
        /// isolated areas, some groups of cells can be isolated from others.
        /// This is the list of groups of connectable cells.
        /// </summary>
        public virtual List<HashSet<MazeCell>> CellGroups => _cellGroups;

        internal RandomSource Random { get => _options.RandomSource; }

        /// <summary>
        /// Creates a new instance of the <see cref="Maze2DBuilder"/> class.
        /// </summary>
        /// <param name="maze"><see cref="Maze2D" /> to build.</param>
        /// <param name="options"><see cref="GeneratorOptions" />.</param>
        public Maze2DBuilder(Maze2D maze, GeneratorOptions options) {
            _maze = maze;
            _options = options;

            // Find priority cells to connect first. _priorityCellsToConnect are
            // associated with areas they relate to. When any of the given area
            // cells is processed, all "priority cells" of this area are removed
            // from the priority list (but stay in the regular pool so they can
            // be processed as normal.
            _priorityCellsToConnect = new Dictionary<MazeCell, List<MazeCell>>();
            foreach (var areaInfo in _maze.MapAreas) {
                var area = areaInfo.Key;
                var areaCells = areaInfo.Value.ToList();
                if (area.Type == AreaType.Cave) {
                    // make sure all cave areas are linked.
                    areaCells.ForEach(c => _priorityCellsToConnect.Set(c, areaCells));
                } else if (area.Type == AreaType.Hall) {
                    // halls will be connected later. BUT we need to make sure
                    // halls have at least one neighbor cell connected to the
                    // maze.
                    var cells = WalkInCells(area)
                        // make sure we don't include cells that belong to 
                        // any other area.
                        .Except(_maze.MapAreas
                            .Where(otherArea =>
                                area != otherArea.Key &&
                                (otherArea.Key.Type == AreaType.Hall ||
                                    otherArea.Key.Type == AreaType.Fill))
                            .SelectMany(otherArea => otherArea.Value))
                        .ToList();
                    cells.ForEach(c => _priorityCellsToConnect.Set(c, cells));
                }
            }

            // fill the unlinked cells skipping the halls and filled areas.
            _cellsToConnect = new HashSet<MazeCell>(_maze.Cells
                .Where(c => c.MapAreas
                       .All(cellArea => cellArea.Key.Type != AreaType.Fill &&
                                        cellArea.Key.Type != AreaType.Hall)));
            _allConnectableCells = new HashSet<MazeCell>(_maze.Cells
                .Where(c => c.MapAreas
                       .All(cellArea => cellArea.Key.Type != AreaType.Fill &&
                                        cellArea.Key.Type != AreaType.Hall)));

            _isFillCompleteAttempts = (int)Math.Pow(maze.Size.Area, 2);

            var buffer = new HashSet<MazeCell>(_maze.Cells
                .Where(c => c.MapAreas
                       .All(cellArea => cellArea.Key.Type != AreaType.Fill &&
                                        cellArea.Key.Type != AreaType.Hall)));
            _cellGroups = new List<HashSet<MazeCell>>();
            if (buffer.Count > 0) {
                do {
                    var distances = DijkstraDistance.FindRaw(
                        this, buffer.First());
                    _cellGroups.Add(new HashSet<MazeCell>(distances.Keys));
                    buffer.ExceptWith(distances.Keys);
                } while (buffer.Count > 0);
            }
        }

        private IEnumerable<MazeCell> WalkInCells(Area area) {
            if (area.Type == AreaType.Hall) {
                // find all cells next to this hall that can be linked to the
                // hall.
                return _maze.Cells
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

        /// <summary>
        /// Cells that can be connected and are not connected yet, in a
        /// priority order.
        /// </summary>
        /// <remarks>
        /// Used only in <see cref="HuntAndKillMazeGenerator"/>.
        /// </remarks>
        public IEnumerable<MazeCell> GetPrioritizedCellsToConnect() =>
            _priorityCellsToConnect.Keys
                    .Concat(_cellsToConnect
                            .Except(_priorityCellsToConnect.Keys));

        /// <summary>
        /// Randomly picks the next cell to be connected from a pool of
        /// available cells.
        /// </summary>
        public virtual MazeCell PickNextCellToLink() {
            // pick the next cell for the maze generator.
            // skip halls and filled areas.
            // prioritize cave areas over other cells to make sure they are
            // connected.
            // also make sure hall areas have at least one neighbor cell
            // connected to the maze so we can connect them later.
            MazeCell nextCell;
            if (_priorityCellsToConnect.Count > 0) {
                nextCell = _options.RandomSource.RandomOf(
                    _priorityCellsToConnect.Keys,
                    _priorityCellsToConnect.Count);
            } else {
                nextCell = _options.RandomSource.RandomOf(
                    _cellsToConnect,
                    _cellsToConnect.Count);
            }
            return nextCell;
        }

        /// <summary>
        /// Retrieves a random neighbor of the given cell, returning priority
        /// cells first.
        /// </summary>
        /// <remarks>
        /// This method does not filter by unconnected cells, i.e. returned
        /// neighbors may be already connected. Use the
        /// <see cref="TryPickRandomNeighbor(MazeCell, out MazeCell, bool, bool)"/>
        /// overload to filter.
        /// </remarks>
        /// <param name="cell">A neighbor of this cell will be returned.
        /// </param>
        /// <param name="neighbor">The returned neighbor.</param>
        /// <returns><c>true</c> if a neighbor was found, otherwise false.
        /// </returns>
        public bool TryPickRandomNeighbor(MazeCell cell,
                                          out MazeCell neighbor) =>
            TryPickRandomNeighbor(cell, out neighbor,
                onlyUnconnected: false, honorPriority: true);

        /// <summary>
        /// Retrieves a random neighbor of the given cell.
        /// </summary>
        /// <param name="cell">A neighbor of this cell will be returned.
        /// </param>
        /// <param name="neighbor">The returned neighbor.</param>
        /// <param name="honorPriority">Consider priority cells first.</param>
        /// <param name="onlyUnconnected">Only return unconnected neighbors.
        /// </param>
        /// <returns><c>true</c> if a neighbor was found, otherwise false.
        /// </returns>
        public virtual bool TryPickRandomNeighbor(MazeCell cell, out MazeCell neighbor,
                                          bool onlyUnconnected = false,
                                          bool honorPriority = true) {
            // pick the next cell for the maze generator.
            // skip halls and filled areas.
            // prioritize cave areas over other cells to make sure they are
            // connected.
            // also make sure hall areas have at least one neighbor cell
            // connected to the maze so we can connect them later.
            var neighbors = cell.Neighbors();
            var cellsToConnect = honorPriority ?
                _priorityCellsToConnect.GetAll(neighbors)
                    .Select(kv => kv.Item1).ToList() :
                    new List<MazeCell>();
            if (cellsToConnect.Count == 0) {
                cellsToConnect =
                    (onlyUnconnected ? _cellsToConnect : _allConnectableCells)
                    .GetAll(neighbors).ToList();
            }
            if (cellsToConnect.Count > 0) {
                neighbor = _options.RandomSource.RandomOf(cellsToConnect);
                return true;
            } else {
                neighbor = null;
                return false;
            }
        }

        /// <summary>
        /// Links all cells in <see cref="AreaType.Hall" /> and
        /// <see cref="AreaType.Cave" /> areas, and removes
        /// <see cref="AreaType.Fill" /> area cells from the neighbors and
        /// links. This should be called in MazeGenerator.Generate() after the
        /// generator algorithm completes.
        /// </summary>
        public void ApplyAreas() {
            // halls were avoided during the maze generation.
            // now is the time to see if there are maze corridors next
            // to any halls, and if there are, connect them.
            foreach (var areaInfo in _maze.MapAreas) {
                var area = areaInfo.Key;
                var areaCells = areaInfo.Value.ToList();
                if (area.Type == AreaType.Hall || area.Type == AreaType.Cave) {
                    // link all hall and cave inner cells together.
                    areaCells.ForEach(cell => cell.LinkAllNeighborsInArea(area));
                }
                if (area.Type == AreaType.Hall) {
                    // create hall entrances.
                    var walkInCells = WalkInCells(area).ToList();
                    var entranceExists =
                        walkInCells.SelectMany(cell => cell.Links())
                            .Any(linkedCell =>
                                 linkedCell.MapAreas.ContainsKey(area));
                    // entrance can already be created by an overlapping area.
                    if (entranceExists) continue;
                    var visitedWalkInCells = walkInCells
                        .Where(c => _connectedCells.Contains(c)).ToList();
                    if (visitedWalkInCells.Count == 0) {
                        Trace.TraceWarning(
                            "Hall {0} has no visited entrance cells",
                            areaInfo.Key
                        );
                        continue;
                    }
                    var walkway = _options.RandomSource.RandomOf(visitedWalkInCells);
                    var entrance = walkway.Neighbors()
                        .First(c => c.MapAreas.ContainsKey(areaInfo.Key));
                    Connect(walkway, entrance);
                } else if (area.Type == AreaType.Fill) {
                    // if it's a filled area it cannot be visited, so we remove
                    // all mentions of its cells:
                    // 1. remove all of its cells from their neighbors
                    // 2. remove all neighbors of its cells
                    // 3. remove all links that involve its cells
                    foreach (var cell in areaCells) {
                        cell.Neighbors().ForEach(
                            neighbor => neighbor.Neighbors().Remove(cell));
                        cell.Neighbors().Clear();
                        var links = cell.Links().ToArray();
                        links.ForEach(link => cell.Unlink(link));
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the given cell can be connected in the given direction.
        /// </summary>
        public bool CanConnect(MazeCell cell, Vector neighbor) =>
            _allConnectableCells.Contains(cell) &&
            cell.Neighbors(neighbor).HasValue &&
            _allConnectableCells.Contains(cell.Neighbors(neighbor).Value);

        /// <summary>
        /// Checks if the given cells can be connected with each other.
        /// </summary>
        public bool CanConnect(MazeCell one, MazeCell another) =>
            _allConnectableCells.Contains(one) &&
            _allConnectableCells.Contains(another) &&
            one.Neighbors().Contains(another);

        /// <summary>
        /// Check if the given cell is connected to the maze cells.
        /// </summary>
        public bool IsConnected(MazeCell cell) {
            var connected = _connectedCells.Contains(cell);
            // TODO: Prod-time assert library? I.e. Assert.That(cell.Links().Count > 0);
            return connected;
        }

        /// <summary>
        /// Connects one cell with it's neighbor in the provided direction.
        /// </summary>
        /// <param name="cell">The first cell to connect.</param>
        /// <param name="direction">The direction of the neighbor to connect.
        /// </param>
        public MazeCell Connect(MazeCell cell, Vector direction) {
            if (CanConnect(cell, direction)) {
                var another = cell.Neighbors(direction).Value;
                Connect(cell, another);
                return another;
            } else {
                throw new InvalidOperationException(
                    "No cell to connect to: " + cell);
            }
        }

        /// <summary>
        /// Connects two cells together.
        /// </summary>
        /// <param name="one">The first cell to connect.</param>
        /// <param name="another">The second cell to connect.</param>
        public void Connect(MazeCell one, MazeCell another) {
            Trace.WriteLine(string.Format("Connecting {0} to {1}.", one, another));
            // mark the cell as visited so it's not picked again in the 
            // PickNextRandomUnlinkedCell
            foreach (var cell in new MazeCell[] { one, another }) {
                if (_priorityCellsToConnect.ContainsKey(cell)) {
                    var cellsToRemove = _priorityCellsToConnect[cell];
                    cellsToRemove.ForEach(c => _priorityCellsToConnect.Remove(c));
                }
                // only the connected cell is removed from _cellsToConnect because
                // we still might want to connect other cells of the related areas.
                _cellsToConnect.Remove(cell);
                _connectedCells.Add(cell);
            }
            one.Link(another);
        }

        /// <summary>
        /// Checks if the maze is complete in accordance with the specified
        /// <see cref="GeneratorOptions"/>.
        /// </summary>
        /// <returns><c>true</c> if the maze is complete, otherwise 
        /// <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">This method has been
        /// called over maze.Size.Area ^ 3 times. Please report a bug providing
        /// the version of this library, the serialized maze, and options.
        /// </exception>
        public virtual bool IsFillComplete() {
            if (_isFillCompleteAttemptsMade >= _isFillCompleteAttempts) {
                throw new InvalidOperationException(
                    new StackTrace().GetFrame(1).GetType().FullName +
                    $" didn't complete in " +
                    $"{_isFillCompleteAttempts} loops. Please report a bug at" +
                    $" https://github.com/aynurin/maze-gen/issues.\n" +
                    $"Cells connected: {_connectedCells.Count} of " +
                    $"{_allConnectableCells.Count}.\n" +
                    $"Options: {_options}");
            }
            _isFillCompleteAttemptsMade--;
            if (_cellsToConnect.Count == 0) {
                return true;
            }
            if (_connectedCells.Count == 0) {
                return false;
            }
            switch (_options.FillFactor) {
                case FillFactorOption.FullWidth: {
                        var minX = _connectedCells.Min(c => c.Position.X);
                        var maxX = _connectedCells.Max(c => c.Position.X);
                        return minX == 0 && maxX == _maze.Size.X - 1;
                    }

                case FillFactorOption.FullHeight: {
                        var minY = _connectedCells.Min(c => c.Position.Y);
                        var maxY = _connectedCells.Max(c => c.Position.Y);
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

        /// <summary>
        /// The requested options set is not supported.
        /// </summary>
        /// <param name="options">The <see cref="GeneratorOptions"/> to check.
        /// </param>
        /// <exception cref="ArgumentException">The options set is not
        /// supported.</exception>
        public void ThrowIfIncompatibleOptions(GeneratorOptions options) {
            if (_options.FillFactor != options.FillFactor) {
                throw new ArgumentException(_options.Algorithm.Name +
                " doesn't currently support fill factors other than Full");
            }
        }

        /// <inheritdoc />
        public override string ToString() => this.DebugString() + "\n" + _maze.Serialize();
    }
}