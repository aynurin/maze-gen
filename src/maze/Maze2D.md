# Maze2D

## Background

### VisitedCells

VisitedCells are cells that have been visited by the generator.

The scenarios of using VisitedCells:

- IsFillComplete (all cases)
- FindLongestTrail

VisitedCells is implemented by filtering all cells on IsConnected == true.

IsConnected is set by Cell.Link/Unlink, and the only Unlink scenario is
ApplyAreas.

### UnlinkedCells

UnlinkedCells are cells that have not been visited by the generator.

The scenarios of using UnlinkedCells:

- Pick the next cell to visit by a generator.
- IsFillComplete for MazeFillFactor.Quarter|Half|ThreeQuarters|Full

!! Bug: UnlinkedCells is not managed in Cell.Link/Unlink. It's only
referenced in ApplyAreas and Parse.
!! Bug: In straight walk algorithms, we iterate UnlinkedCells. This is not a
problem, but in general we should walk via neighbors with previously applied
Fill cells.

## Goals

Basically, UnlinkedCells and VisitedCells are antagonists. Combined, they should
match \_cells (less filled areas).

Main operations are:

- Pick next random unlinked cell to visit
  - Needs to maintain priority (areas first for random picks)
- Iterate from lowest to highest unlinked cells via neighbors, considering
  filled areas.
- Get counts (unlinked cells left, visited cells, or left to visit)
- Get a random visited cell.

Also, I don't like the naming. "Visited" and "unvisited" terms are ambiguous,
because they can be applied to both the player and the algorithm. "Unlinked"
is vague because it technically tells that a cell has a link, and not that it
is not yet processed by the algorithm.

I need to implement the listed operations on Maze2D and ensure it's clear and
easy to understand.

### PickNextRandomCell and MarkConnected

Implementing PickNextRandomCell and MarkConnected requires random access to the
elements as well as removal of the elements.

Given that the counts are small (O(100)), it doesn't matter much. But it would
be interesting to find a way to combine random access and removal eficiency.
Memory complexity is not significant here.

| Algorithm          | Addition    | RandomAccess | Removal | Total[^3]        |
| ------------------ | ----------- | ------------ | ------- | ---------------- |
| Array/List         | O(1)        | O(n)         | O(n)    | O(2 \* n)        |
| Dictionary         | O(1)        | O(n)         | O(1)    | O(n)             |
| LinkedList         | O(1)        | O(n)         | O(1)    | O(n)             |
| Balanced Tree      |             |              |         |                  |
| - SortedDictionary | O(logN)[^4] | O(n)         | O(logN) | O(n + 2 \* logN) |
| - SortedList       | O(logN)[^2] | O(1)         | O(n)    | O(n + logN)      |
| - SortedSet        | O(logN)     | O(n)         | O(logN) | O(n + 2 \* logN) |

[^2]: Sorted.
[^3]:
    Estimated the same number of additions, removals, and random
    access.

[^4]:
    We can use SortedDictionary with a custom IComparer to ensure items are
    sorted the way we need.

It seems a Dictionary and a LinkedList can give us about the same performance
with the Dictionary giving a convenient UX.
