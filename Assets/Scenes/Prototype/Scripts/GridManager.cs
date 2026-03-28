using System.Collections.Generic;
using UnityEngine;
 
namespace VTT.Grid
{
    /// <summary>
    /// Central singleton. Owns all chunks, exposes world↔cell conversion,
    /// and is the single source of truth for cell state.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }
 
        [Header("Grid Settings")]
        [Tooltip("Must match the Cell Size X value on your ProceduralGrid material.")]
        [SerializeField] private float cellSize = 1f;
 
        [Tooltip("Y height used for world-position queries (top of terrain).")]
        [SerializeField] private float worldY = 0f;
 
        // Chunk dictionary — key is (chunkX, chunkZ) packed into a long
        private readonly Dictionary<long, GridChunk> _chunks = new();
 
        public float CellSize => cellSize;
        public float WorldY   => worldY;
 
        // ── Events ─────────────────────────────────────────────────────────
        public event System.Action<GridCell> OnCellChanged;
 
        // ── Lifecycle ───────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
 
        // ── Chunk access ────────────────────────────────────────────────────
        private static long ChunkKey(int cx, int cz) =>
            ((long)(uint)cx << 32) | (uint)cz;
 
        private GridChunk GetOrCreateChunk(int chunkX, int chunkZ)
        {
            long key = ChunkKey(chunkX, chunkZ);
            if (!_chunks.TryGetValue(key, out GridChunk chunk))
            {
                chunk = new GridChunk(chunkX, chunkZ);
                _chunks[key] = chunk;
            }
            return chunk;
        }
 
        // ── Cell access ─────────────────────────────────────────────────────
 
        /// <summary>Returns the cell at grid coordinates (x, z). Creates chunk if needed.</summary>
        public GridCell GetCell(int x, int z)
        {
            int chunkX = Mathf.FloorToInt((float)x / GridChunk.CHUNK_SIZE);
            int chunkZ = Mathf.FloorToInt((float)z / GridChunk.CHUNK_SIZE);
            int localX = x - chunkX * GridChunk.CHUNK_SIZE;
            int localZ = z - chunkZ * GridChunk.CHUNK_SIZE;
 
            return GetOrCreateChunk(chunkX, chunkZ).GetLocalCell(localX, localZ);
        }
 
        /// <summary>Returns the cell at grid coords without creating a new chunk (returns null if not loaded).</summary>
        public GridCell GetCellSafe(int x, int z)
        {
            int chunkX = Mathf.FloorToInt((float)x / GridChunk.CHUNK_SIZE);
            int chunkZ = Mathf.FloorToInt((float)z / GridChunk.CHUNK_SIZE);
 
            if (!_chunks.TryGetValue(ChunkKey(chunkX, chunkZ), out GridChunk chunk))
                return null;
 
            int localX = x - chunkX * GridChunk.CHUNK_SIZE;
            int localZ = z - chunkZ * GridChunk.CHUNK_SIZE;
            return chunk.GetLocalCell(localX, localZ);
        }
 
        // ── Coordinate conversion ───────────────────────────────────────────
 
        /// <summary>Convert a world position to grid coordinates.</summary>
        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / cellSize),
                Mathf.FloorToInt(worldPos.z / cellSize)
            );
        }
 
        /// <summary>Convert grid coordinates to world-space centre of the cell.</summary>
        public Vector3 GridToWorld(int x, int z)
        {
            return new Vector3(x * cellSize + cellSize * 0.5f, worldY, z * cellSize + cellSize * 0.5f);
        }
 
        public Vector3 GridToWorld(Vector2Int coords) => GridToWorld(coords.x, coords.y);
 
        /// <summary>Snap a world position to the centre of its cell.</summary>
        public Vector3 SnapToGrid(Vector3 worldPos)
        {
            Vector2Int g = WorldToGrid(worldPos);
            return GridToWorld(g.x, g.y);
        }
 
        // ── State mutation ──────────────────────────────────────────────────
 
        public void PlaceObject(int x, int z, GameObject obj)
        {
            GridCell cell = GetCell(x, z);
            cell.Place(obj);
            OnCellChanged?.Invoke(cell);
        }
 
        public void RemoveObject(int x, int z)
        {
            GridCell cell = GetCell(x, z);
            cell.Remove();
            OnCellChanged?.Invoke(cell);
        }
 
        public void SetBlocked(int x, int z, bool blocked)
        {
            GridCell cell = GetCell(x, z);
            cell.SetBlocked(blocked);
            OnCellChanged?.Invoke(cell);
        }
 
        // ── Neighbour query (used by A*) ─────────────────────────────────────
 
        /// <summary>Returns walkable orthogonal + diagonal neighbours.</summary>
        public List<GridCell> GetNeighbours(GridCell cell, bool allowDiagonals = true)
        {
            var result = new List<GridCell>(8);
            int[] dx = { 0, 0, 1, -1, 1, 1, -1, -1 };
            int[] dz = { 1, -1, 0, 0, 1, -1, 1, -1 };
            int count = allowDiagonals ? 8 : 4;
 
            for (int i = 0; i < count; i++)
            {
                GridCell n = GetCellSafe(cell.X + dx[i], cell.Z + dz[i]);
                if (n != null && n.IsWalkable)
                    result.Add(n);
            }
            return result;
        }
    }
}
