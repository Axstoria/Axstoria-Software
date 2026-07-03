using System.Collections.Generic;
using NUnit.Framework;
using Grid.Domain;

namespace Tests
{
    public class GridTests
    {
        private Grid.Domain.Grid _grid;

        [SetUp]
        public void Setup()
        {
            _grid = new Grid.Domain.Grid { CellSize = 1f, SurfaceY = 0f };
        }

        // ── Coordinate conversion ──────────────────────────────────────────────

        [Test]
        public void WorldToGrid_ConvertsCorrectly()
        {
            _grid.CellSize = 2f;
            var (gx, gz) = _grid.WorldToGrid(5f, 3f);
            Assert.AreEqual(2, gx); // floor(5/2) = 2
            Assert.AreEqual(1, gz); // floor(3/2) = 1
        }

        [Test]
        public void WorldToGrid_NegativeCoords_UsesFloor()
        {
            _grid.CellSize = 1f;
            var (gx, gz) = _grid.WorldToGrid(-0.1f, -1.9f);
            Assert.AreEqual(-1, gx); // floor(-0.1) = -1
            Assert.AreEqual(-2, gz); // floor(-1.9) = -2
        }

        [Test]
        public void WorldToGrid_ExactCellBoundary_MapsToNextCell()
        {
            _grid.CellSize = 2f;
            var (gx, gz) = _grid.WorldToGrid(4f, 6f);
            Assert.AreEqual(2, gx); // floor(4/2) = 2
            Assert.AreEqual(3, gz); // floor(6/2) = 3
        }

        [Test]
        public void GridToWorld_ReturnsCellCenter()
        {
            _grid.CellSize = 2f;
            _grid.SurfaceY = 5f;
            var (wx, wy, wz) = _grid.GridToWorld(3, 1);
            Assert.AreEqual(7f, wx); // 3*2 + 1 = 7
            Assert.AreEqual(5f, wy);
            Assert.AreEqual(3f, wz); // 1*2 + 1 = 3
        }

        [Test]
        public void GridToWorld_UsesConfiguredSurfaceY()
        {
            _grid.SurfaceY = 10f;
            var (_, wy, _) = _grid.GridToWorld(0, 0);
            Assert.AreEqual(10f, wy);
        }

        // ── Cell access ───────────────────────────────────────────────────────

        [Test]
        public void GetCell_CreatesAndReturnsCell()
        {
            var cell = _grid.GetCell(0, 0);
            Assert.IsNotNull(cell);
            Assert.AreEqual(0, cell.X);
            Assert.AreEqual(0, cell.Z);
        }

        [Test]
        public void GetCell_SameCoord_ReturnsSameInstance()
        {
            var a = _grid.GetCell(3, 4);
            var b = _grid.GetCell(3, 4);
            Assert.AreSame(a, b);
        }

        [Test]
        public void GetCellSafe_NonExistentChunk_ReturnsNull()
        {
            var cell = _grid.GetCellSafe(100, 100);
            Assert.IsNull(cell);
        }

        [Test]
        public void GetCellSafe_AfterGetCell_ReturnsCell()
        {
            _grid.GetCell(5, 5);
            var cell = _grid.GetCellSafe(5, 5);
            Assert.IsNotNull(cell);
        }

        // ── Placement ─────────────────────────────────────────────────────────

        [Test]
        public void CanPlace_OnEmptyCells_ReturnsTrue()
        {
            var footprint = new List<GridCoord> { GridCoord.Zero };
            Assert.IsTrue(_grid.CanPlace(footprint, new GridCoord(0, 0)));
        }

        [Test]
        public void CanPlace_OnOccupiedCell_ReturnsFalse()
        {
            var footprint = new List<GridCoord> { GridCoord.Zero };
            _grid.PlaceOccupant("a", footprint, new GridCoord(0, 0));
            Assert.IsFalse(_grid.CanPlace(footprint, new GridCoord(0, 0)));
        }

        [Test]
        public void CanPlace_OnBlockedCell_ReturnsFalse()
        {
            _grid.GetCell(2, 2).SetBlocked(true);
            var footprint = new List<GridCoord> { GridCoord.Zero };
            Assert.IsFalse(_grid.CanPlace(footprint, new GridCoord(2, 2)));
        }

        [Test]
        public void PlaceOccupant_CellBecomesOccupied()
        {
            var footprint = new List<GridCoord> { GridCoord.Zero };
            _grid.PlaceOccupant("unit-1", footprint, new GridCoord(2, 3));
            var cell = _grid.GetCell(2, 3);
            Assert.AreEqual(CellState.Occupied, cell.State);
            Assert.AreEqual("unit-1", cell.OccupantId);
        }

        [Test]
        public void PlaceOccupant_MultiCell_AllCellsOccupied()
        {
            var footprint = new List<GridCoord>
            {
                new(0, 0), new(1, 0), new(0, 1)
            };
            _grid.PlaceOccupant("big-unit", footprint, new GridCoord(0, 0));
            Assert.AreEqual(CellState.Occupied, _grid.GetCell(0, 0).State);
            Assert.AreEqual(CellState.Occupied, _grid.GetCell(1, 0).State);
            Assert.AreEqual(CellState.Occupied, _grid.GetCell(0, 1).State);
        }

        [Test]
        public void PlaceOccupant_FiresOnCellChanged()
        {
            var changedCells = new List<GridCell>();
            _grid.OnCellChanged += cell => changedCells.Add(cell);
            var footprint = new List<GridCoord> { GridCoord.Zero };
            _grid.PlaceOccupant("unit-1", footprint, new GridCoord(0, 0));
            Assert.AreEqual(1, changedCells.Count);
        }

        [Test]
        public void RemoveOccupant_CellBecomesEmpty()
        {
            var footprint = new List<GridCoord> { GridCoord.Zero };
            _grid.PlaceOccupant("unit-1", footprint, new GridCoord(0, 0));
            _grid.RemoveOccupant("unit-1");
            Assert.IsTrue(_grid.GetCell(0, 0).IsEmpty);
        }

        [Test]
        public void RemoveOccupant_FiresOnCellChangedForEachCell()
        {
            var footprint = new List<GridCoord> { new(0, 0), new(1, 0) };
            _grid.PlaceOccupant("unit-1", footprint, new GridCoord(0, 0));
            var changedCells = new List<GridCell>();
            _grid.OnCellChanged += cell => changedCells.Add(cell);
            _grid.RemoveOccupant("unit-1");
            Assert.AreEqual(2, changedCells.Count);
        }

        [Test]
        public void RemoveOccupant_UnknownId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _grid.RemoveOccupant("ghost"));
        }

        [Test]
        public void TryGetPlacement_AfterPlace_ReturnsPlacementData()
        {
            var origin = new GridCoord(5, 5);
            var footprint = new List<GridCoord> { GridCoord.Zero };
            _grid.PlaceOccupant("unit-1", footprint, origin);

            bool found = _grid.TryGetPlacement("unit-1", out var resultOrigin, out _);
            Assert.IsTrue(found);
            Assert.AreEqual(origin, resultOrigin);
        }

        [Test]
        public void TryGetPlacement_AfterRemove_ReturnsFalse()
        {
            var footprint = new List<GridCoord> { GridCoord.Zero };
            _grid.PlaceOccupant("unit-1", footprint, new GridCoord(0, 0));
            _grid.RemoveOccupant("unit-1");

            bool found = _grid.TryGetPlacement("unit-1", out _, out _);
            Assert.IsFalse(found);
        }

        [Test]
        public void TryGetPlacement_UnknownId_ReturnsFalse()
        {
            bool found = _grid.TryGetPlacement("ghost", out _, out _);
            Assert.IsFalse(found);
        }

        [Test]
        public void ClearAllOccupants_EmptiesAllCells()
        {
            var fp = new List<GridCoord> { GridCoord.Zero };
            _grid.PlaceOccupant("a", fp, new GridCoord(0, 0));
            _grid.PlaceOccupant("b", fp, new GridCoord(5, 5));
            _grid.ClearAllOccupants();
            Assert.IsTrue(_grid.GetCell(0, 0).IsEmpty);
            Assert.IsTrue(_grid.GetCell(5, 5).IsEmpty);
        }

        [Test]
        public void ClearAllOccupants_MakesCanPlaceReturnTrue()
        {
            var fp = new List<GridCoord> { GridCoord.Zero };
            _grid.PlaceOccupant("a", fp, new GridCoord(0, 0));
            _grid.ClearAllOccupants();
            Assert.IsTrue(_grid.CanPlace(fp, new GridCoord(0, 0)));
        }

        // ── Neighbours ────────────────────────────────────────────────────────

        [Test]
        public void GetNeighbours_4Dir_ReturnsUpTo4()
        {
            var cell = _grid.GetCell(5, 5);
            var neighbours = _grid.GetNeighbours(cell, allowDiagonals: false);
            Assert.AreEqual(4, neighbours.Count);
        }

        [Test]
        public void GetNeighbours_8Dir_ReturnsUpTo8()
        {
            var cell = _grid.GetCell(5, 5);
            var neighbours = _grid.GetNeighbours(cell, allowDiagonals: true);
            Assert.AreEqual(8, neighbours.Count);
        }

        [Test]
        public void GetNeighbours_BlockedNeighbour_IsExcluded()
        {
            _grid.GetCell(5, 6).SetBlocked(true);
            var cell = _grid.GetCell(5, 5);
            var neighbours = _grid.GetNeighbours(cell, allowDiagonals: false);
            Assert.AreEqual(3, neighbours.Count);
        }

        [Test]
        public void GetNeighbours_OccupiedNeighbour_IsIncluded()
        {
            var fp = new List<GridCoord> { GridCoord.Zero };
            _grid.PlaceOccupant("obstacle", fp, new GridCoord(5, 6));
            var cell = _grid.GetCell(5, 5);
            var neighbours = _grid.GetNeighbours(cell, allowDiagonals: false);
            Assert.AreEqual(4, neighbours.Count);
        }
    }
}
