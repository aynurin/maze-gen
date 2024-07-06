# Area

The goal is to:

1. Generate a maze
2. Generate a map based on the maze
3. Modify the original maze impacting the generated map

Alternatively:

1. Generate a map with another approach
2. Optionally generate a maze somewhere in the map
3. Maybe??? Modify the original map impacting the maze?

Alternatively:

1. Generate a fancy area that looks like a maze
2. Make it a map
3. Generate a real maze in the corridors of the fancy map

All in all, we want to have a way to:

1. Convert between a maze and a map
2. Maintain the connection between the maze and the map, so changes to one
    of them cause changes in another

## Approaches

A layered structure, where every next layer depends on the previous layer. So
it will regenerate every time there is a change to the underlying layer.
This approach requires consistency, i.e. during regeneration, the unchanged
parts must remain the same.

### Maze -> Map

This should be easy as rendering a maze to a map is a repeatable and consistent
process.

### Map -> Maze

Here we need to adjust the maze generation algorithms to allow generating a part
of the maze as opposed to generating the full maze.

## Technical design

### Operations on cells

1. Get neighbors. Checks all neighbors on the grid, and validates rules in
    the neighbor cells. Are those cells ok for the requested goal (e.g., maze
    building)?
    
    When we create a maze layer, we need to bake the properties of the
    underlying map cells into the maze map. I.e. is this a "maze-able" cell?

    So cell properties are double-baked:

    1. When a map layer is created, if it's created based on an underlying layer
        all its cells inherit properties of the underlying cells.
    2. When any type of layer is composed (of all the child areas), the child
        area properties are baked into the cells.

2. Maze related operations (get connected, get unconnected, prioritization) are
    based on pre-computed collections created in the beginning of maze
    generation (see MazeBuilder.cs).

3. When a layer changes, it sends a message to an underlying layer reporting the
    change.