using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TerrainBuilder : MonoBehaviour
{
    [Header("Map Generation Settings")]
    public int width = 20;
    public int depth = 20;
    public float baseHeight = 0f;
    public float thickness = 3f; 
    public Color terrainColor = new Color(0.6f, 0.4f, 0.2f); // Bright Brown

    [Header("Table Settings")]
    public Transform tableTransform; 
    public float tableThickness = 1f;
    public float tablePadding = 0.1f; 

    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] colors; // Added for vertex coloring
    private float[] heights;

    void Start()
    {
        GenerateTerrain();
        mesh.vertices = vertices;
        mesh.colors = colors; 
    }

    public void GenerateTerrain()
    {
        mesh = new Mesh();
        mesh.name = "CenteredTerrainCube";

        int gridWidth = width + 1;
        int gridDepth = depth + 1;
        int topVertCount = gridWidth * gridDepth;
        
        vertices = new Vector3[topVertCount * 2];
        colors = new Color[vertices.Length]; // Initialize color array
        heights = new float[topVertCount];
        List<int> triangles = new List<int>();

        float offsetX = width / 2f;
        float offsetZ = depth / 2f;

        for (int z = 0; z < gridDepth; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int i = z * gridWidth + x;
                
                // Top Vertex
                vertices[i] = new Vector3(x - offsetX, baseHeight, z - offsetZ);
                colors[i] = terrainColor; // Assign Color
                heights[i] = baseHeight;
                
                // Bottom Vertex
                vertices[i + topVertCount] = new Vector3(x - offsetX, baseHeight - thickness, z - offsetZ);
                colors[i + topVertCount] = terrainColor; // Assign Color
            }
        }

        // Triangle Generation (Face & Sides)
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = z * gridWidth + x;
                triangles.Add(i); triangles.Add(i + gridWidth); triangles.Add(i + 1);
                triangles.Add(i + 1); triangles.Add(i + gridWidth); triangles.Add(i + gridWidth + 1);
                
                if (x == 0) AddQuad(triangles, i + gridWidth, i, i + topVertCount + gridWidth, i + topVertCount);
                if (x == width - 1) AddQuad(triangles, i + 1, i + 1 + gridWidth, i + 1 + topVertCount, i + 1 + topVertCount + gridWidth);
                if (z == 0) AddQuad(triangles, i, i + 1, i + topVertCount, i + 1 + topVertCount);
                if (z == depth - 1) AddQuad(triangles, i + gridWidth + 1, i + gridWidth, i + topVertCount + gridWidth + 1, i + topVertCount + gridWidth);
            }
        }

        mesh.vertices = vertices;
        mesh.colors = colors; // Push colors to mesh
        mesh.triangles = triangles.ToArray();
        
        UpdateMesh();
        UpdateTable(); 
    }

    private void UpdateTable()
    {
        if (tableTransform == null) return;
        float tableY = baseHeight - thickness - (tableThickness / 2f);
        tableTransform.localPosition = new Vector3(0, tableY, 0);
        tableTransform.localScale = new Vector3(width + tablePadding, tableThickness, depth + tablePadding);
    }

    void AddQuad(List<int> tris, int v1, int v2, int v3, int v4)
    {
        tris.Add(v1); tris.Add(v2); tris.Add(v3);
        tris.Add(v3); tris.Add(v2); tris.Add(v4);
    }

    public void Sculpt(Vector3 point, float radius, float intensity)
    {
        int topVertCount = (width + 1) * (depth + 1);
        for (int i = 0; i < topVertCount; i++)
        {
            float dist = Vector3.Distance(point, transform.TransformPoint(vertices[i]));
            if (dist < radius)
            {
                float falloff = 1f - (dist / radius);
                heights[i] += intensity * falloff;
                vertices[i].y = heights[i];
            }
        }
        UpdateMesh();
    }

    void UpdateMesh()
    {
        mesh.vertices = vertices;
        mesh.colors = colors; 
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshRenderer renderer = GetComponent<MeshRenderer>();

        // Apply terrainColor to the correct material slot.
        // The renderer has two materials: [0] ProceduralGrid, [1] Terrain.
        // renderer.material only returns index 0 — we must use renderer.materials[].
        var mats = renderer.materials;
        // Find the Terrain material by name (index 1), fall back to last slot.
        int terrainMatIndex = mats.Length - 1;
        for (int i = 0; i < mats.Length; i++)
            if (mats[i] != null && mats[i].name.Contains("Terrain"))
            { terrainMatIndex = i; break; }

        if (mats[terrainMatIndex] != null)
        {
            // Works for Standard, URP Lit, and any shader with _BaseColor or _Color
            if (mats[terrainMatIndex].HasProperty("_BaseColor"))
                mats[terrainMatIndex].SetColor("_BaseColor", terrainColor);
            if (mats[terrainMatIndex].HasProperty("_Color"))
                mats[terrainMatIndex].SetColor("_Color", terrainColor);
        }
        // Write the array back — required, Unity copies on read
        renderer.materials = mats;

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}