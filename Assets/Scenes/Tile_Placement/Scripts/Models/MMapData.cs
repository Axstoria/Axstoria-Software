using System;
using System.Collections.Generic;

namespace HexGrid.Models
{
    [Serializable]
    public class PlacedTileDTO
    {
        public int prefabIndex;
        public int x, y, z;
        public float yRotation;
    }

    [Serializable]
    public class MapDataDTO
    {
        public List<PlacedTileDTO> tiles = new();
    }
}
