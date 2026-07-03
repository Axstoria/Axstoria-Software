using NUnit.Framework;
using Grid.Domain;

namespace Tests
{
    public class GridCellTests
    {
        [Test]
        public void NewCell_IsEmpty()
        {
            var cell = new GridCell(0, 0);
            Assert.AreEqual(CellState.Empty, cell.State);
            Assert.IsTrue(cell.IsEmpty);
        }

        [Test]
        public void NewCell_IsWalkable()
        {
            var cell = new GridCell(0, 0);
            Assert.IsTrue(cell.IsWalkable);
        }

        [Test]
        public void NewCell_HasNoOccupant()
        {
            var cell = new GridCell(3, 5);
            Assert.IsNull(cell.OccupantId);
        }

        [Test]
        public void Place_SetsOccupiedStateAndId()
        {
            var cell = new GridCell(3, 5);
            cell.Place("unit-1");
            Assert.AreEqual(CellState.Occupied, cell.State);
            Assert.AreEqual("unit-1", cell.OccupantId);
            Assert.IsFalse(cell.IsEmpty);
        }

        [Test]
        public void Place_CellIsStillWalkable()
        {
            var cell = new GridCell(0, 0);
            cell.Place("unit-1");
            Assert.IsTrue(cell.IsWalkable);
        }

        [Test]
        public void Remove_AfterPlace_ResetsToEmpty()
        {
            var cell = new GridCell(3, 5);
            cell.Place("unit-1");
            cell.Remove();
            Assert.AreEqual(CellState.Empty, cell.State);
            Assert.IsNull(cell.OccupantId);
            Assert.IsTrue(cell.IsEmpty);
        }

        [Test]
        public void SetBlocked_True_SetsBlockedAndClearsOccupant()
        {
            var cell = new GridCell(0, 0);
            cell.Place("unit-1");
            cell.SetBlocked(true);
            Assert.AreEqual(CellState.Blocked, cell.State);
            Assert.IsNull(cell.OccupantId);
            Assert.IsFalse(cell.IsWalkable);
            Assert.IsFalse(cell.IsEmpty);
        }

        [Test]
        public void SetBlocked_False_WhenBlocked_ResetsToEmpty()
        {
            var cell = new GridCell(0, 0);
            cell.SetBlocked(true);
            cell.SetBlocked(false);
            Assert.AreEqual(CellState.Empty, cell.State);
            Assert.IsTrue(cell.IsWalkable);
            Assert.IsTrue(cell.IsEmpty);
        }

        [Test]
        public void SetBlocked_False_WhenOccupied_DoesNothing()
        {
            var cell = new GridCell(0, 0);
            cell.Place("unit-1");
            cell.SetBlocked(false);
            Assert.AreEqual(CellState.Occupied, cell.State);
            Assert.AreEqual("unit-1", cell.OccupantId);
        }

        [Test]
        public void SetBlocked_False_WhenEmpty_DoesNothing()
        {
            var cell = new GridCell(0, 0);
            cell.SetBlocked(false);
            Assert.AreEqual(CellState.Empty, cell.State);
        }

        [Test]
        public void Cell_StoresCoordinates()
        {
            var cell = new GridCell(7, 12);
            Assert.AreEqual(7, cell.X);
            Assert.AreEqual(12, cell.Z);
        }
    }
}
