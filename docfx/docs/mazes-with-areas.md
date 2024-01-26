## Types of areas

There are three types of areas that can be placed in the maze:

1. Halls, more like human-made halls or atriums with one or two entrances.
2. Caves, large openings in the maze with no specific rules around entrances.
3. Filled areas, such as mountains, ruins that cannot be explored, or just void
   spaces.

The type of space can be defined by setting the
[AreaType](xref:PlayersWorlds.Maps.Areas.AreaType) parameter in the `MazeArea`
[constructor](xref:PlayersWorlds.Maps.Areas.MapArea.#ctor*).

### Halls

Halls are placed before generating the maze to sure the maze generator algorithm
goes around them. As a last step halls are connected to the maze following the
entrance placement rules.

This works great for human-made structures (as we the humans like symmetry). It
also creates nice passages outside the walls of the areas making it feel even
more like it's built by humans.

Halls use specific rules of area entrance placement. The entrance can be placed
in the center of a wall. In a proper maze (where any cell is guaranteed to be
connected with any other cell by a passage), it doesn't matter where the room
connects to the maze.

### Caves

Caves don't block the generator algorithm from entering the area. Caves just
ignore all walls created by the generator, honoring all entrances.

Caves are good for making the environment look random or nature-made. There will
be no symmetry. Small areas can even not look like open areas because there are
too many entrances.

Caves feels like a nature-made cave or an animal burrow.

### Filled areas

Not visitable areas have to be placed on the map before the generator algorithm
starts. If we place an area like this after the maze is generated, we can
disrupt the proper maze and create dead areas that cannot be entered.
