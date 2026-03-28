using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class RuntimeGrid : MonoBehaviour
{
    public int width = 10;
    public int depth = 10;
    public float cellSize = 1f;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    void Start()
    {
        GenerateGrid();
    }

    public void ResizeGrid(int newWidth, int newDepth)
    {
        width = newWidth;
        depth = newDepth;
        GenerateGrid();
    }

    void GenerateGrid()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        vertices = new Vector3[(width + 1) * (depth + 1)];
        for (int i = 0, z = 0; z <= depth; z++) {
            for (int x = 0; x <= width; x++) {
                vertices[i] = new Vector3(x * cellSize, 0, z * cellSize);
                i++;
            }
        }

        triangles = new int[width * depth * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < depth; z++) {
            for (int x = 0; x < width; x++) {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + width + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + width + 1;
                triangles[tris + 5] = vert + width + 2;
                vert++;
                tris += 6;
            }
            vert++;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        // Update collider so objects can be placed on it
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
