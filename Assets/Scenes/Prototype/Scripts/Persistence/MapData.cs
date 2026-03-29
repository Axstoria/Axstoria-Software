using System;
using System.Collections.Generic;
using UnityEngine;

namespace VTT.Persistence
{
    // ── Root ──────────────────────────────────────────────────────────────────

    [Serializable]
    public class MapData
    {
        public string version   = "1.0";
        public string savedAt   = "";
        public string mapName   = "Unnamed Map";

        public TerrainData      terrain = new();
        public GridShaderData   grid    = new();
        public List<DecorObjectData> objects = new();
    }

    // ── Terrain ───────────────────────────────────────────────────────────────

    [Serializable]
    public class TerrainData
    {
        public int    width     = 20;
        public int    depth     = 20;
        public int    thickness = 3;
        public float  baseHeight = 0f;
        public SColor terrainColor = new(0.6f, 0.4f, 0.2f);
    }

    // ── Grid shader ───────────────────────────────────────────────────────────

    [Serializable]
    public class GridShaderData
    {
        public float  cellSizeX     = 1f;
        public float  cellSizeY     = 1f;
        public float  gridThickness = 0.05f;
        public SColor bgColor       = new(0.6f, 0.4f, 0.2f);
        public SColor gridColor     = new(1f,   1f,   1f);
        public float  gridOpacity   = 0.24f;
        public bool   transparent   = true;
    }

    // ── Placed objects ────────────────────────────────────────────────────────

    [Serializable]
    public class DecorObjectData
    {
        /// <summary>Stable identifier so load can match existing objects on re-save.</summary>
        public string guid        = "";

        public string displayName = "";
        public string category    = "Uncategorized";

        /// <summary>
        /// Name of the prefab in PrefabRegistry (or VTTPanelUI categories).
        /// Used to re-instantiate the object on load.
        /// </summary>
        public string prefabName  = "";

        /// <summary>True if this object was imported at runtime (glTF/GLB).</summary>
        public bool   isImported  = false;

        /// <summary>Absolute file path of the source glTF/GLB — used to re-import on load.</summary>
        public string importPath  = "";

        public SVector3 position = new();
        public SVector3 rotation = new();   // Euler angles
        public SVector3 scale    = new(1, 1, 1);

        public int gridCellX = 0;
        public int gridCellZ = 0;
    }

    // ── Serializable primitives (Unity types are not JSON-serializable) ────────

    [Serializable]
    public class SColor
    {
        public float r, g, b, a;

        public SColor() { r = g = b = 0; a = 1; }
        public SColor(float r, float g, float b, float a = 1f)
        { this.r = r; this.g = g; this.b = b; this.a = a; }

        public static SColor From(Color c) => new(c.r, c.g, c.b, c.a);
        public Color ToColor() => new(r, g, b, a);
    }

    [Serializable]
    public class SVector3
    {
        public float x, y, z;

        public SVector3() { }
        public SVector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }

        public static SVector3 From(Vector3 v) => new(v.x, v.y, v.z);
        public Vector3 ToVector3() => new(x, y, z);
    }
}
