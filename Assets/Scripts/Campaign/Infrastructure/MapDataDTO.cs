using System;
using System.Collections.Generic;

namespace Campaign.Infrastructure
{
    [Serializable]
    public class MapDataDTO
    {
        public string version = "0.1";
        public string savedAt;
        public string mapId;
        public string mapName;

        public TerrainDTO           terrain;
        public List<SceneObjectDTO> objects;
    }

    [Serializable]
    public class TerrainDTO
    {
        public int   width;
        public int   depth;
        public int   thickness;
        public float height;
        public float colorR, colorG, colorB, colorA;
        public float cellSize;
        public float surfaceY;
    }

    [Serializable]
    public class SceneObjectDTO
    {
        public string id;
        public string displayName;
        public string category;
        public string modelPath;
        public bool   isImported;
        public string importPath;

        public float posX, posY, posZ;
        public float rotX, rotY, rotZ, rotW;
        public float scaleX, scaleY, scaleZ;
    }
}
