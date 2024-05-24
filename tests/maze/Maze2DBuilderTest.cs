using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Maze {

    [TestFixture]
    public class Maze2DBuilderTest : Test {
        [Test]
        public void BuildsCorrectCellsCollections() {
            var maze = Area.CreateEnvironment(new Vector(5, 5));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    RandomSource = RandomSource.CreateFromEnv()
                });

            Assert.That(builder.TestCellsToConnect, Has.Exactly(25).Items);
            Assert.That(builder.TestPriorityCells, Has.Exactly(0).Items);
        }

        [Test]
        public void BuildsCorrectCellsCollectionsWithHallAreas() {
            var maze = Area.CreateEnvironment(new Vector(6, 6));
            maze.CreateChildArea(
                Area.Create(new Vector(2, 3),
                            new Vector(3, 2),
                            AreaType.Hall,
                            "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    RandomSource = RandomSource.CreateFromEnv()
                });

            var priorityCells = new List<Cell>() {
                maze.Cells[new Vector(2, 2)],
                maze.Cells[new Vector(3, 2)],
                maze.Cells[new Vector(4, 2)],
                maze.Cells[new Vector(1, 3)],
                maze.Cells[new Vector(5, 3)],
                maze.Cells[new Vector(1, 4)],
                maze.Cells[new Vector(5, 4)],
                maze.Cells[new Vector(2, 5)],
                maze.Cells[new Vector(3, 5)],
                maze.Cells[new Vector(4, 5)],
            };

            Assert.That(builder.TestCellsToConnect, Has.Exactly(30).Items);
            Assert.That(builder.TestPriorityCells, Has.Exactly(10).Items);
            Assert.That(new List<Cell>(builder.TestPriorityCells.Keys),
                Is.EqualTo(priorityCells));
            Assert.That(builder.TestPriorityCells.First().Value,
                Is.EqualTo(priorityCells));

            foreach (var areaInfo in maze.ChildAreas) {
                var areaCells = areaInfo.Cells.Select(cell => cell.Parent);
                if (areaInfo.Type == AreaType.Hall || areaInfo.Type == AreaType.Fill) {
                    var selectableCells = builder.TestCellsToConnect.Intersect(areaCells).ToList();
                    if (selectableCells.Count > 0 || areaCells.Any(cell => cell.Links().Count > 0)) {
                        Assert.Fail("Hall cells are in the cellsToConnect collection or have links.");
                    }
                }
            }
        }

        [Test]
        public void BuildsCorrectCellsCollectionsWithHallAreasAtTheEdge() {
            var maze = Area.CreateEnvironment(new Vector(6, 6));
            maze.CreateChildArea(Area.Create(new Vector(3, 4),
                                     new Vector(3, 2),
                                     AreaType.Hall,
                                     "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    RandomSource = RandomSource.CreateFromEnv()
                });

            var priorityCells = new List<Cell>() {
                maze.Cells[new Vector(3, 3)],
                maze.Cells[new Vector(4, 3)],
                maze.Cells[new Vector(5, 3)],
                maze.Cells[new Vector(2, 4)],
                maze.Cells[new Vector(2, 5)],
            };

            Assert.That(builder.TestCellsToConnect, Has.Exactly(30).Items);
            Assert.That(builder.TestPriorityCells, Has.Exactly(5).Items);
            Assert.That(new List<Cell>(builder.TestPriorityCells.Keys),
                Is.EqualTo(priorityCells));
            Assert.That(builder.TestPriorityCells.First().Value,
                Is.EqualTo(priorityCells));
        }

        [Test]
        public void PicksPriorityCellsWhenAvailable() {
            var maze = Area.CreateEnvironment(new Vector(6, 6));
            maze.CreateChildArea(Area.Create(new Vector(3, 4),
                                     new Vector(3, 2),
                                     AreaType.Hall,
                                     "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    RandomSource = RandomSource.CreateFromEnv()
                });

            var priorityCells = new List<Cell>() {
                maze.Cells[new Vector(3, 3)],
                maze.Cells[new Vector(4, 3)],
                maze.Cells[new Vector(5, 3)],
                maze.Cells[new Vector(2, 4)],
                maze.Cells[new Vector(2, 5)],
            };

            Assert.That(builder.TestCellsToConnect, Has.Exactly(30).Items);
            Assert.That(builder.TestPriorityCells, Has.Exactly(5).Items);

            for (var i = 0; i < 1000; i++) {
                Assert.That(builder.PickNextCellToLink(),
                    Is.AnyOf(priorityCells));
            }

            foreach (var cell in priorityCells) {
                try {
                    builder.Connect(cell, Vector.East2D);
                } catch (InvalidOperationException) { }
                try {
                    builder.Connect(cell, Vector.North2D);
                } catch (InvalidOperationException) { }
            }

            Assert.That(builder.TestConnectedCells, Is.EqualTo(priorityCells));

            for (var i = 0; i < 1000; i++) {
                Assert.That(builder.PickNextCellToLink(),
                    Is.Not.AnyOf(priorityCells));
            }
        }

        [Test]
        public void PickRandomNeighborToLink() {
            var maze = Area.CreateEnvironment(new Vector(6, 6));
            maze.CreateChildArea(Area.Create(new Vector(3, 4),
                                     new Vector(3, 2),
                                     AreaType.Hall,
                                     "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    RandomSource = RandomSource.CreateFromEnv()
                });

            // hall area neighbors are never chosen;
            for (var i = 0; i < 1000; i++) {
                builder.TryPickRandomNeighbor(maze.Cells[new Vector(1, 5)], out var randomNeighbor);
                Assert.That(randomNeighbor, Is.Not.Null);
                Assert.That(builder.TestPriorityCells.ContainsKey(randomNeighbor), Is.True);
                Assert.That(randomNeighbor.Position.X, Is.AnyOf(0, 2));
            }

            // hall area neighbors are never chosen;
            for (var i = 0; i < 1000; i++) {
                builder.TryPickRandomNeighbor(maze.Cells[new Vector(1, 5)], out var randomNeighbor);
                Assert.That(randomNeighbor, Is.Not.Null);
                Assert.That(randomNeighbor.Position.X, Is.Not.EqualTo(3));
            }
        }

        [Test]
        public void MarkConnected() {
            var maze = Area.CreateEnvironment(new Vector(3, 3));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    RandomSource = RandomSource.CreateFromEnv()
                });

            var connectedCells = new List<Cell>() {
                maze.Cells[new Vector(0, 0)],
                maze.Cells[new Vector(1, 0)],
                maze.Cells[new Vector(1, 1)],
                maze.Cells[new Vector(1, 2)]
            };

            for (var i = 0; i < connectedCells.Count - 1; i++) {
                builder.Connect(connectedCells[i], connectedCells[i + 1]);
            }

            Assert.That(builder.TestCellsToConnect, Has.Exactly(5).Items);
            Assert.That(builder.TestPriorityCells, Has.Exactly(0).Items);
            Assert.That(builder.TestConnectedCells, Has.Exactly(4).Items);

            connectedCells.ForEach(c =>
                Assert.That(builder.IsConnected(c)));
        }


        [Test]
        public void IterateUnlinkedCells_IteratesAllAvailableCells() {
            var maze = Area.CreateEnvironment(new Vector(6, 6));
            maze.CreateChildArea(Area.Create(new Vector(3, 4),
                                     new Vector(3, 2),
                                     AreaType.Hall,
                                     "hall"));
            maze.CreateChildArea(Area.Create(new Vector(1, 1),
                                     new Vector(2, 3),
                                     AreaType.Fill,
                                     "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    RandomSource = RandomSource.CreateFromEnv()
                });

            var priorityCells = new List<Cell>() {
                maze.Cells[new Vector(3, 3)],
                maze.Cells[new Vector(4, 3)],
                maze.Cells[new Vector(5, 3)],
                maze.Cells[new Vector(2, 4)],
                maze.Cells[new Vector(2, 5)],
            };

            var unavailableCells = new List<Cell>() {
                maze.Cells[new Vector(1, 1)],
                maze.Cells[new Vector(2, 1)],
                maze.Cells[new Vector(1, 2)],
                maze.Cells[new Vector(2, 2)],
                maze.Cells[new Vector(1, 3)],
                maze.Cells[new Vector(2, 3)],
            };


            Assert.That(builder.TestCellsToConnect, Has.Exactly(24).Items);
            Assert.That(builder.TestPriorityCells, Has.Exactly(5).Items);

            var iterate = builder.AllCells.ToList();
            var cellsOrder = maze.Cells.ToList();

            Assert.That(iterate, Is.SupersetOf(priorityCells));
            Assert.That(iterate, Has.None.AnyOf(unavailableCells));

            // check if the order of cells in IterateCells matches the 
            // order of cells in maze.Cells
            var allCellsIndex = -1;
            var iterateIdx = 0;
            for (; iterateIdx < iterate.Count; iterateIdx++) {
                var thisCellIndex =
                    cellsOrder.IndexOf(iterate[iterateIdx], allCellsIndex + 1);
                Assert.That(thisCellIndex, Is.GreaterThan(allCellsIndex));
                allCellsIndex = thisCellIndex;
            }
        }

        [Test]
        public void IsFillComplete_NoCells() {
            var maze = Area.CreateEnvironment(new Vector(5, 5));
            maze.CreateChildArea(Area.Create(new Vector(0, 0),
                                     new Vector(5, 5),
                                     AreaType.Fill,
                                     "fill"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    RandomSource = RandomSource.CreateFromEnv()
                });

            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Full() {
            var maze = Area.CreateEnvironment(new Vector(2, 2));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.Cells[new Vector(0, 0)],
                maze.Cells[new Vector(1, 0)]);
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.Cells[new Vector(0, 0)],
                maze.Cells[new Vector(0, 1)]);
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.Cells[new Vector(0, 1)],
                maze.Cells[new Vector(1, 1)]);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Full_WithHall() {
            var maze = Area.CreateEnvironment(new Vector(5, 5));
            maze.CreateChildArea(Area.Create(new Vector(1, 1),
                                     new Vector(3, 3),
                                     AreaType.Hall,
                                     "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    RandomSource = RandomSource.CreateFromEnv()
                });

            Assert.That(builder.IsFillComplete(), Is.False);
            var areaCells = maze.Cells
                    .IterateIntersection(new Vector(1, 1), new Vector(3, 3));
            var cellsToConnect = maze.Cells.ToHashSet();
            cellsToConnect.ExceptWith(areaCells.Select(c => c.cell));
            foreach (var cell in cellsToConnect) {
                try {
                    builder.Connect(cell, Vector.East2D);
                } catch (InvalidOperationException) { }
                try {
                    builder.Connect(cell, Vector.North2D);
                } catch (InvalidOperationException) { }
            }
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Full_WithFill() {
            var maze = Area.CreateEnvironment(new Vector(5, 5));
            maze.CreateChildArea(Area.Create(new Vector(1, 1),
                                     new Vector(3, 3),
                                     AreaType.Fill,
                                     "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    RandomSource = RandomSource.CreateFromEnv()
                });

            Assert.That(builder.IsFillComplete(), Is.False);
            var areaCells = maze.Cells
                    .IterateIntersection(new Vector(1, 1), new Vector(3, 3));
            var cellsToConnect = maze.Cells.ToHashSet();
            cellsToConnect.ExceptWith(areaCells.Select(c => c.cell));
            foreach (var cell in cellsToConnect) {
                try {
                    builder.Connect(cell, Vector.East2D);
                } catch (InvalidOperationException) { }
                try {
                    builder.Connect(cell, Vector.North2D);
                } catch (InvalidOperationException) { }
            }
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Full_WithCave() {
            var maze = Area.CreateEnvironment(new Vector(5, 5));
            maze.CreateChildArea(Area.Create(new Vector(1, 1),
                                     new Vector(3, 3),
                                     AreaType.Cave,
                                     "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    RandomSource = RandomSource.CreateFromEnv()
                });

            Assert.That(builder.IsFillComplete(), Is.False);
            var areaCells = maze.Cells
                    .IterateIntersection(new Vector(1, 1), new Vector(3, 3));

            for (var i = 0; i < 4; i++) {
                builder.Connect(maze.Cells[new Vector(i, 0)], Vector.East2D);
                builder.Connect(maze.Cells[new Vector(i, 4)], Vector.East2D);
                builder.Connect(maze.Cells[new Vector(0, i)], Vector.North2D);
                builder.Connect(maze.Cells[new Vector(4, i)], Vector.North2D);
            }

            maze.Cells
                    .IterateIntersection(new Vector(1, 1), new Vector(2, 2))
                    .ForEach(c => {
                        builder.Connect(c.cell, Vector.East2D);
                        builder.Connect(c.cell, Vector.North2D);
                    });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(maze.Cells[new Vector(2, 3)], Vector.East2D);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_FullWidth() {
            var maze = Area.CreateEnvironment(new Vector(2, 2));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.MazeFillFactor.FullWidth,
                    RandomSource = RandomSource.CreateFromEnv()
                });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.Cells[new Vector(0, 0)],
                maze.Cells[new Vector(0, 1)]);
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.Cells[new Vector(0, 0)],
                maze.Cells[new Vector(1, 0)]);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_FullHeight() {
            var maze = Area.CreateEnvironment(new Vector(2, 2));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.MazeFillFactor.FullHeight,
                    RandomSource = RandomSource.CreateFromEnv()
                });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.Cells[new Vector(0, 0)],
                maze.Cells[new Vector(1, 0)]);
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.Cells[new Vector(0, 0)],
                maze.Cells[new Vector(0, 1)]);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Quarter() {
            var maze = Area.CreateEnvironment(new Vector(2, 2));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.MazeFillFactor.Quarter,
                    RandomSource = RandomSource.CreateFromEnv()
                });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.Cells[new Vector(0, 0)],
                maze.Cells[new Vector(1, 0)]);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Half() {
            var maze = Area.CreateEnvironment(new Vector(2, 2));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.MazeFillFactor.Half,
                    RandomSource = RandomSource.CreateFromEnv()
                });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.Cells[new Vector(0, 0)],
                maze.Cells[new Vector(1, 0)]);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_ThreeQuarters() {
            var maze = Area.CreateEnvironment(new Vector(2, 2));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.MazeFillFactor.ThreeQuarters,
                    RandomSource = RandomSource.CreateFromEnv()
                });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.Cells[new Vector(0, 0)],
                maze.Cells[new Vector(1, 0)]);
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.Cells[new Vector(0, 0)],
                maze.Cells[new Vector(0, 1)]);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_NinetyPercent() {
            var maze = Area.CreateEnvironment(new Vector(2, 2));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.MazeFillFactor.NinetyPercent,
                    RandomSource = RandomSource.CreateFromEnv()
                });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.Cells[new Vector(0, 0)],
                maze.Cells[new Vector(1, 0)]);
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.Cells[new Vector(0, 0)],
                maze.Cells[new Vector(0, 1)]);
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.Cells[new Vector(0, 1)],
                maze.Cells[new Vector(1, 1)]);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void ApplyAreas() {
            var maze = Area.CreateEnvironment(new Vector(5, 5));
            maze.CreateChildArea(Area.Create(new Vector(1, 1),
                                     new Vector(3, 3),
                                     AreaType.Hall,
                                     "hall"));
            var walkway = new List<Cell>() {
                maze.Cells[new Vector(1, 0)],
                maze.Cells[new Vector(0, 0)]
            };
            var entrance = maze.Cells[new Vector(1, 1)];
            var areaCells = maze.Cells
                    .IterateIntersection(new Vector(1, 1), new Vector(3, 3));

            Assert.That(maze.Cells.Where(c => c.Links().Count() > 0), Has.No.Member(entrance));
            Assert.That(maze.Cells.Where(c => c.Links().Count() > 0), Has.No.Member(walkway[0]));

            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    RandomSource = RandomSource.CreateFromEnv()
                });
            builder.Connect(walkway[0], walkway[1]);
            // this should connect entrance to walkway[0]
            builder.ApplyAreas();

            var connectedCells = areaCells
                .Where(c => c.cell.Links().Count() > 0)
                .Select(c => c.cell).ToList();

            Assert.That(maze.Cells.Where(c => c.Links().Count() > 0), Has.Member(entrance));
            Assert.That(maze.Cells.Where(c => c.Links().Count() > 0), Has.Member(walkway[0]));
            Assert.That(entrance.Links(), Has.Member(walkway[0]));
        }

        [Test]
        public void OverlappingAreas_ProduceValidPriorityCells() {
            var maze = Area.CreateEnvironment(new Vector(15, 15));
            var area1 = maze.CreateChildArea(Area.Create(new Vector(2, 3), new Vector(4, 7), AreaType.Hall));
            var area2 = maze.CreateChildArea(Area.Create(new Vector(4, 8), new Vector(7, 3), AreaType.Hall));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    RandomSource = RandomSource.CreateFromEnv()
                });

            Assert.That(builder.TestPriorityCells.Keys.Intersect(area1.Cells.Select(c => c.Parent)), Is.Empty);
            Assert.That(builder.TestCellsToConnect.Intersect(area1.Cells.Select(c => c.Parent)), Is.Empty);
            Assert.That(builder.TestPriorityCells.Keys.Intersect(area2.Cells.Select(c => c.Parent)), Is.Empty);
            Assert.That(builder.TestCellsToConnect.Intersect(area2.Cells.Select(c => c.Parent)), Is.Empty);

            foreach (var areaInfo in maze.ChildAreas) {
                var areaCells = areaInfo.Cells.Select(cell => cell.Parent);
                if (areaInfo.Type == AreaType.Hall || areaInfo.Type == AreaType.Fill) {
                    var selectableCells = builder.TestCellsToConnect.Intersect(areaCells).ToList();
                    if (selectableCells.Count > 0 || areaCells.Any(cell => cell.Links().Count > 0)) {
                        Assert.Fail("Hall cells are in the cellsToConnect collection or have links.");
                    }
                }
            }
        }

        [Test]
        public void CanConnect() {
            var maze = Area.CreateEnvironment(new Vector(10, 10));
            maze.CreateChildArea(Area.Create(new Vector(1, 2),
                                     new Vector(3, 2),
                                     AreaType.Hall,
                                     "hall"));
            maze.CreateChildArea(Area.Create(new Vector(6, 1),
                                     new Vector(2, 4),
                                     AreaType.Fill,
                                     "fill"));
            maze.CreateChildArea(Area.Create(new Vector(1, 6),
                                     new Vector(2, 2),
                                     AreaType.Cave,
                                     "cave"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    RandomSource = RandomSource.CreateFromEnv()
                });

            Assert.That(
                builder.CanConnect(
                    maze.Cells[new Vector(0, 0)],
                    Vector.East2D),
                Is.True);

            Assert.That(
                builder.CanConnect(
                    maze.Cells[new Vector(0, 2)],
                    Vector.East2D),
                Is.False);

            Assert.That(
                builder.CanConnect(
                    maze.Cells[new Vector(2, 1)],
                    Vector.North2D),
                Is.False);

            Assert.That(
                builder.CanConnect(
                    maze.Cells[new Vector(2, 2)],
                    Vector.East2D),
                Is.False);

            Assert.That(
                builder.CanConnect(
                    maze.Cells[new Vector(3, 2)],
                    Vector.East2D),
                Is.False);

            Assert.That(
                builder.CanConnect(
                    maze.Cells[new Vector(4, 2)],
                    Vector.East2D),
                Is.True);

            Assert.That(
                builder.CanConnect(
                    maze.Cells[new Vector(5, 2)],
                    Vector.East2D),
                Is.False);

            Assert.That(
                builder.CanConnect(
                    maze.Cells[new Vector(0, 6)],
                    Vector.East2D),
                Is.True);

            Assert.That(
                builder.CanConnect(
                    maze.Cells[new Vector(1, 5)],
                    Vector.North2D),
                Is.True);

            Assert.That(
                builder.CanConnect(
                    maze.Cells[new Vector(1, 9)],
                    Vector.South2D),
                Is.True);

            Assert.That(
                builder.CanConnect(
                    maze.Cells[new Vector(2, 6)],
                    Vector.East2D),
                Is.True);
        }
    }
}