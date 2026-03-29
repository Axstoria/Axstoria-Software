using System.Collections.Generic;
using UnityEngine;

namespace VTT.Grid
{
    /// <summary>
    /// A fixed-size block of GridCells. Chunks are created on demand
    /// as players explore or place objects in new areas.
    /// </summary>
    public class GridChunk
    {
        public const int CHUNK_SIZE = 16; // cells per side

        // Chunk coordinate (not world coordinate)
        public int ChunkX { get; private set; }
        public int ChunkZ { get; private set; }

        private readonly GridCell[,] _cells = new GridCell[CHUNK_SIZE, CHUNK_SIZE];

        public GridChunk(int chunkX, int chunkZ)
        {
            ChunkX = chunkX;
            ChunkZ = chunkZ;

            int originX = chunkX * CHUNK_SIZE;
            int originZ = chunkZ * CHUNK_SIZE;

            for (int x = 0; x < CHUNK_SIZE; x++)
            for (int z = 0; z < CHUNK_SIZE; z++)
                _cells[x, z] = new GridCell(originX + x, originZ + z);
        }

        /// <summary>Get a cell by its LOCAL index within this chunk.</summary>
        public GridCell GetLocalCell(int localX, int localZ)
        {
            if (localX < 0 || localX >= CHUNK_SIZE || localZ < 0 || localZ >= CHUNK_SIZE)
                return null;
            return _cells[localX, localZ];
        }

        /// <summary>Iterate all cells in this chunk.</summary>
        public IEnumerable<GridCell> AllCells()
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            for (int z = 0; z < CHUNK_SIZE; z++)
                yield return _cells[x, z];
        }
    }
}
