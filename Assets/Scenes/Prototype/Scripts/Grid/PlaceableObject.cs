using System.Collections.Generic;
using UnityEngine;

namespace VTT.Grid
{
    /// <summary>
    /// Marks a token or prop as grid-placeable and defines its cell footprint.
    /// </summary>
    public class PlaceableObject : MonoBehaviour
    {
        [Tooltip("Cell offsets relative to the origin cell.")]
        public List<Vector2Int> FootprintOffsets = new() { Vector2Int.zero };

        public bool       IsPlaced       { get; private set; }
        public Vector2Int OccupiedOrigin { get; set; }

        public virtual void OnPlaced()  { IsPlaced = true; }
        public virtual void OnRemoved() { IsPlaced = false; }
    }
}
