using System;
using System.Collections.Generic;

namespace Domain
{
    /// <summary>
    /// Represents a grid-based spatial system for managing occupancy and movement within the game world.
    /// </summary>
    public class Grid
    {
        public float CellSize { get; set; }
        public float SurfaceY { get; set; }

        public event Action<GridCell> OnCellChanged;

        private readonly Dictionary<long, GridChunk> _chunks = new();
        private readonly Dictionary<string, PlacementRecord> _placements = new();

        // ── Cell access ───────────────────────────────────────────────────────

        public GridCell GetCell(int x, int z)
        {
            ChunkCoords(x, z, out int cx, out int cz, out int lx, out int lz);
            return GetOrCreateChunk(cx, cz).GetCell(lx, lz);
        }

        public GridCell GetCellSafe(int x, int z)
        {
            ChunkCoords(x, z, out int cx, out int cz, out int lx, out int lz);
            return _chunks.TryGetValue(ChunkKey(cx, cz), out var chunk)
                ? chunk.GetCell(lx, lz) : null;
        }

        // ── Coordinate conversion ─────────────────────────────────────────────

        public (int x, int z) WorldToGrid(float worldX, float worldZ) => (
            (int)System.Math.Floor(worldX / CellSize),
            (int)System.Math.Floor(worldZ / CellSize)
        );

        public (float x, float y, float z) GridToWorld(int x, int z) => (
            x * CellSize + CellSize * 0.5f,
            SurfaceY,
            z * CellSize + CellSize * 0.5f
        );

        // ── Placement ─────────────────────────────────────────────────────────

        public bool CanPlace(List<GridCoord> footprint, GridCoord origin)
        {
            foreach (var offset in footprint)
            {
                var cell = GetCell(origin.X + offset.X, origin.Z + offset.Z);
                if (!cell.IsEmpty) return false;
            }
            return true;
        }

        public void PlaceOccupant(string id, List<GridCoord> footprint, GridCoord origin)
        {
            foreach (var offset in footprint)
            {
                var cell = GetCell(origin.X + offset.X, origin.Z + offset.Z);
                cell.Place(id);
                OnCellChanged?.Invoke(cell);
            }
            _placements[id] = new PlacementRecord(origin, footprint);
        }

        public void RemoveOccupant(string id)
        {
            if (!_placements.TryGetValue(id, out var record)) return;
            foreach (var offset in record.Footprint)
            {
                var cell = GetCell(record.Origin.X + offset.X, record.Origin.Z + offset.Z);
                cell.Remove();
                OnCellChanged?.Invoke(cell);
            }
            _placements.Remove(id);
        }

        public bool TryGetPlacement(string id, out GridCoord origin, out List<GridCoord> footprint)
        {
            if (_placements.TryGetValue(id, out var record))
            {
                origin   = record.Origin;
                footprint = record.Footprint;
                return true;
            }
            origin    = GridCoord.Zero;
            footprint = null;
            return false;
        }

        public void ClearAllOccupants()
        {
            foreach (var id in new List<string>(_placements.Keys))
                RemoveOccupant(id);
        }

        /// <summary>
        /// Rebuilds the occupancy grid based on the current positions of scene objects. This is for when loading a map or after bulk changes to objects.
        /// </summary>
        /// <param name="objects"></param>
        public void RebuildOccupancy(IEnumerable<SceneObject> objects)
        {
            ClearAllOccupants();
            var singleCell = new List<GridCoord> { GridCoord.Zero };
            foreach (var obj in objects)
            {
                if (obj.Transform == null) continue;
                var (gx, gz) = WorldToGrid(obj.Transform.Position.x, obj.Transform.Position.z);
                PlaceOccupant(obj.Id, singleCell, new GridCoord(gx, gz));
            }
        }

        // ── Neighbours (A*) ───────────────────────────────────────────────────

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
            cx = (int)System.Math.Floor((double)x / GridChunk.CHUNK_SIZE);
            cz = (int)System.Math.Floor((double)z / GridChunk.CHUNK_SIZE);
            lx = x - cx * GridChunk.CHUNK_SIZE;
            lz = z - cz * GridChunk.CHUNK_SIZE;
        }

        private readonly struct PlacementRecord
        {
            public GridCoord        Origin   { get; }
            public List<GridCoord>  Footprint { get; }
            public PlacementRecord(GridCoord origin, List<GridCoord> footprint)
            {
                Origin    = origin;
                Footprint = footprint;
            }
        }
    }
}
