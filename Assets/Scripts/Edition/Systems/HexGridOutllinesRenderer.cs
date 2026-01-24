using System.Collections.Generic;
using UnityEngine;

namespace HexGrid.Systems
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexGridOutlineRenderer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Grid grid;

        [Header("Grid area to draw (cells)")]
        [SerializeField] private Vector3Int minCell = new Vector3Int(0, 0, 0);
        [SerializeField] private Vector3Int maxCell = new Vector3Int(20, 20, 0);

        [Header("Hex shape")]
        [SerializeField] private bool pointyTop = true;
        [SerializeField] private float radius = 0.5f; // center -> corner in world units

        [Header("Precision for deduping shared edges")]
        [SerializeField] private float quantize = 0.001f;

        MeshFilter _mf;

        void Awake() => _mf = GetComponent<MeshFilter>();

        void Start() => Rebuild();

        public void Rebuild()
        {
            if (!grid) return;

            var verts = new List<Vector3>();
            var indices = new List<int>();
            var edgeSet = new HashSet<ulong>();

            int idx = 0;

            for (int x = minCell.x; x <= maxCell.x; x++)
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                Vector3 center = grid.GetCellCenterWorld(cell);

                Vector3[] corners = new Vector3[6];
                for (int i = 0; i < 6; i++) corners[i] = Corner(center, i);

                for (int i = 0; i < 6; i++)
                {
                    Vector3 a = corners[i];
                    Vector3 b = corners[(i + 1) % 6];

                    ulong key = EdgeKey(a, b);
                    if (!edgeSet.Add(key)) continue; // avoid drawing shared borders twice

                    verts.Add(a);
                    verts.Add(b);
                    indices.Add(idx++);
                    indices.Add(idx++);
                }
            }

            var mesh = new Mesh();
            mesh.indexFormat = verts.Count > 65000
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;

            mesh.SetVertices(verts);
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            mesh.RecalculateBounds();

            _mf.sharedMesh = mesh;
        }

        Vector3 Corner(Vector3 center, int i)
        {
            float angleDeg = pointyTop ? (60f * i - 30f) : (60f * i);
            float a = angleDeg * Mathf.Deg2Rad;
            return center + new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
        }

        Vector3Int Q(Vector3 v) => new Vector3Int(
            Mathf.RoundToInt(v.x / quantize),
            Mathf.RoundToInt(v.y / quantize),
            Mathf.RoundToInt(v.z / quantize)
        );

        ulong EdgeKey(Vector3 p1, Vector3 p2)
        {
            var a = Q(p1);
            var b = Q(p2);

            // order-independent
            if (a.x > b.x || (a.x == b.x && (a.y > b.y || (a.y == b.y && a.z > b.z))))
                (a, b) = (b, a);

            unchecked
            {
                ulong h = 1469598103934665603UL;
                h = (h ^ (ulong)a.x) * 1099511628211UL;
                h = (h ^ (ulong)a.y) * 1099511628211UL;
                h = (h ^ (ulong)a.z) * 1099511628211UL;
                h = (h ^ (ulong)b.x) * 1099511628211UL;
                h = (h ^ (ulong)b.y) * 1099511628211UL;
                h = (h ^ (ulong)b.z) * 1099511628211UL;
                return h;
            }
        }
    }
}
