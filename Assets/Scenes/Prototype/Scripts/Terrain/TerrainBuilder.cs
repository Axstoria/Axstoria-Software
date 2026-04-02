using UnityEngine;
using System.Collections.Generic;
using VTT.Grid;

namespace VTT
{

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TerrainBuilder : MonoBehaviour
{
    [Header("Map Generation Settings")]
    public int   width       = 20;
    public int   depth       = 20;
    public float baseHeight  = 0f;
    public float thickness   = 3f;
    public Color terrainColor = new Color(0.6f, 0.4f, 0.2f);

    [Header("Table Settings")]
    public Transform tableTransform;
    public float     tableThickness = 1f;
    public float     tablePadding   = 0.1f;

    private Mesh      _mesh;
    private Vector3[] _vertices;
    private Color[]   _colors;
    private float[]   _heights;

    private void Start() => GenerateTerrain();

    public void GenerateTerrain()
    {
        _mesh      = new Mesh { name = "TerrainCube" };
        int gw     = width + 1;
        int gd     = depth + 1;
        int topVerts = gw * gd;

        _vertices = new Vector3[topVerts * 2];
        _colors   = new Color[_vertices.Length];
        _heights  = new float[topVerts];

        float ox = width  / 2f;
        float oz = depth  / 2f;

        for (int z = 0; z < gd; z++)
        for (int x = 0; x < gw; x++)
        {
            int i = z * gw + x;
            _vertices[i]            = new Vector3(x - ox, baseHeight,           z - oz);
            _vertices[i + topVerts] = new Vector3(x - ox, baseHeight - thickness, z - oz);
            _colors[i] = _colors[i + topVerts] = terrainColor;
            _heights[i] = baseHeight;
        }

        var tris = new List<int>();
        for (int z = 0; z < depth; z++)
        for (int x = 0; x < width; x++)
        {
            int i = z * gw + x;
            // Top face
            tris.Add(i); tris.Add(i + gw); tris.Add(i + 1);
            tris.Add(i + 1); tris.Add(i + gw); tris.Add(i + gw + 1);
            // Side faces
            if (x == 0)         AddQuad(tris, i + gw, i, i + topVerts + gw, i + topVerts);
            if (x == width - 1) AddQuad(tris, i + 1, i + 1 + gw, i + 1 + topVerts, i + 1 + topVerts + gw);
            if (z == 0)         AddQuad(tris, i, i + 1, i + topVerts, i + 1 + topVerts);
            if (z == depth - 1) AddQuad(tris, i + gw + 1, i + gw, i + topVerts + gw + 1, i + topVerts + gw);
        }

        _mesh.vertices  = _vertices;
        _mesh.colors    = _colors;
        _mesh.triangles = tris.ToArray();

        ApplyMesh();
        UpdateTable();

        // Inform GridManager of new surface Y level
        GridManager.Instance?.SetSurface(baseHeight);
    }

    public void Sculpt(Vector3 worldPoint, float radius, float intensity)
    {
        int topVerts = (width + 1) * (depth + 1);
        for (int i = 0; i < topVerts; i++)
        {
            float dist = Vector3.Distance(worldPoint, transform.TransformPoint(_vertices[i]));
            if (dist >= radius) continue;
            float falloff = 1f - (dist / radius);
            _heights[i]    += intensity * falloff;
            _vertices[i].y  = _heights[i];
        }
        ApplyMesh();
    }

    private void ApplyMesh()
    {
        _mesh.vertices = _vertices;
        _mesh.colors   = _colors;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh         = _mesh;
        GetComponent<MeshCollider>().sharedMesh = _mesh;

        // Update terrain color in material
        var renderer = GetComponent<MeshRenderer>();
        var mats     = renderer.materials;
        int slot     = mats.Length - 1;
        for (int i = 0; i < mats.Length; i++)
            if (mats[i] != null && mats[i].name.Contains("Terrain")) { slot = i; break; }

        if (mats[slot] != null)
        {
            if (mats[slot].HasProperty("_BaseColor")) mats[slot].SetColor("_BaseColor", terrainColor);
            if (mats[slot].HasProperty("_Color"))     mats[slot].SetColor("_Color",     terrainColor);
        }
        renderer.materials = mats;
    }

    private void UpdateTable()
    {
        if (tableTransform == null) return;
        tableTransform.localPosition = new Vector3(0, baseHeight - thickness - tableThickness * 0.5f, 0);
        tableTransform.localScale    = new Vector3(width + tablePadding, tableThickness, depth + tablePadding);
    }

    private static void AddQuad(List<int> tris, int v1, int v2, int v3, int v4)
    {
        tris.Add(v1); tris.Add(v2); tris.Add(v3);
        tris.Add(v3); tris.Add(v2); tris.Add(v4);
    }
}

} // namespace VTT
