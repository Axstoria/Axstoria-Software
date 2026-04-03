namespace Domain
{
    public class GridChunk
    {
        public const int CHUNK_SIZE = 16;

        public int ChunkX { get; }
        public int ChunkZ { get; }

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

        public GridCell GetCell(int localX, int localZ)
        {
            if (localX < 0 || localX >= CHUNK_SIZE || localZ < 0 || localZ >= CHUNK_SIZE)
                return null;
            return _cells[localX, localZ];
        }
    }
}
