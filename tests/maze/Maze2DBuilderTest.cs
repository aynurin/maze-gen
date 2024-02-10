using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Maze {

    [TestFixture]
    public class Maze2DBuilderTest {
        [Test]
        public void BuildsCorrectCellsCollections() {
            var maze = new Maze2D(5, 5);
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });

            Assert.That(builder.CellsToConnect, Has.Exactly(25).Items);
            Assert.That(builder.PriorityCells, Has.Exactly(0).Items);
        }

        [Test]
        public void BuildsCorrectCellsCollectionsWithHallAreas() {
            var maze = new Maze2D(6, 6);
            maze.AddArea(MapArea.Create(AreaType.Hall,
                                        new Vector(2, 3),
                                        new Vector(3, 2),
                                        "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });

            var priorityCells = new List<MazeCell>() {
                maze.AllCells[new Vector(2, 2)],
                maze.AllCells[new Vector(3, 2)],
                maze.AllCells[new Vector(4, 2)],
                maze.AllCells[new Vector(1, 3)],
                maze.AllCells[new Vector(5, 3)],
                maze.AllCells[new Vector(1, 4)],
                maze.AllCells[new Vector(5, 4)],
                maze.AllCells[new Vector(2, 5)],
                maze.AllCells[new Vector(3, 5)],
                maze.AllCells[new Vector(4, 5)],
            };

            Assert.That(builder.CellsToConnect, Has.Exactly(30).Items);
            Assert.That(builder.PriorityCells, Has.Exactly(10).Items);
            Assert.That(new List<MazeCell>(builder.PriorityCells.Keys),
                Is.EqualTo(priorityCells));
            Assert.That(builder.PriorityCells.First().Value,
                Is.EqualTo(priorityCells));

            foreach (var areaInfo in maze.MapAreas) {
                if (areaInfo.Key.Type == AreaType.Hall || areaInfo.Key.Type == AreaType.Fill) {
                    var selectableCells = builder.CellsToConnect.Intersect(areaInfo.Value).ToList();
                    if (selectableCells.Count > 0 || areaInfo.Value.Any(cell => cell.Links().Count > 0)) {
                        Assert.Fail("Hall cells are in the cellsToConnect collection or have links.");
                    }
                }
            }
        }

        [Test]
        public void BuildsCorrectCellsCollectionsWithHallAreasAtTheEdge() {
            var maze = new Maze2D(6, 6);
            maze.AddArea(MapArea.Create(AreaType.Hall,
                                        new Vector(3, 4),
                                        new Vector(3, 2),
                                        "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });

            var priorityCells = new List<MazeCell>() {
                maze.AllCells[new Vector(3, 3)],
                maze.AllCells[new Vector(4, 3)],
                maze.AllCells[new Vector(5, 3)],
                maze.AllCells[new Vector(2, 4)],
                maze.AllCells[new Vector(2, 5)],
            };

            Assert.That(builder.CellsToConnect, Has.Exactly(30).Items);
            Assert.That(builder.PriorityCells, Has.Exactly(5).Items);
            Assert.That(new List<MazeCell>(builder.PriorityCells.Keys),
                Is.EqualTo(priorityCells));
            Assert.That(builder.PriorityCells.First().Value,
                Is.EqualTo(priorityCells));
        }

        [Test]
        public void PicksPriorityCellsWhenAvailable() {
            var maze = new Maze2D(6, 6);
            maze.AddArea(MapArea.Create(AreaType.Hall,
                                        new Vector(3, 4),
                                        new Vector(3, 2),
                                        "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });

            var priorityCells = new List<MazeCell>() {
                maze.AllCells[new Vector(3, 3)],
                maze.AllCells[new Vector(4, 3)],
                maze.AllCells[new Vector(5, 3)],
                maze.AllCells[new Vector(2, 4)],
                maze.AllCells[new Vector(2, 5)],
            };

            Assert.That(builder.CellsToConnect, Has.Exactly(30).Items);
            Assert.That(builder.PriorityCells, Has.Exactly(5).Items);

            for (var i = 0; i < 1000; i++) {
                Assert.That(builder.PickNextCellToLink(),
                    Is.AnyOf(priorityCells));
            }

            foreach (var cell in priorityCells) {
                builder.MarkConnected(cell);
            }

            Assert.That(builder.ConnectedCells, Is.EqualTo(priorityCells));

            for (var i = 0; i < 1000; i++) {
                Assert.That(builder.PickNextCellToLink(),
                    Is.Not.AnyOf(priorityCells));
            }
        }

        [Test]
        public void PickRandomNeighborToLink() {
            var maze = new Maze2D(6, 6);
            maze.AddArea(MapArea.Create(AreaType.Hall,
                                        new Vector(3, 4),
                                        new Vector(3, 2),
                                        "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });

            // hall area neighbors are never chosen;
            for (var i = 0; i < 1000; i++) {
                builder.TryPickRandomNeighbor(maze.AllCells[new Vector(1, 5)], out var randomNeighbor);
                Assert.That(randomNeighbor, Is.Not.Null);
                Assert.That(builder.PriorityCells.ContainsKey(randomNeighbor), Is.True);
                Assert.That(randomNeighbor.X, Is.AnyOf(0, 2));
            }

            // hall area neighbors are never chosen;
            for (var i = 0; i < 1000; i++) {
                builder.TryPickRandomNeighbor(maze.AllCells[new Vector(1, 5)], out var randomNeighbor);
                Assert.That(randomNeighbor, Is.Not.Null);
                Assert.That(randomNeighbor.X, Is.Not.EqualTo(3));
            }
        }

        [Test]
        public void MarkConnected() {
            var maze = new Maze2D(3, 3);
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });

            var connectedCells = new List<MazeCell>() {
                maze.AllCells[new Vector(0, 0)],
                maze.AllCells[new Vector(1, 1)],
                maze.AllCells[new Vector(2, 2)]
            };

            connectedCells.ForEach(builder.MarkConnected);

            Assert.That(builder.CellsToConnect, Has.Exactly(6).Items);
            Assert.That(builder.PriorityCells, Has.Exactly(0).Items);
            Assert.That(builder.ConnectedCells, Has.Exactly(3).Items);

            connectedCells.ForEach(c =>
                Assert.That(builder.IsConnected(c)));
        }


        [Test]
        public void IterateUnlinkedCells_IteratesAllAvailableCells() {
            var maze = new Maze2D(6, 6);
            maze.AddArea(MapArea.Create(AreaType.Hall,
                                        new Vector(3, 4),
                                        new Vector(3, 2),
                                        "hall"));
            maze.AddArea(MapArea.Create(AreaType.Fill,
                                        new Vector(1, 1),
                                        new Vector(2, 3),
                                        "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });

            var priorityCells = new List<MazeCell>() {
                maze.AllCells[new Vector(3, 3)],
                maze.AllCells[new Vector(4, 3)],
                maze.AllCells[new Vector(5, 3)],
                maze.AllCells[new Vector(2, 4)],
                maze.AllCells[new Vector(2, 5)],
            };

            var unavailableCells = new List<MazeCell>() {
                maze.AllCells[new Vector(1, 1)],
                maze.AllCells[new Vector(2, 1)],
                maze.AllCells[new Vector(1, 2)],
                maze.AllCells[new Vector(2, 2)],
                maze.AllCells[new Vector(1, 3)],
                maze.AllCells[new Vector(2, 3)],
            };


            Assert.That(builder.CellsToConnect, Has.Exactly(24).Items);
            Assert.That(builder.PriorityCells, Has.Exactly(5).Items);

            var iterate = builder.AllMazeCells().ToList();
            var cellsOrder = maze.AllCells.ToList();

            Assert.That(iterate, Is.SupersetOf(priorityCells));
            Assert.That(iterate, Has.None.AnyOf(unavailableCells));

            // check if the order of cells in IterateCells matches the 
            // order of cells in maze.AllCells
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
            var maze = new Maze2D(5, 5);
            maze.AddArea(MapArea.Create(AreaType.Fill,
                                        new Vector(0, 0),
                                        new Vector(5, 5),
                                        "fill"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });

            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Full() {
            var maze = new Maze2D(2, 2);
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.AllCells[new Vector(0, 0)],
                maze.AllCells[new Vector(1, 0)]);
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.AllCells[new Vector(0, 0)],
                maze.AllCells[new Vector(0, 1)]);
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.AllCells[new Vector(0, 1)],
                maze.AllCells[new Vector(1, 1)]);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Full_WithHall() {
            var maze = new Maze2D(5, 5);
            maze.AddArea(MapArea.Create(AreaType.Hall,
                                        new Vector(1, 1),
                                        new Vector(3, 3),
                                        "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });

            Assert.That(builder.IsFillComplete(), Is.False);
            var areaCells = maze.AllCells
                    .IterateIntersection(new Vector(1, 1), new Vector(3, 3));
            var cellsToConnect = maze.AllCells.ToHashSet();
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
            var maze = new Maze2D(5, 5);
            maze.AddArea(MapArea.Create(AreaType.Fill,
                                        new Vector(1, 1),
                                        new Vector(3, 3),
                                        "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });

            Assert.That(builder.IsFillComplete(), Is.False);
            var areaCells = maze.AllCells
                    .IterateIntersection(new Vector(1, 1), new Vector(3, 3));
            var cellsToConnect = maze.AllCells.ToHashSet();
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
            var maze = new Maze2D(5, 5);
            maze.AddArea(MapArea.Create(AreaType.Cave,
                                        new Vector(1, 1),
                                        new Vector(3, 3),
                                        "hall"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });

            Assert.That(builder.IsFillComplete(), Is.False);
            var areaCells = maze.AllCells
                    .IterateIntersection(new Vector(1, 1), new Vector(3, 3));

            for (var i = 0; i < 4; i++) {
                builder.Connect(maze.AllCells[new Vector(i, 0)], Vector.East2D);
                builder.Connect(maze.AllCells[new Vector(i, 4)], Vector.East2D);
                builder.Connect(maze.AllCells[new Vector(0, i)], Vector.North2D);
                builder.Connect(maze.AllCells[new Vector(4, i)], Vector.North2D);
            }

            maze.AllCells
                    .IterateIntersection(new Vector(1, 1), new Vector(2, 2))
                    .ForEach(c => {
                        builder.Connect(c.cell, Vector.East2D);
                        builder.Connect(c.cell, Vector.North2D);
                    });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(maze.AllCells[new Vector(2, 3)], Vector.East2D);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_FullWidth() {
            var maze = new Maze2D(2, 2);
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.FullWidth
                });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.AllCells[new Vector(0, 0)],
                maze.AllCells[new Vector(0, 1)]);
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.AllCells[new Vector(0, 0)],
                maze.AllCells[new Vector(1, 0)]);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_FullHeight() {
            var maze = new Maze2D(2, 2);
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.FullHeight
                });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.AllCells[new Vector(0, 0)],
                maze.AllCells[new Vector(1, 0)]);
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.AllCells[new Vector(0, 0)],
                maze.AllCells[new Vector(0, 1)]);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Quarter() {
            var maze = new Maze2D(2, 2);
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Quarter
                });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.AllCells[new Vector(0, 0)],
                maze.AllCells[new Vector(1, 0)]);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_Half() {
            var maze = new Maze2D(2, 2);
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.Half
                });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.AllCells[new Vector(0, 0)],
                maze.AllCells[new Vector(1, 0)]);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_ThreeQuarters() {
            var maze = new Maze2D(2, 2);
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.ThreeQuarters
                });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.AllCells[new Vector(0, 0)],
                maze.AllCells[new Vector(1, 0)]);
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.AllCells[new Vector(0, 0)],
                maze.AllCells[new Vector(0, 1)]);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void IsFillComplete_NinetyPercent() {
            var maze = new Maze2D(2, 2);
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() {
                    FillFactor = GeneratorOptions.FillFactorOption.NinetyPercent
                });

            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.AllCells[new Vector(0, 0)],
                maze.AllCells[new Vector(1, 0)]);
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.AllCells[new Vector(0, 0)],
                maze.AllCells[new Vector(0, 1)]);
            Assert.That(builder.IsFillComplete(), Is.False);
            builder.Connect(
                maze.AllCells[new Vector(0, 1)],
                maze.AllCells[new Vector(1, 1)]);
            Assert.That(builder.IsFillComplete(), Is.True);
        }

        [Test]
        public void ConnectHalls() {
            var maze = new Maze2D(5, 5);
            maze.AddArea(MapArea.Create(AreaType.Hall,
                                        new Vector(1, 1),
                                        new Vector(3, 3),
                                        "hall"));
            var walkway = maze.AllCells[new Vector(1, 0)];
            var entrance = maze.AllCells[new Vector(1, 1)];
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });
            builder.Connect(walkway, Vector.West2D);

            var areaCells = maze.AllCells
                    .IterateIntersection(new Vector(1, 1), new Vector(3, 3));

            Assert.That(areaCells.Where(c => c.cell.Links().Count() > 0),
                Is.Empty);

            builder.ConnectHalls();

            var connectedCells = areaCells
                .Where(c => c.cell.Links().Count() > 0).ToList();

            Assert.That(connectedCells, Has.Exactly(1).Items);
            Assert.That(connectedCells.First().cell, Is.EqualTo(entrance));
            Assert.That(entrance.Links().First(), Is.EqualTo(walkway));
        }

        [Test]
        public void OverlappingAreas_ProduceValidPriorityCells() {
            var maze = new Maze2D(15, 15);
            var area1 = MapArea.Create(AreaType.Hall, new Vector(2, 3), new Vector(4, 7));
            var area2 = MapArea.Create(AreaType.Hall, new Vector(4, 8), new Vector(7, 3));
            maze.AddArea(area1);
            maze.AddArea(area2);
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });

            Assert.That(builder.PriorityCells.Keys.Intersect(maze.MapAreas[area1]), Is.Empty);
            Assert.That(builder.CellsToConnect.Intersect(maze.MapAreas[area1]), Is.Empty);
            Assert.That(builder.PriorityCells.Keys.Intersect(maze.MapAreas[area2]), Is.Empty);
            Assert.That(builder.CellsToConnect.Intersect(maze.MapAreas[area2]), Is.Empty);

            foreach (var areaInfo in maze.MapAreas) {
                if (areaInfo.Key.Type == AreaType.Hall || areaInfo.Key.Type == AreaType.Fill) {
                    var selectableCells = builder.CellsToConnect.Intersect(areaInfo.Value).ToList();
                    if (selectableCells.Count > 0 || areaInfo.Value.Any(cell => cell.Links().Count > 0)) {
                        Assert.Fail("Hall cells are in the cellsToConnect collection or have links.");
                    }
                }
            }
        }

        [Test]
        public void CanConnect() {
            var maze = new Maze2D(10, 10);
            maze.AddArea(MapArea.Create(AreaType.Hall,
                                        new Vector(1, 2),
                                        new Vector(3, 2),
                                        "hall"));
            maze.AddArea(MapArea.Create(AreaType.Fill,
                                        new Vector(6, 1),
                                        new Vector(2, 4),
                                        "fill"));
            maze.AddArea(MapArea.Create(AreaType.Cave,
                                        new Vector(1, 6),
                                        new Vector(2, 2),
                                        "cave"));
            var builder = new Maze2DBuilder(maze,
                new GeneratorOptions() { });

            Assert.That(
                builder.CanConnect(
                    maze.AllCells[new Vector(0, 0)],
                    Vector.East2D),
                Is.True);

            Assert.That(
                builder.CanConnect(
                    maze.AllCells[new Vector(0, 2)],
                    Vector.East2D),
                Is.False);

            Assert.That(
                builder.CanConnect(
                    maze.AllCells[new Vector(2, 1)],
                    Vector.North2D),
                Is.False);

            Assert.That(
                builder.CanConnect(
                    maze.AllCells[new Vector(2, 2)],
                    Vector.East2D),
                Is.False);

            Assert.That(
                builder.CanConnect(
                    maze.AllCells[new Vector(3, 2)],
                    Vector.East2D),
                Is.False);

            Assert.That(
                builder.CanConnect(
                    maze.AllCells[new Vector(4, 2)],
                    Vector.East2D),
                Is.True);

            Assert.That(
                builder.CanConnect(
                    maze.AllCells[new Vector(5, 2)],
                    Vector.East2D),
                Is.False);

            Assert.That(
                builder.CanConnect(
                    maze.AllCells[new Vector(0, 6)],
                    Vector.East2D),
                Is.True);

            Assert.That(
                builder.CanConnect(
                    maze.AllCells[new Vector(1, 5)],
                    Vector.North2D),
                Is.True);

            Assert.That(
                builder.CanConnect(
                    maze.AllCells[new Vector(1, 9)],
                    Vector.South2D),
                Is.True);

            Assert.That(
                builder.CanConnect(
                    maze.AllCells[new Vector(2, 6)],
                    Vector.East2D),
                Is.True);
        }
    }
}