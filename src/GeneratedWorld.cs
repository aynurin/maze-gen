using System;
using System.Linq;
using System.Collections.Generic;
using PlayersWorlds.Maps.Maze;
using PlayersWorlds.Maps.Areas;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;
using PlayersWorlds.Maps.MapFilters;
using PlayersWorlds.Maps.Maze.PostProcessing;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// A world
    /// </summary>
    public class GeneratedWorld {
        private readonly RandomSource _randomSource;
        private readonly List<Area> _layers = new List<Area>();

        /// <summary>
        /// Creates a new instance of an empty generated world.
        /// </summary>
        /// <param name="randomSource">The random source to use for this world.
        /// </param>
        public GeneratedWorld(RandomSource randomSource) {
            _randomSource = randomSource;
        }

        /// <summary>
        /// Adds a new layer to the generated world using the specified map
        /// dimensions.
        /// </summary>
        /// <param name="type"><see cref="AreaType"/> of the new layer.</param>
        /// <param name="size">The vector representing the dimensions of the
        /// new map layer.</param>
        /// <returns>The <see cref="GeneratedWorld"/> instance with the new
        /// layer added, allowing for method chaining.</returns>
        public GeneratedWorld AddLayer(AreaType type, Vector size) {
            _layers.Add(Area.Create(Vector.Zero2D, size, type));
            return this;
        }

        /// <summary>
        /// Adds a new layer to the generated world using the specified map
        /// dimensions.
        /// </summary>
        /// <param name="createLayer">Create a new layer based on the
        /// current layer</param>
        /// <returns>The <see cref="GeneratedWorld"/> instance with the new
        /// layer added, allowing for method chaining.</returns>
        public GeneratedWorld AddLayer(Func<Area, Area> createLayer) {
            _layers.Add(createLayer(CurrentLayer));
            return this;
        }

        /// <summary>
        /// Adds a new layer to the generated world cloning the last layer.
        /// </summary>
        /// <returns>The <see cref="GeneratedWorld"/> instance with the new
        /// layer added, allowing for method chaining.</returns>
        public GeneratedWorld AddLayer() {
            _layers.Add(CurrentLayer.ShallowCopy());
            return this;
        }

        public GeneratedWorld WithAreas(params Area[] areas) {
            areas.ForEach(a => CurrentLayer.AddChildArea(a));
            return this;
        }

        /// <summary>
        /// Adds random areas to the world describing parts of the environment.
        /// </summary>
        /// <param name="areaTypes">Types of areas to be added.</param>
        /// <param name="tags">Tags for the added areas.</param>
        /// <param name="count">Number of areas to add.</param>
        /// <param name="minSize">Minimum size of the added areas.</param>
        /// <param name="maxSize">Maximum size of the added areas.</param>
        /// <returns></returns>
        public GeneratedWorld WithAreas(AreaType[] areaTypes,
                                       string[] tags,
                                       int count,
                                       Vector minSize,
                                       Vector maxSize) {
            AreaGenerator areaGenerator =
                new BasicAreaGenerator(
                    _randomSource, CurrentLayer, areaTypes,
                    tags, count, minSize, maxSize,
                    CurrentLayer.ChildAreas());
            areaGenerator.GenerateMazeAreas(CurrentLayer);
            return this;
        }

        /// <summary>
        /// Adds environment areas that fill the maze and describe the map
        /// environment with tags.
        /// </summary>
        /// <returns>The <see cref="GeneratedWorld"/> instance with the new
        /// layer added, allowing for method chaining.</returns>
        public GeneratedWorld AddEnvironmentAreas(string[] tags) {
            // TODO: Implement this
            // var layer = CurrentLayer.ShallowCopy();
            // AreaGenerator areaGenerator = new EnvironmentAreaGenerator(
            //     _randomSource, layer, tags);
            // var areas = areaGenerator.Generate();
            // if (areas != null) {
            //     layer.AddAreas(areas);
            // } else throw new InvalidOperationException(
            //     "No valid areas generated.");
            // CurrentLayer = layer;
            return this;
        }


        /// <summary>
        /// Adds a maze layer to the generated world.
        /// </summary>
        /// <returns>The <see cref="GeneratedWorld"/> instance with the maze
        /// layer added, allowing for method chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the world is
        /// empty.</exception>
        public GeneratedWorld OfMaze(Type mazeGeneratorType = null) {
            var mazeLayer = CurrentLayer.ShallowCopy();
            // default: full area of the layer will be used for the maze.
            // alternative: add parameter to specify which area of the layer
            //      will be used for the maze.
            var options = new GeneratorOptions() {
                RandomSource = RandomSource.CreateFromEnv(),
                MazeAlgorithm = mazeGeneratorType ??
                                GeneratorOptions.Algorithms.Wilsons
            };
            void CreateMaze(Area area) {
                if (area.Type == AreaType.Maze) {
                    Maze2DBuilder.BuildMaze(area, options);
                }
                area.ChildAreas().ForEach(a => CreateMaze(a));
            };
            CreateMaze(mazeLayer);
            CurrentLayer = mazeLayer;
            return this;
        }

        /// <summary>
        /// Marks dead ends in the world.
        /// </summary>
        /// <returns></returns>
        public GeneratedWorld MarkDeadends() {
            var _ = CurrentLayer.X<Maze2DBuilder>() ??
                throw new InvalidOperationException(
                    "Can't use MarkDeadends on a non-maze layer.");
            CurrentLayer.X(DeadEnd.Find(CurrentLayer));
            return this;
        }

        /// <summary>
        /// Finds and marks the longest path in the world.
        /// </summary>
        /// <returns></returns>
        public GeneratedWorld MarkLongestPath() {
            var builder = CurrentLayer.X<Maze2DBuilder>() ??
                throw new InvalidOperationException(
                    "Can't use MarkLongestPath on a non-maze layer.");
            CurrentLayer.X(DijkstraDistance.FindLongestTrail(builder));
            return this;
        }

        public GeneratedWorld ToMap(MazeToMapOptions options = null) {
            var _ = CurrentLayer.X<Maze2DBuilder>() ??
                throw new InvalidOperationException(
                    "Can't use ToMap on a non-maze layer.");
            options = options ??
                      MazeToMapOptions.RectCells(
                            new Vector(1, 1), new Vector(1, 1));
            options.ThrowIfWrong(CurrentLayer.Size);
            var map = Maze2DRenderer.CreateMapForMaze(CurrentLayer, options);
            new Maze2DRenderer(CurrentLayer, options)
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail }, Cell.CellTag.MazeWall, options.WallWidths.Min(), options.WallHeights.Min()))
                .With(new Map2DSmoothCorners(Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner, options.WallWidths.Min(), options.WallHeights.Min()))
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail, Cell.CellTag.MazeWallCorner }, Cell.CellTag.MazeWall, options.WallWidths.Min(), options.WallHeights.Min()))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeVoid }, true, Cell.CellTag.MazeWall, 5, 5))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeWall, Cell.CellTag.MazeWallCorner }, false, Cell.CellTag.MazeTrail, 3, 3))
                .Render(map);
            CurrentLayer = map;
            return this;
        }

        /// <summary>
        /// Provides random elevation to the world.
        /// </summary>
        /// <param name="minElevation">The minimum elevation value. Default is
        /// -1.</param>
        /// <param name="maxElevation">The maximum elevation value. Default is
        /// 1.</param>
        /// <returns>The <see cref="GeneratedWorld"/> instance with the maze
        /// layer added, allowing for method chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the world is
        /// empty.</exception>
        public GeneratedWorld WithElevation(double minElevation = -1,
                                            double maxElevation = 1) {
            throw new NotImplementedException(
                "Elevation is not yet implemented.");
            // var newLayer = CurrentLayer().Clone();
            // var elevationOptions = new ElevationOptions() {
            //     Min = minElevation,
            //     Max = maxElevation,
            // };
            // _layers.Add(newLayer);
            // return this;
        }

        /// <summary>
        /// Scales the world map to the specified size.
        /// </summary>
        /// <param name="vector">The new size of the world.</param>
        /// <returns>The <see cref="GeneratedWorld"/> instance with the scaled
        /// layer added, allowing for method chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the world is
        /// empty.</exception>
        public GeneratedWorld Scale(Vector vector) {
            // !! scaling should depend on cell contents. E.g., when scaling a
            // !! maze, the neighbors / links will behave differently.
            // !! either don't allow scaling the maze, or implement a proper
            // !! scaling algorithm.
            _layers.Add(CurrentLayer.Scale(vector));
            return this;
        }

        /// <summary>
        /// Retrieves the map of the generated world.
        /// </summary>
        /// <returns>A <see cref="Area"/> instance representing the current
        /// state of the top layer of the world. If no layers have been added,
        /// an <see cref="InvalidOperationException"/> is thrown.</returns>
        public Area Map() {
            return CurrentLayer;
        }

        /// <summary>
        /// The current layer of the generated world.
        /// </summary>
        public Area CurrentLayer {
            get {
                if (_layers.Count == 0) {
                    throw new InvalidOperationException(
                        "No layers have been added to this world.");
                }
                return _layers.Last();
            }
            set {
                if (_layers.Count == 0) {
                    throw new InvalidOperationException(
                        "No layers have been added to this world.");
                }
                _layers[_layers.Count - 1] = value;
            }
        }

        /// <summary>
        /// Serializes the current state of the generated world into a string.
        /// </summary>
        /// <returns>A string that represents the serialized form of the generated world.</returns>
        public string Serialize() {
            return CurrentLayer.ToString();
        }
    }
}