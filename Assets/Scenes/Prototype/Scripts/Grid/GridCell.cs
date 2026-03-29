using UnityEngine;

namespace VTT.Grid
{
    public enum CellState { Empty, Occupied, Blocked }

    [System.Serializable]
    public class GridCell
    {
        public int X { get; private set; }
        public int Z { get; private set; }

        public CellState  State     { get; private set; } = CellState.Empty;
        public GameObject Occupant  { get; private set; }

        public bool IsWalkable => State != CellState.Blocked;
        public bool IsEmpty    => State == CellState.Empty;

        public GridCell(int x, int z) { X = x; Z = z; }

        public void Place(GameObject obj) { Occupant = obj; State = CellState.Occupied; }

        public void Remove() { Occupant = null; State = CellState.Empty; }

        public void SetBlocked(bool blocked)
        {
            if (blocked)      { State = CellState.Blocked; Occupant = null; }
            else if (State == CellState.Blocked) State = CellState.Empty;
        }

        public Vector3 WorldPosition(float cellSize, float worldY = 0f) =>
            new(X * cellSize + cellSize * 0.5f, worldY, Z * cellSize + cellSize * 0.5f);

        public override string ToString() => $"Cell({X},{Z}) [{State}]";
    }
}
