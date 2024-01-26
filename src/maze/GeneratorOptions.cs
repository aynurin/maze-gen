using System;
using System.Collections.Generic;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// <see cref="MazeGenerator"/> options.
    /// </summary>
    public class GeneratorOptions {
        /// <summary>
        /// How much to fill the maze. <see
        /// cref="MazeGenerator.IsFillComplete(GeneratorOptions, Maze2D) "/>
        /// implementation for details.
        /// </summary>
        public FillFactorOption FillFactor { get; set; }
        /// <summary>
        /// How (and if) to generate areas in the maze.
        /// </summary>
        public MapAreaOptions MapAreasOptions { get; set; }
        /// <summary>
        /// When <see cref="MapAreasOptions"/> is set to
        /// <see cref="MapAreaOptions.Manual"/>, this list of areas will be added to
        /// the maze.
        /// generate areas.
        /// </summary>
        public List<MapArea> MapAreas { get; set; }
        /// <summary>
        /// When <see cref="MapAreasOptions"/> is set to
        /// <see cref="MapAreaOptions.Auto"/>, this generator will be used to
        /// generate areas.
        /// </summary>
        public AreaGenerator AreaGenerator { get; set; } =
                new RandomAreaGenerator(
                    RandomAreaGenerator.RandomAreaGeneratorSettings.Default);

        /// <summary>
        /// Algorithm to use when generating the maze. Has to be a type derived
        /// from <see cref="MazeGenerator"/>.
        /// </summary>
        public Type Algorithm { get; set; }

        /// <summary>
        /// How much to fill the maze. <see
        /// cref="MazeGenerator.IsFillComplete(GeneratorOptions, Maze2D) "/>
        /// implementation for details.
        /// </summary>
        public enum FillFactorOption {
            /// <summary>
            /// All cells of the maze will be visited.
            /// </summary>
            Full,
            /// <summary>
            /// The algorithm stops as soon as it visits <c>x = 0</c> and
            /// <c>x = maze.Size.X - 1</c>.
            /// </summary>
            FullWidth,
            /// <summary>
            /// The algorithm stops as soon as it visits <c>y = 0</c> and
            /// <c>x = maze.Size.Y - 1</c>.
            /// </summary>
            FullHeight,
            /// <summary>
            /// The algorithm stops as soon as it visits at least 25% of maze
            /// cells.
            /// </summary>
            Quarter,
            /// <summary>
            /// The algorithm stops as soon as it visits at least 50% of maze
            /// cells.
            /// </summary>
            Half,
            /// <summary>
            /// The algorithm stops as soon as it visits at least 75% of maze
            /// cells.
            /// </summary>
            ThreeQuarters,
            /// <summary>
            /// The algorithm stops as soon as it visits at least 90% of maze
            /// cells.
            /// </summary>
            NinetyPercent
        }

        /// <summary>
        /// How to generate various areas in the maze.
        /// </summary>
        public enum MapAreaOptions {
            /// <summary>
            /// Do not generate any areas.
            /// </summary>
            None,
            /// <summary>
            /// Specify the areas manually in
            /// <see cref="GeneratorOptions.MapAreas"/>.
            /// </summary>
            Manual,
            /// <summary>
            /// Use a generator to generate the areas. The generator can be
            /// specified in <see cref="GeneratorOptions.Algorithm" />. If a
            /// generator is not specified, a default generator is used (<see
            /// cref="RandomAreaGenerator.RandomAreaGeneratorSettings.Default" />).
            /// </summary>
            Auto
        }

        /// <summary>
        /// Maze generation algorithms implemented in this library.
        /// </summary>
        public static class Algorithms {
            /// <summary>
            /// <see cref="RecursiveBacktrackerMazeGenerator" />.
            /// </summary>
            public static readonly Type Default =
                typeof(RecursiveBacktrackerMazeGenerator);
            /// <summary>
            /// <see cref="AldousBroderMazeGenerator" />.
            /// </summary>
            public static readonly Type AldousBroder =
                typeof(AldousBroderMazeGenerator);
            /// <summary>
            /// <see cref="HuntAndKillMazeGenerator" />.
            /// </summary>
            public static readonly Type HuntAndKill =
                typeof(HuntAndKillMazeGenerator);
            /// <summary>
            /// <see cref="RecursiveBacktrackerMazeGenerator" />.
            /// </summary>
            public static readonly Type RecursiveBacktracker =
                typeof(RecursiveBacktrackerMazeGenerator);
            /// <summary>
            /// <see cref="SidewinderMazeGenerator" />.
            /// </summary>
            public static readonly Type Sidewinder =
                typeof(SidewinderMazeGenerator);
            /// <summary>
            /// <see cref="WilsonsMazeGenerator" />.
            /// </summary>
            public static readonly Type Wilsons =
                typeof(WilsonsMazeGenerator);
            /// <summary>
            /// <see cref="BinaryTreeMazeGenerator" />.
            /// </summary>
            public static readonly Type BinaryTree =
                typeof(BinaryTreeMazeGenerator);
        }
    }
}