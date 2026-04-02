using System.Collections.Generic;

namespace VTT.Grid
{
    /// <summary>
    /// Fixed-size block of GridCells. Created on demand as the world is explored.
    /// </summary>
    public class GridChunk
    {
        public const int CHUNK_SIZE = 16; // Maybe change chunk size later, for dynamic cell sizes or something.

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

        /// <summary>
        /// Returns null if local coords are out of bounds
        /// </summary>
        public GridCell GetLocalCell(int localX, int localZ)
        {
            if (localX < 0 || localX >= CHUNK_SIZE || localZ < 0 || localZ >= CHUNK_SIZE)
                return null;
            return _cells[localX, localZ];
        }

        public IEnumerable<GridCell> AllCells()
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            for (int z = 0; z < CHUNK_SIZE; z++)
                yield return _cells[x, z];
        }
    }
}
