# Maze rendering on the map

## Implementation

A map has several layers:

1. Cells to match Unity Cells
2. Areas to denote game a group of Cells that denote a POI or have
different visual style
3. Maze Cell that denotes enterable / walkable Cell or group of Cells

### algo

1. create a map and mark cells on all layers as individual cells
2. create areas by adjoining cells on the Areas layer
3. process Maze Cells to reflect Area Cells honoring Area Cell Types
     Fill cells remove Maze Cells
     Hall cells adjoin Maze Cells
     Maze cells do nothing
     Area Types might have some rules attached to them to ensure the
     Maze is still a Maze and have enough corridors when played
4. run the maze generators on the Maze Cells layer

MazeGenerator code would be something like:

```c#
public static Map2D GenerateWithAreas<T>(Vector size)
    where T : MazeGenerator, new() {
    var map = new Map2D(size);
    (new T()).GenerateMaze(map);
    var composer = new Map2DAreaComposer(map);
    var areaGenerator = new RandomAreaGenerator(
        RandomAreaGenerator.GeneratorSettings.Default);
    foreach (var newArea in areaGenerator) {
        if (!composer.TryPlaceArea(newArea)) {
            // If not enough areas, maybe make several attempts
            // to place the next area hoping we'll find something that
            // fits
            break;
        }
    }
    return map;
}
```

### DS

* Map2D consists of (1) Cells.
* Cells have x,y coordinates
* Each Cell has a link to an Area and a Maze Cell
* Maze Cells are connected to each other
* Maze Cells know of their relative positioning using Cell x,y coordinates

## Cells and walls

It seems there are three separate ideas:

* Physical coordinates of the world (e.g., Unity cells)
* Maze nodes / corridors
* Visual styles that can apply to mazes

There is also a desire to allow mazes to have special areas - like rooms or
impenetrable solid zones. How do we define data structures for this entire
config?

Some thoughts on this in a [spreadsheet](https://docs.google.com/spreadsheets/d/1qvLtWNySSYW7v10g28d1CcepfDeN-g649f2Hsff5yDs/edit#gid=0).

Things that can overlap will be on different layers, so there are the following
layers:

* World
* Maze
* Style

Each layer will consist of it's own data structure mappable to the World
coordinates. Effectively, all layers will impact World layers in their own way.

Now, how do we put rooms to these layers?

