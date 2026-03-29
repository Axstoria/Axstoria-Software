using System.Collections.Generic;
using UnityEngine;

namespace VTT.Grid
{
    /// <summary>
    /// Attach to any VTT token or prop to make it placeable on the grid.
    /// </summary>
    public class PlaceableObject : MonoBehaviour
    {
        [Tooltip("Cell offsets relative to origin. (0,0) = origin cell.")]
        public List<Vector2Int> FootprintOffsets = new() { Vector2Int.zero };

        public bool       IsPlaced       { get; private set; }
        public Vector2Int OccupiedOrigin { get; set; }

        public virtual void OnPlaced()  { IsPlaced = true; }
        public virtual void OnRemoved() { IsPlaced = false; }
    }
}
