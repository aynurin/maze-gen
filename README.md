# Summary

PlayersWorlds.Maps is a helper library that contains algorithms to help generate
dungeon and maze maps for your 2D and 3D games.

The basic workflow is:

1. Add the latest `PlayersWorlds.Maps.dll` to your Unity assets folder.
2. Use the following code to generate the map:

   ```c#
   var size = new Vector(20, 20);
   var map = MazeGenerator.Generate(size,
         new GeneratorOptions() {
            Algorithm = GeneratorOptions.Algorithms.RecursiveBacktracker,
            FillFactor = GeneratorOptions.FillFactorOption.ThreeQuarters,
            MapAreasOptions = GeneratorOptions.MapAreaOptions.Auto,
         }).ToMap(Maze.Maze2DRenderer.MazeToMapOptions.SquareCells(1, 1));
   ```

3. This will generate a maze-like dungeon map that looks like the following:

   ```
               ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
               ▓▓░░░░░░░░░░░░░░░░░░░░░░▓▓
               ▓▓░░▒▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▒▒░░▓▓
               ▓▓░░░░░░▓▓      ▓▓░░░░░░▓▓
               ▓▓▓▓▒▒░░▓▓      ▓▓░░▒▒▓▓▓▓
             ▓▓▓▓░░░░░░▓▓▓▓▓▓▓▓▓▓░░▓▓▓▓
       ▓▓▓▓▓▓▓▓▒▒░░▒▒▓▓▓▓▓▓▓▓▓▓▒▒░░▓▓
       ▓▓░░░░░░░░░░▓▓░░▓▓░░▓▓░░░░░░▓▓
       ▓▓░░▒▒▓▓▓▓▓▓▓▓░░▓▓░░▒▒▓▓▒▒░░▓▓
       ▓▓░░▓▓░░░░░░▒▒░░▓▓░░░░░░░░░░▓▓
       ▓▓░░▓▓░░░░░░░░░░▓▓▓▓▓▓▓▓▓▓▓▓▓▓
       ▓▓░░▓▓░░░░░░▒▒░░▓▓░░░░░░▓▓░░▓▓▓▓
       ▓▓░░▒▒▓▓▓▓▓▓▒▒░░▓▓░░░░░░▒▒░░▒▒▓▓▓▓▓▓▓▓
       ▓▓░░░░░░░░░░░░░░▓▓░░░░░░░░░░░░░░░░░░▓▓
       ▓▓▒▒░░▒▒▓▓▓▓▒▒░░▓▓░░░░░░▒▒░░▒▒▓▓▒▒░░▓▓
       ▓▓░░░░░░▓▓▓▓▓▓░░▓▓░░░░░░▓▓░░▓▓░░░░░░▓▓
       ▓▓░░░░░░▓▓  ▓▓░░▓▓▓▓▓▓▓▓▓▓▓▓▒▒░░▒▒░░▓▓
       ▓▓░░░░░░▓▓  ▓▓░░▓▓░░░░░░░░░░░░░░▓▓░░▓▓
       ▓▓▓▓▓▓▓▓▓▓  ▓▓░░▒▒░░▒▒▓▓▓▓▓▓▓▓▓▓▒▒░░▓▓
                   ▓▓░░░░░░▓▓░░░░░░░░░░░░░░▓▓
                   ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
   ```

4. The resulting `map` is a [Map2D](https://aynurin.github.io/maze-gen/api/PlayersWorlds.Maps.Map2D.html)
   instance that contains cells that map to Unity cells. To render the map you
   iterate over the cells of the `map` and render whatever map objects you want
   based on the cell tags, e.g. walls or floor tiles.

5. The generated maze also contains markers for dead ends that can be a good
   place to place loot, as well as the markers for the suggested starting and
   ending cells that have a guaranteed and longest path between them.

# Contributing

Check out the [contributing guide](CONTRIBUTING.md).

# Dev Environment

1. Your favorite code editor. VSCode or Code OSS work well (I'm using Code OSS).
   Please note that MonoDevelop (and perhaps Visual Studio) will change the
   .csproj files putting a lot of unnecessary stuff there. If using one of those
   IDEs, please don't send the .csproj files in your PRs to make sure the
   project is maintainable in VSCode.
2. This project is built for .NET Framework 4.7.2. Later versions of .NET are
   not supported by Unity (yet?). I'm using the latest `mono` on Linux.
3. All the Build/Test workflows are described in `./.vscode/tasks.json`, so if
   using VSCode derived editor, you can use tasks to build, test, and get a
   coverage report.
4. Running the example maze-gen:

   ```bash
   mono --debug build/Debug/mazegen/maze-gen.exe
   ```

See some notes in the
