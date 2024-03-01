using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze.PostProcessing;
using static PlayersWorlds.Maps.Maze.GeneratorOptions;

namespace PlayersWorlds.Maps.Maze {

    [TestFixture]
    internal class MazeGeneratorAreasTest {
        public Maze2D GenerateMaze(Type generatorType, List<MapArea> areas) =>
            MazeTestHelper.GenerateMaze(new Vector(15, 15),
                new GeneratorOptions() {
                    FillFactor = FillFactorOption.Full,
                    Algorithm = generatorType,
                    MapAreasOptions = MapAreaOptions.Manual,
                    MapAreas = areas
                });

        [Test]
        [Repeat(10), Category("Integration")]
        public void FillAreasAreAppliedProperly(
            [ValueSource("GetAllGenerators")] Type generatorType) {
            var fill = MapArea.Create(AreaType.Fill, new Vector(2, 4), new Vector(3, 4));

            var maze = GenerateMaze(generatorType, new List<MapArea>() { fill });

            var fillCells = maze.MapAreas[fill];
            Assert.That(fillCells, Has.Exactly(12).Items);

            var otherCells = maze.Cells
                .Except(fillCells)
                .ToList();

            Assert.That(otherCells, Has.Exactly(213).Items);

            // fill areas do not have links with the external cells
            Assert.That(fillCells.SelectMany(cell => cell.Links()),
                Has.None.AnyOf(otherCells));
        }

        [Test]
        [Repeat(10), Category("Integration")]
        public void HallAreasAreAppliedProperly(
            [ValueSource("GetAllGenerators")] Type generatorType) {
            var hall = MapArea.Create(AreaType.Hall, new Vector(8, 2), new Vector(4, 5));
            var maze = GenerateMaze(generatorType, new List<MapArea>() { hall });

            var areaArea = 20;
            var areaPerimeter = 14;
            var mazeArea = 225;

            var hallCells = maze.Cells
                .IterateIntersection(hall.Position, hall.Size)
                .Select(cell => cell.cell).ToList();
            Assert.That(hallCells, Has.Exactly(areaArea).Items);

            var otherCells = maze.Cells
                .Except(hallCells)
                .ToList();

            Assert.That(otherCells, Has.Exactly(mazeArea - areaArea).Items);
            // hall areas have only one link with the external cells
            Assert.That(hallCells.SelectMany(cell => cell.Links()),
                Has.Exactly(1).AnyOf(otherCells));

            var hallInnerCells = maze.Cells.IterateIntersection(
                hall.Position + Vector.NorthEast2D,
                hall.Size + Vector.SouthWest2D + Vector.SouthWest2D)
                .Select(cell => cell.cell).ToList();
            Assert.That(hallInnerCells.Count(),
                Is.EqualTo(areaArea - areaPerimeter));
            // hall areas don't have walls inside the areas
            Assert.That(hallInnerCells.All(cell => cell.Links().Count == 4));

            var hallEdgeCells = new HashSet<MazeCell>(hallCells);
            hallEdgeCells.ExceptWith(hallInnerCells);
            Assert.That(hallEdgeCells.Count(), Is.EqualTo(areaPerimeter));

            // only corners can have at least two links
            Assert.That(hallEdgeCells.Count(cell => cell.Links().Count == 2),
                Is.LessThanOrEqualTo(4));
            // all other edge cells have 3 or more links
            Assert.That(hallEdgeCells.Count(cell => cell.Links().Count >= 3),
                Is.GreaterThanOrEqualTo(areaPerimeter - 4));

            // exactly one of hall edge cells is connected to the external cells
            Assert.That(
                hallEdgeCells.SelectMany(cell => cell.Links()),
                Has.Exactly(1).AnyOf(otherCells));
        }

        [Test]
        [Repeat(10), Category("Integration")]
        public void CaveAreasAreAppliedProperly(
            [ValueSource("GetAllGenerators")] Type generatorType) {
            var cave = MapArea.Create(AreaType.Cave, new Vector(5, 10), new Vector(7, 3));
            var maze = GenerateMaze(generatorType, new List<MapArea>() { cave });

            var areaArea = 21;
            var areaPerimeter = 16;
            var mazeArea = 225;

            var caveCells = maze.MapAreas[cave];
            Assert.That(caveCells, Has.Exactly(21).Items);

            var otherCells = maze.Cells
                .Except(caveCells)
                .ToList();

            Assert.That(otherCells, Has.Exactly(mazeArea - areaArea).Items);
            // cave areas have at least one link with the external cells
            Assert.That(caveCells.SelectMany(cell => cell.Links())
                                 .Intersect(otherCells).ToList(),
                        Has.Count.GreaterThanOrEqualTo(1));

            var caveInnerCells = maze.Cells.IterateIntersection(
                cave.Position + Vector.NorthEast2D,
                cave.Size + Vector.SouthWest2D + Vector.SouthWest2D)
                .Select(cell => cell.cell).ToList();
            Assert.That(caveInnerCells.Count(),
                Is.EqualTo(areaArea - areaPerimeter));
            // cave areas don't have walls inside the areas
            Assert.That(caveInnerCells.All(cell => cell.Links().Count == 4));

            var caveEdgeCells = new HashSet<MazeCell>(caveCells);
            caveEdgeCells.ExceptWith(caveInnerCells);
            Assert.That(caveEdgeCells.Count(), Is.EqualTo(areaPerimeter));
            // only corners can have at least two links
            Assert.That(caveEdgeCells.Count(cell => cell.Links().Count == 2),
                Is.LessThanOrEqualTo(4));
            // all other edge cells have 3 or more links
            Assert.That(caveEdgeCells.Count(cell => cell.Links().Count >= 3),
                Is.GreaterThanOrEqualTo(areaPerimeter - 4));
        }

        [Test]
        [Repeat(10), Category("Integration")]
        public void OverlappingAreasAreAppliedProperly(
            [ValueSource("GetAllGenerators")] Type generatorType,
            [ValueSource("GetAllAreaTypes")] AreaType areaType) {
            var area1 = MapArea.Create(areaType, new Vector(2, 3), new Vector(4, 7));
            var area2 = MapArea.Create(areaType, new Vector(4, 8), new Vector(7, 3));
            var maze = GenerateMaze(generatorType, new List<MapArea>() { area1, area2 });

            var areaArea = 45;
            var mazeArea = 225;
            var areaCells = maze.MapAreas[area1]
                            .Concat(maze.MapAreas[area2])
                            .Distinct();

            Assert.That(maze.MapAreas[area1], Has.Exactly(28).Items);
            Assert.That(maze.MapAreas[area2], Has.Exactly(21).Items);
            Assert.That(areaCells, Has.Exactly(areaArea).Items);

            var otherCells = maze.Cells
                .Except(maze.MapAreas[area1])
                .Except(maze.MapAreas[area2])
                .ToList();

            Assert.That(otherCells, Has.Exactly(mazeArea - areaArea).Items);

            if (areaType == AreaType.Fill) {
                // fill areas do not have links with the external cells
                Assert.That(areaCells.SelectMany(cell => cell.Links()),
                    Has.None.AnyOf(otherCells));
            } else {
                var innerCells = maze.Cells.IterateIntersection(
                    area1.Position + Vector.NorthEast2D,
                    area1.Size + Vector.SouthWest2D + Vector.SouthWest2D)
                    .Concat(
                        maze.Cells.IterateIntersection(
                            area2.Position + Vector.NorthEast2D,
                            area2.Size + Vector.SouthWest2D + Vector.SouthWest2D)
                    ).Select(cell => cell.cell).Distinct().ToList();
                // cave areas don't have walls inside the areas
                Assert.That(innerCells.All(cell => cell.Links().Count == 4), string.Join(",", innerCells.Where(cell => cell.Links().Count < 4)));

                var edgeCells = new HashSet<MazeCell>(areaCells);
                edgeCells.ExceptWith(innerCells);
                // only corners can have at least two links
                Assert.That(edgeCells.Count(cell => cell.Links().Count == 2),
                    Is.LessThanOrEqualTo(6));
                // all other edge cells have 3 or more links
                Assert.That(edgeCells.Count(cell => cell.Links().Count >= 3),
                    Is.GreaterThanOrEqualTo(edgeCells.Count() - 6));

            }
        }

        [Test]
        [Repeat(10), Category("Integration")]
        public void TwoMatchingAreas(
            [ValueSource("GetAllGenerators")] Type generatorType,
            [ValueSource("GetAllAreaTypes")] AreaType areaType) {
            var area1 = MapArea.Create(areaType, new Vector(2, 2), new Vector(2, 2));
            var area2 = MapArea.Create(areaType, new Vector(2, 2), new Vector(2, 2));
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(6, 6), new GeneratorOptions() {
                    Algorithm = generatorType,
                    MapAreas = new List<MapArea>() { area1, area2 },
                    FillFactor = FillFactorOption.Full,
                },
                out var builder);
            Assert.That(MazeTestHelper.IsSolveable(maze));

            var areaArea = 4;
            var mazeArea = 36;
            var expectConnected =
                areaType == AreaType.Cave ? mazeArea : mazeArea - areaArea;
            if (areaType == AreaType.Hall) {
                expectConnected += 1; // entrance cell within the call.
            }

            Assert.That(maze.MapAreas[area1], Has.Exactly(areaArea).Items);
            Assert.That(maze.MapAreas[area2], Has.Exactly(areaArea).Items);
            Assert.That(maze.MapAreas[area1], Is.EqualTo(maze.MapAreas[area2]));
            Assert.That(builder.CellsToConnect, Is.Empty);
            Assert.That(builder.ConnectedCells,
                Has.Exactly(expectConnected).Items);
        }

        [Test]
        [Repeat(10), Category("Integration")]
        public void DenseWalkways(
            [ValueSource("GetAllGenerators")] Type generatorType,
            [ValueSource("GetAllAreaTypes")] AreaType areaType) {
            var areas = new List<MapArea>() {
                MapArea.Create(areaType, new Vector(1, 1), new Vector(2, 2)),
                MapArea.Create(areaType, new Vector(4, 1), new Vector(2, 3)),
                MapArea.Create(areaType, new Vector(7, 1), new Vector(2, 2)),
                MapArea.Create(areaType, new Vector(1, 4), new Vector(2, 3)),
                MapArea.Create(areaType, new Vector(4, 5), new Vector(1, 2)),
                MapArea.Create(areaType, new Vector(6, 5), new Vector(1, 2)),
                MapArea.Create(areaType, new Vector(7, 4), new Vector(2, 1)),
                MapArea.Create(areaType, new Vector(8, 6), new Vector(1, 1)),
            };
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(10, 8), new GeneratorOptions() {
                    Algorithm = generatorType,
                    MapAreas = areas,
                    FillFactor = GeneratorOptions.FillFactorOption.Full,
                },
                out var builder);

            var areaArea = areas.Sum(area => area.Size.Area);
            var mazeArea = maze.Size.Area;
            var expectConnected =
                areaType == AreaType.Cave ? mazeArea : mazeArea - areaArea;
            if (areaType == AreaType.Hall) {
                expectConnected += areas.Count; // entrance cell within the call.
            }

            Assert.That(builder.CellsToConnect, Is.Empty);
            Assert.That(builder.ConnectedCells,
                Has.Exactly(expectConnected).Items);
        }

        [Test]
        [Repeat(10), Category("Integration")]
        public void WilsonsHalls() {
            GenerateMaze(GeneratorOptions.Algorithms.Wilsons,
                new List<MapArea>() {
                    MapArea.Create(AreaType.Hall,
                                   new Vector(2, 3),
                                   new Vector(4, 7)) });

            // The algorithm hung up b/c wilson's creates it's own walk paths
            // without marking the cells as visited. And if it's choosing only
            // priority cells while doing that, it will walk along one edge of
            // an area forever.
            Assert.Pass();
        }

        [Test]
        [Repeat(10), Category("Integration")]
        public void ScatteredAreas(
            [ValueSource("GetAllGenerators")] Type generatorType,
            [ValueSource("GetAllAreaTypes")] AreaType areaType,
            [ValueSource("GetAllFillFactors")] FillFactorOption fillFactor) {
            if (!MazeTestHelper.IsSupported(generatorType, fillFactor)) {
                Assert.Ignore();
            }
            if (areaType == AreaType.Fill) {
                Assert.Ignore(); // we can't test fill areas here.
            }
            if (generatorType == typeof(AldousBroderMazeGenerator) ||
                generatorType == typeof(HuntAndKillMazeGenerator) ||
                generatorType == typeof(RecursiveBacktrackerMazeGenerator)) {
                if (fillFactor == FillFactorOption.Quarter ||
                    fillFactor == FillFactorOption.Half ||
                    fillFactor == FillFactorOption.ThreeQuarters ||
                    // TODO(#32): Re-enable low fill factors with probabalistic 
                    //            testing
                    fillFactor == FillFactorOption.NinetyPercent ||
                    fillFactor == FillFactorOption.FullWidth ||
                    fillFactor == FillFactorOption.FullHeight) {
                    // full layouts are uninteresting for this test
                    Assert.Ignore();
                    return;
                }
            }
            var area1 = MapArea.Create(areaType, new Vector(2, 2), new Vector(2, 2));
            var area2 = MapArea.Create(areaType, new Vector(24, 24), new Vector(2, 2));
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(30, 30),
                new GeneratorOptions() {
                    Algorithm = generatorType,
                    MapAreas = new List<MapArea>() { area1, area2 }
                },
                out _);

            var paths = DijkstraDistance.Find(maze.Cells[area1.Position]);

            Assert.That(paths.ContainsKey(maze.Cells[area2.Position]));
        }

        [Test]
        [Repeat(100), Category("Integration")]
        public void ManualAndAutoAreasGeneration() {
            var options = new GeneratorOptions() {
                Algorithm = GeneratorOptions.Algorithms.AldousBroder,
                MapAreas = new List<MapArea>() {
                    MapArea.Create(AreaType.Hall,
                                   new Vector(2, 3),
                                   new Vector(4, 7), "fixed"),
                    MapArea.CreateAutoPositioned(AreaType.Hall, new Vector(2, 5), "auto")
                },
                MapAreasOptions = GeneratorOptions.MapAreaOptions.Auto
            };
            var maze = MazeTestHelper.GenerateMaze(new Vector(20, 20), options,
                                                   out _);
            Assert.That(maze.MapAreas.Count, Is.GreaterThan(2));
        }

        public static IEnumerable<Type> GetAllGenerators() =>
            MazeTestHelper.GetAllGenerators();

        public static IEnumerable<AreaType> GetAllAreaTypes() =>
            MazeTestHelper.GetAllAreaTypes();

        public static IEnumerable<FillFactorOption> GetAllFillFactors() =>
            MazeTestHelper.GetAllFillFactors();
    }
}