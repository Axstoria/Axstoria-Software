using System.Collections.Generic;
using UnityEngine;

namespace VTT.Grid
{
    /// <summary>
    /// Singleton. Owns all chunks and is the single source of truth for cell state.
    /// Chunk key packs (chunkX, chunkZ) into a long for O(1) lookup.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Grid Settings")]
        [Tooltip("Must match Cell Size X on the ProceduralGrid material.")]
        [SerializeField] private float cellSize = 1f;

        [Tooltip("Y position of the top of the terrain.")]
        [SerializeField] private float worldY = 0f;

        public float CellSize => cellSize;
        public float WorldY   => worldY;

        /// <summary>
        /// Called by TerrainBuilder after each generation to keep surface Y in sync
        /// </summary>
        public void SetSurface(float y) => worldY = y;

        public event System.Action<GridCell> OnCellChanged;

        private readonly Dictionary<long, GridChunk> _chunks = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── Cell access ───────────────────────────────────────────────────────

        /// <summary>Returns the cell, creating its chunk if needed.</summary>
        public GridCell GetCell(int x, int z)
        {
            ChunkCoords(x, z, out int cx, out int cz, out int lx, out int lz);
            return GetOrCreateChunk(cx, cz).GetLocalCell(lx, lz);
        }

        /// <summary>Returns the cell without creating a chunk. Returns null if not loaded.</summary>
        public GridCell GetCellSafe(int x, int z)
        {
            ChunkCoords(x, z, out int cx, out int cz, out int lx, out int lz);
            return _chunks.TryGetValue(ChunkKey(cx, cz), out var chunk)
                ? chunk.GetLocalCell(lx, lz) : null;
        }

        // ── Coordinate conversion ─────────────────────────────────────────────

        public Vector2Int WorldToGrid(Vector3 worldPos) => new(
            Mathf.FloorToInt(worldPos.x / cellSize),
            Mathf.FloorToInt(worldPos.z / cellSize));

        public Vector3 GridToWorld(int x, int z) =>
            new(x * cellSize + cellSize * 0.5f, worldY, z * cellSize + cellSize * 0.5f);

        public Vector3 GridToWorld(Vector2Int coords) => GridToWorld(coords.x, coords.y);

        public Vector3 SnapToGrid(Vector3 worldPos)
        {
            var g = WorldToGrid(worldPos);
            return GridToWorld(g.x, g.y);
        }

        // ── State mutation ────────────────────────────────────────────────────

        public void PlaceObject(int x, int z, GameObject obj)
        {
            var cell = GetCell(x, z);
            cell.Place(obj);
            OnCellChanged?.Invoke(cell);
        }

        public void RemoveObject(int x, int z)
        {
            var cell = GetCell(x, z);
            cell.Remove();
            OnCellChanged?.Invoke(cell);
        }

        public void SetBlocked(int x, int z, bool blocked)
        {
            var cell = GetCell(x, z);
            cell.SetBlocked(blocked);
            OnCellChanged?.Invoke(cell);
        }

        // ── A* neighbour query ────────────────────────────────────────────────

        public List<GridCell> GetNeighbours(GridCell cell, bool allowDiagonals = true)
        {
            var result = new List<GridCell>(8);
            int[] dx = { 0, 0, 1, -1, 1, 1, -1, -1 };
            int[] dz = { 1, -1, 0, 0, 1, -1, 1, -1 };
            int count = allowDiagonals ? 8 : 4;

            for (int i = 0; i < count; i++)
            {
                var n = GetCellSafe(cell.X + dx[i], cell.Z + dz[i]);
                if (n != null && n.IsWalkable) result.Add(n);
            }
            return result;
        }

        // ── Internals ─────────────────────────────────────────────────────────

        private static long ChunkKey(int cx, int cz) => ((long)(uint)cx << 32) | (uint)cz;

        private GridChunk GetOrCreateChunk(int cx, int cz)
        {
            long key = ChunkKey(cx, cz);
            if (!_chunks.TryGetValue(key, out var chunk))
                _chunks[key] = chunk = new GridChunk(cx, cz);
            return chunk;
        }

        private static void ChunkCoords(int x, int z,
            out int cx, out int cz, out int lx, out int lz)
        {
            cx = Mathf.FloorToInt((float)x / GridChunk.CHUNK_SIZE);
            cz = Mathf.FloorToInt((float)z / GridChunk.CHUNK_SIZE);
            lx = x - cx * GridChunk.CHUNK_SIZE;
            lz = z - cz * GridChunk.CHUNK_SIZE;
        }
    }
}
