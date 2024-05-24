using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Areas.Evolving;
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
        private readonly Area _mazeArea;
        private readonly GeneratorOptions _options;

        private readonly int _isFillCompleteAttempts;
        private int _isFillCompleteAttemptsMade;

        private readonly HashSet<Cell> _cellsToConnect;
        private readonly Dictionary<Cell, List<Cell>> _priorityCellsToConnect;
        private readonly HashSet<Cell> _allConnectableCells;
        private readonly HashSet<Cell> _connectedCells =
            new HashSet<Cell>();
        private readonly List<HashSet<Cell>> _cellGroups;

        /// <summary>
        /// Cells that can be connected and are not connected yet.
        /// </summary>
        /// <remarks>
        /// Used only in <see cref="AldousBroderMazeGenerator"/>.
        /// </remarks>
        internal IReadOnlyCollection<Cell> TestCellsToConnect =>
            _cellsToConnect;
        /// <summary>
        /// All connectable cells (connected or unconnected) in the
        /// lowest xy to highest xy order with row priority.
        /// </summary>
        /// <remarks>
        /// Used only in <see cref="BinaryTreeMazeGenerator"/> and 
        /// <see cref="SidewinderMazeGenerator"/>.
        /// </remarks>
        internal IReadOnlyCollection<Cell> AllCells => _allConnectableCells;
        /// <summary>
        /// Cells that are already have connections to other cells.
        /// </summary>
        /// <remarks>
        /// Used only in tests.
        /// </remarks>
        /// <testonly />
        internal IReadOnlyCollection<Cell> TestConnectedCells =>
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
        internal Dictionary<Cell, List<Cell>> TestPriorityCells =>
            _priorityCellsToConnect;

        /// <summary>
        /// Contains groups of connectable cells. In case the maze field has
        /// isolated areas, some groups of cells can be isolated from others.
        /// This is the list of groups of connectable cells.
        /// </summary>
        public virtual List<HashSet<Cell>> CellGroups => _cellGroups;

        internal RandomSource Random { get => _options.RandomSource; }

        /// <summary>
        /// Creates a new instance of the <see cref="Maze2DBuilder"/> class.
        /// </summary>
        /// <param name="mazeArea"><see cref="Area" /> to build the maze on.
        /// </param>
        /// <param name="options"><see cref="GeneratorOptions" />.</param>
        public Maze2DBuilder(Area mazeArea, GeneratorOptions options) {
            _mazeArea = mazeArea;
            _options = options;

            if (_options.RandomSource == null) {
                throw new ArgumentNullException(
                    "Please specify a RandomSource to use for maze " +
                    "generation using GeneratorOptions.RandomSource.",
                    "_options.RandomSource");
            }
            if (_options.AreaGeneration == GeneratorOptions.AreaGenerationMode.Auto
                && _options.AreaGenerator == null) {
                throw new ArgumentNullException(
                    "Please specify an AreaGenerator to use for maze " +
                    "area generation using GeneratorOptions.AreaGenerator.");
            }
            if (_options.MazeAlgorithm == null) {
                throw new ArgumentNullException(
                    "Please specify maze generation algorithm using " +
                    "GeneratorOptions.MazeAlgorithm.");
            }
            if (!typeof(MazeGenerator).IsAssignableFrom(_options.MazeAlgorithm)) {
                throw new ArgumentException(
                    "Specified maze generation algorithm " +
                    $"({_options.MazeAlgorithm.FullName}) is not a subtype of " +
                    "MazeGenerator.");
            }
            if (_options.MazeAlgorithm.GetConstructor(Type.EmptyTypes) == null) {
                throw new ArgumentException(
                    "Specified maze generation algorithm " +
                    $"({_options.MazeAlgorithm.FullName}) does not have a " +
                    "default constructor.");
            }

            // Find priority cells to connect first. _priorityCellsToConnect are
            // associated with areas they relate to. When any of the given area
            // cells is processed, all "priority cells" of this area are removed
            // from the priority list (but stay in the regular pool so they can
            // be processed as normal.
            _priorityCellsToConnect = new Dictionary<Cell, List<Cell>>();
            foreach (var areaInfo in _mazeArea.ChildAreas) {
                var area = areaInfo;
                var mazeAreaCells = areaInfo.Cells // TODO: Not covered
                    .Select(c => c.Parent)
                    .ToList();
                if (area.Type == AreaType.Cave) {
                    // make sure all cave areas are linked.
                    mazeAreaCells.ForEach(
                        c => _priorityCellsToConnect.Set(c, mazeAreaCells));
                } else if (area.Type == AreaType.Hall) {
                    // halls will be connected later. BUT we need to make sure
                    // halls have at least one neighbor cell connected to the
                    // maze.
                    var cells = WalkInCells(area)
                        // make sure we don't include cells that belong to 
                        // any other area.
                        .Except(_mazeArea.ChildAreas
                            .Where(otherArea =>
                                area != otherArea &&
                                (otherArea.Type == AreaType.Hall ||
                                    otherArea.Type == AreaType.Fill))
                            .SelectMany(otherArea => // TODO: Not covered
                                otherArea.Cells.Select(c => c.Parent)))
                        .ToList();
                    cells.ForEach(c => _priorityCellsToConnect.Set(c, cells));
                }
            }

            _isFillCompleteAttempts = (int)Math.Pow(_mazeArea.Size.Area, 2);

            // all cells that do not belong to fill and hall areas.
            _cellsToConnect = new HashSet<Cell>(_mazeArea.Cells
                .Where(c => c.Children
                    .All(childCell =>
                        childCell.OwningArea.Value.Type != AreaType.Fill &&
                        childCell.OwningArea.Value.Type != AreaType.Hall)));

            // _cellsToConnect but persisted over time.
            _allConnectableCells = new HashSet<Cell>(_cellsToConnect);

            // in case the area has isolated areas, we need to find all
            // connectable groups of cells.
            var buffer = new HashSet<Cell>(_cellsToConnect);
            _cellGroups = new List<HashSet<Cell>>();
            if (buffer.Count > 0) {
                do {
                    var distances = DijkstraDistance.FindRaw(
                        this, buffer.First());
                    _cellGroups.Add(new HashSet<Cell>(distances.Keys));
                    buffer.ExceptWith(distances.Keys);
                } while (buffer.Count > 0);
            }
        }

        /// <summary>
        /// A helper method to generate a new maze on the given area.
        /// </summary>
        /// <exception cref="ArgumentNullException">Maze generation algorithm
        /// is not specified. See <see cref="GeneratorOptions.Algorithms" />.
        /// </exception>
        /// <exception cref="ArgumentException">The provided maze generator type
        /// is not inherited from <see cref="MazeGenerator" /> or does not
        /// provide a default constructor.</exception>
        public void BuildMaze() {
            try {
                GenerateMazeAreas(_mazeArea);
                (Activator.CreateInstance(_options.MazeAlgorithm) as MazeGenerator)
                    .GenerateMaze(this);
                ApplyAreas();
                _mazeArea.X(DeadEnd.Find(this));
                _mazeArea.X(DijkstraDistance.FindLongestTrail(this));
                _mazeArea.X(this);
            } catch (Exception ex) {
                throw new MazeGenerationException(_mazeArea, ex);
            }
        }

        /// <summary>
        /// A static helper method to generate a new maze on the given area.
        /// </summary>
        /// <returns>An instance of <see cref="Maze2DBuilder" /> used to
        /// generate the maze.</returns>
        /// <exception cref="ArgumentNullException">Maze generation algorithm
        /// is not specified. See <see cref="GeneratorOptions.Algorithms" />.
        /// </exception>
        /// <exception cref="ArgumentException">The provided maze generator type
        /// is not inherited from <see cref="MazeGenerator" /> or does not
        /// provide a default constructor.</exception>
        public static Maze2DBuilder BuildMaze(Area mazeArea,
                                     GeneratorOptions options = null) {
            var builder = new Maze2DBuilder(mazeArea,
                              options ?? new GeneratorOptions());
            builder.BuildMaze();
            return builder;
        }

        private void GenerateMazeAreas(
            Area mazeArea) {
            // count existing (desired) placement errors we can ignore when
            // checking auto-generated areas.
            var existingErrors =
                mazeArea.ChildAreas.Count(
                    area => area.IsPositionFixed &&
                            mazeArea.ChildAreas.Any(other =>
                                area != other &&
                                other.IsPositionFixed &&
                                area.OverlapArea(other).Area > 0)) +
                mazeArea.ChildAreas.Count(
                    area => area.IsPositionFixed &&
                            !area.FitsInto(Vector.Zero2D, mazeArea.Size));
            // when we auto-generate the areas, there is a <1% chance that we
            // can't auto-distribute (see DirectedDistanceForceProducer.cs) so
            // we make several attempts.
            var attempts = _options.AreaGeneration ==
                    GeneratorOptions.AreaGenerationMode.Auto ? 3 : 1;
            while (attempts > 0) {
                var allAreas = new List<Area>(mazeArea.ChildAreas);
                // add more rooms
                //     AreaGenerator creates new rooms as a separate list
                // layout
                //     Tries to layout Area rooms with new rooms
                // if all worked out, stop.
                if (_options.AreaGeneration ==
                    GeneratorOptions.AreaGenerationMode.Auto) {
                    var areaGenerator = (
                        _options.AreaGenerator ??
                        new RandomAreaGenerator(_options.RandomSource));
                    allAreas.AddRange(areaGenerator.Generate(mazeArea));
                }
                if (mazeArea.ChildAreas.Any(a => !a.IsPositionFixed)) {
                    new AreaDistributor(_options.RandomSource)
                        .Distribute(mazeArea, allAreas, 1);
                }
                // problem is: how do we distribute the rooms w/o changing
                // the original room locations?
                // on the other hand, if we deep clone, how do we let the area
                // know which rooms are new to add?
                // in a perfect world, we would deep clone, try to layout, and
                // if it worked,
                //          a: return a new mazeArea with the new layout
                //          b: add only new rooms to the original mazeArea
                // FIXME: while distributing, we can't disturb the original layout...
                var errors = -existingErrors +
                    allAreas.Count(
                        area => allAreas.Any(other =>
                                    area != other &&
                                    area.OverlapArea(other).Area > 0)) +
                    allAreas.Count(
                        area => !area.FitsInto(Vector.Zero2D, mazeArea.Size));
                if (errors <= 0) {
                    allAreas.Where(area => !mazeArea.ChildAreas.Contains(area))
                            .ForEach(area => mazeArea.CreateChildArea(area));
                } else if (--attempts == 0) {
                    var roomsDebugStr = string.Join(", ",
                        allAreas.Select(a => $"P{a.Position};S{a.Size}"));
                    var message =
                        $"Could not generate rooms for maze of size " +
                        $"{mazeArea.Size}. Last set of rooms had {errors} " +
                        $"errors ({string.Join(" ", roomsDebugStr)}) " +
                        $"{_options.RandomSource}.";
                    throw new MazeGenerationException(mazeArea, message);
                }
            }
        }

        private IEnumerable<Cell> WalkInCells(Area area) {
            if (area.Type == AreaType.Hall) {
                // find all cells next to this hall that can be linked to the
                // hall.
                return _mazeArea.Cells
                    .IterateIntersection(
                        new Vector(area.Position.Value.Select(c => c - 1)),
                        new Vector(area.Size.Value.Select(c => c + 2)))
                    .Where(c => !c.xy.IsIn(area.Position, area.Size) &&
                                 // get rid of corner cells that can't be an
                                 // entrance into this area.
                                 c.cell.Neighbors().Any(
                                    n => n.Children
                                        .Any(ch => ch.OwningArea == area)))
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
        public IEnumerable<Cell> GetPrioritizedCellsToConnect() =>
            _priorityCellsToConnect.Keys
                    .Concat(_cellsToConnect
                            .Except(_priorityCellsToConnect.Keys));

        /// <summary>
        /// Randomly picks the next cell to be connected from a pool of
        /// available cells.
        /// </summary>
        public virtual Cell PickNextCellToLink() {
            // pick the next cell for the maze generator.
            // skip halls and filled areas.
            // prioritize cave areas over other cells to make sure they are
            // connected.
            // also make sure hall areas have at least one neighbor cell
            // connected to the maze so we can connect them later.
            Cell nextCell;
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
        /// <see cref="TryPickRandomNeighbor(Cell, out Cell, bool, bool)"/>
        /// overload to filter.
        /// </remarks>
        /// <param name="cell">A neighbor of this cell will be returned.
        /// </param>
        /// <param name="neighbor">The returned neighbor.</param>
        /// <returns><c>true</c> if a neighbor was found, otherwise false.
        /// </returns>
        public bool TryPickRandomNeighbor(Cell cell,
                                          out Cell neighbor) =>
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
        public virtual bool TryPickRandomNeighbor(Cell cell, out Cell neighbor,
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
                    new List<Cell>();
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
            foreach (var areaInfo in _mazeArea.ChildAreas) {
                var area = areaInfo;
                var areaCells = areaInfo.Cells.Select(c => c.Parent);
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
                                 linkedCell.Children
                                    .Any(c => c.OwningArea == area));
                    // entrance can already be created by an overlapping area.
                    if (entranceExists) continue;
                    var visitedWalkInCells = walkInCells
                        .Where(c => _connectedCells.Contains(c)).ToList();
                    if (visitedWalkInCells.Count == 0) {
                        Trace.TraceWarning(
                            "Hall {0} has no visited entrance cells",
                            areaInfo
                        );
                        continue;
                    }
                    var walkway = _options.RandomSource.RandomOf(visitedWalkInCells);
                    var entrance = walkway.Neighbors()
                        .First(c => c.Children.Any(child => child.OwningArea == area));
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
        public bool CanConnect(Cell cell, Vector neighbor) =>
            _allConnectableCells.Contains(cell) &&
            cell.Neighbors(neighbor).HasValue &&
            _allConnectableCells.Contains(cell.Neighbors(neighbor).Value);

        /// <summary>
        /// Checks if the given cells can be connected with each other.
        /// </summary>
        public bool CanConnect(Cell one, Cell another) =>
            _allConnectableCells.Contains(one) &&
            _allConnectableCells.Contains(another) &&
            one.Neighbors().Contains(another);

        /// <summary>
        /// Check if the given cell is connected to the maze cells.
        /// </summary>
        public bool IsConnected(Cell cell) {
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
        public Cell Connect(Cell cell, Vector direction) {
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
        public void Connect(Cell one, Cell another) {
            Trace.WriteLine(string.Format("Connecting {0} to {1}.", one, another));
            // mark the cell as visited so it's not picked again in the 
            // PickNextRandomUnlinkedCell
            foreach (var cell in new Cell[] { one, another }) {
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
        /// the version of this library, the serialized maze, and _options.
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
                case MazeFillFactor.FullWidth: {
                        var minX = _connectedCells.Min(c => c.Position.X);
                        var maxX = _connectedCells.Max(c => c.Position.X);
                        return minX == 0 && maxX == _mazeArea.Size.X - 1;
                    }

                case MazeFillFactor.FullHeight: {
                        var minY = _connectedCells.Min(c => c.Position.Y);
                        var maxY = _connectedCells.Max(c => c.Position.Y);
                        return minY == 0 && maxY == _mazeArea.Size.Y - 1;
                    }

                case MazeFillFactor.Full:
                    return _cellsToConnect.Count == 0;

                default: {
                        var fillFactor =
                            _options.FillFactor ==
                                MazeFillFactor.Quarter ? 0.25 :
                            _options.FillFactor ==
                                MazeFillFactor.Half ? 0.5 :
                            _options.FillFactor ==
                                MazeFillFactor.ThreeQuarters ? 0.75 :
                            0.9;
                        return _cellsToConnect.Count <=
                            _allConnectableCells.Count * (1 - fillFactor);
                    }
            }
        }

        /// <summary>
        /// The requested _options set is not supported.
        /// </summary>
        /// <param name="_options">The <see cref="GeneratorOptions"/> to check.
        /// </param>
        /// <exception cref="ArgumentException">The _options set is not
        /// supported.</exception>
        public void ThrowIfIncompatibleOptions(GeneratorOptions _options) {
            if (_options.FillFactor != _options.FillFactor) {
                throw new ArgumentException(_options.MazeAlgorithm.Name +
                " doesn't currently support fill factors other than Full");
            }
        }

        /// <inheritdoc />
        public override string ToString() => this.DebugString() + "\n" + _mazeArea.Serialize(); // TODO: Not covered
    }
}