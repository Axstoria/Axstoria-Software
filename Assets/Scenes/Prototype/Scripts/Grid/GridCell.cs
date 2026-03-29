using UnityEngine;

namespace VTT.Grid
{
    public enum CellState
    {
        Empty,
        Occupied,
        Blocked   // impassable terrain, walls, etc.
    }

    [System.Serializable]
    public class GridCell
    {
        // Grid coordinates (can be negative for infinite world)
        public int X { get; private set; }
        public int Z { get; private set; }

        public CellState State { get; private set; } = CellState.Empty;
        public GameObject Occupant { get; private set; }

        // Pathfinding weights (1 = normal, higher = harder to traverse)
        public float MovementCost { get; set; } = 1f;

        public GridCell(int x, int z)
        {
            X = x;
            Z = z;
        }

        public bool IsWalkable => State != CellState.Blocked;
        public bool IsEmpty    => State == CellState.Empty;

        public void Place(GameObject obj)
        {
            Occupant = obj;
            State    = CellState.Occupied;
        }

        public void Remove()
        {
            Occupant = null;
            State    = CellState.Empty;
        }

        public void SetBlocked(bool blocked)
        {
            if (blocked)
            {
                State    = CellState.Blocked;
                Occupant = null;
            }
            else if (State == CellState.Blocked)
            {
                State = CellState.Empty;
            }
        }

        // World-space centre of this cell
        public Vector3 WorldPosition(float cellSize, float worldY = 0f)
        {
            return new Vector3(X * cellSize + cellSize * 0.5f, worldY, Z * cellSize + cellSize * 0.5f);
        }

        public override string ToString() => $"Cell({X},{Z}) [{State}]";
    }
}
