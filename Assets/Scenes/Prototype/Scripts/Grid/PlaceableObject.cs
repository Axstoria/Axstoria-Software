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

        /// <summary>
        /// Recomputes FootprintOffsets from the object's world-space renderer bounds,
        /// centered around the origin cell. Falls back to single-cell if no renderers found.
        /// Call this after instantiation and before placement.
        /// </summary>
        public void ComputeFootprint(float cellSize = 1f)
        {
            Renderer[]      renderers = GetComponentsInChildren<Renderer>();
            Bounds          bounds    = new Bounds();
            int             cellsX    = 1;
            int             cellsZ    = 1;
            int             offX      = 0;
            int             offZ      = 0;

            if (renderers.Length == 0) return;

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            cellsX = Mathf.Max(1, Mathf.RoundToInt(bounds.size.x / cellSize));
            cellsZ = Mathf.Max(1, Mathf.RoundToInt(bounds.size.z / cellSize));
            offX   = cellsX / 2;
            offZ   = cellsZ / 2;

            FootprintOffsets = new List<Vector2Int>(cellsX * cellsZ);
            for (int x = 0; x < cellsX; x++)
            for (int z = 0; z < cellsZ; z++)
                FootprintOffsets.Add(new Vector2Int(x - offX, z - offZ));
        }
    }
}
