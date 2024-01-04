using System;
using System.Collections.Generic;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps.Maze {
    public class GeneratorOptions {
        /// <summary>
        /// How much to fill the maze. <see
        /// cref="MazeGenerator.IsFillComplete(
        ///     GeneratorOptions, ICollection&lt;MazeCell&gt;,
        ///     ICollection&lt;MazeCell&gt;, Vector) "/> implementation for
        /// details.
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
        /// <see cref="MapAreaOptions.Auto"/>, this settings will be used to
        /// generate areas.
        /// </summary>
        public RandomAreaGenerator.GeneratorSettings
            AreaGeneratorSettings { get; set; } =
                RandomAreaGenerator.GeneratorSettings.Default;

        // TODO: Allow to specify the algorithm instance pre-created by the user.
        /// <summary>
        /// Algorithm to use when generating the maze. Has to be a type derived
        /// from <see cref="MazeGenerator"/>.
        /// </summary>
        public Type Algorithm { get; set; }

        /// <summary>
        /// How much to fill the maze. <see
        /// cref="MazeGenerator.IsFillComplete(
        ///     GeneratorOptions, ICollection&lt;MazeCell&gt;,
        ///     ICollection&lt;MazeCell&gt;, Vector) "/> implementation for
        /// details.
        /// </summary>
        public enum FillFactorOption {
            Full,
            FullWidth,
            FullHeight,
            Quarter,
            Half,
            ThreeQuarters,
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
            /// cref="RandomAreaGenerator.GeneratorSettings.Default" />).
            /// </summary>
            Auto
        }

        public static class Algorithms {
            public static readonly Type Default =
                typeof(RecursiveBacktrackerMazeGenerator);
            public static readonly Type AldousBroder =
                typeof(AldousBroderMazeGenerator);
            public static readonly Type HuntAndKill =
                typeof(HuntAndKillMazeGenerator);
            public static readonly Type RecursiveBacktracker =
                typeof(RecursiveBacktrackerMazeGenerator);
            public static readonly Type Sidewinder =
                typeof(SidewinderMazeGenerator);
            public static readonly Type Wilsons =
                typeof(WilsonsMazeGenerator);
        }
    }
}