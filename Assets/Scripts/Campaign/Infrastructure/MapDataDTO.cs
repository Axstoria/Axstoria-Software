using System;
using System.Collections.Generic;

namespace Campaign.Infrastructure
{
    [Serializable]
    public abstract class MetadataValueDTO { }

    [Serializable]
    public class NoteValueDTO : MetadataValueDTO
    {
        public string Text { get; set; }
    }

    [Serializable]
    public class TagValueDTO : MetadataValueDTO
    {
        public string Id       { get; set; }
        public string Name     { get; set; }
        public string HexColor { get; set; } = "#FFFFFF";
    }

    [Serializable]
    public class SheetValueDTO : MetadataValueDTO
    {
        // TODO: Implement this class when sheets are done
    }

    [Serializable]
    public class MetadataEntryDTO
    {
        public string EntryType;
        public MetadataValueDTO EntryValue;
    }

    [Serializable]
    public class MapDataDTO
    {
        public string version = "0.1";
        public string savedAt;
        public string mapId;
        public string mapName;

        public TerrainDTO             terrain;
        public List<SceneObjectDTO>   objects;
        public List<MetadataEntryDTO> metadata;
        public List<PlayerDTO>        players;
    }

    [Serializable]
    public class PlayerDTO
    {
        public string id;
        public string name;
        public bool   isGameMaster;
        public string pawnId;
        public List<MetadataEntryDTO> metadata;
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
        public bool   isPawn;
        public List<MetadataEntryDTO> metadata;

        public float posX, posY, posZ;
        public float rotX, rotY, rotZ, rotW;
        public float scaleX, scaleY, scaleZ;
    }
}
