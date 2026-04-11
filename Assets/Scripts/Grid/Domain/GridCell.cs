namespace Domain
{
    public enum CellState { Empty, Occupied, Blocked }

    /// <summary>
    /// Represents a single cell in the grid, which can be empty, occupied by a token, or blocked by a structure.
    /// </summary>
    public class GridCell
    {
        public int X { get; }
        public int Z { get; }
        public CellState State { get; private set; }
        public string OccupantId { get; private set; }

        public bool IsEmpty => State == CellState.Empty;
        public bool IsWalkable => State != CellState.Blocked;

        public GridCell(int x, int z) { X = x; Z = z; }

        public void Place(string occupantId)
        {
            OccupantId = occupantId;
            State = CellState.Occupied;
        }

        public void Remove()
        {
            OccupantId = null;
            State = CellState.Empty;
        }

        public void SetBlocked(bool blocked)
        {
            if (blocked)
            {
                OccupantId = null;
                State = CellState.Blocked;
            }
            else if (State == CellState.Blocked)
            {
                State = CellState.Empty;
            }
        }
    }
}
