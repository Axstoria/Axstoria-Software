using System;
using System.Collections.Generic;

namespace Campaign.Infrastructure
{
    // JsonUtility cannot serialize/deserialize through a polymorphic (abstract-typed)
    // field — it silently drops it, producing an entry with no value at all on load.
    // MetadataEntryDTO is deliberately flat (one concrete field per known entry type)
    // instead of wrapping an abstract "value" type, so every field is a plain,
    // JsonUtility-serializable member.
    [Serializable]
    public class MetadataEntryDTO
    {
        public string entryType;
        public string noteText;
        public string tagId;
        public string tagName;
        public string tagHexColor;
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
        public LightDTO               light;
        public string                 skyboxName;
        public CameraSettingsDTO      cameraSettings;
    }

    [Serializable]
    public class LightDTO
    {
        public float intensity;
        public float colorR, colorG, colorB;
        public float pitch, yaw;
        public float shadowStrength;
        public float ambientIntensity;
        public float ambientColorR, ambientColorG, ambientColorB;
    }

    [Serializable]
    public class CameraSettingsDTO
    {
        public float orbitSensitivity;
        public float minPitch, maxPitch;
        public float orbitSmoothing;
        public float zoomSpeed;
        public float zoomSmoothing;
        public float minZoomDistance, maxZoomDistance;
        public float panSensitivity;
        public float panSmoothing;
    }

    [Serializable]
    public class PlayerDTO
    {
        public string id;
        public string name;
        public bool   isGameMaster;
        public string pawnId;
        public string hexColor;
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
