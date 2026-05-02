using System.Collections.Generic;
using UnityEngine;

namespace MapEditor.Presenter.View
{
    internal static class GizmoRenderer
    {
        private static Material _lineMat;
        private static Material _meshMat;
        private static Mesh     _sphereMesh;
        private static Mesh     _cylinderMesh;
        private static Mesh     _coneMesh;

        public static void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            EnsureLineMat();
            _lineMat.SetPass(0);
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex3(start.x, start.y, start.z);
            GL.Vertex3(end.x,   end.y,   end.z);
            GL.End();
            GL.PopMatrix();
        }

        public static void DrawWireCircle(Vector3 center, float radius, int segments, Vector3 normal, Color color)
        {
            Vector3 forward = Vector3.Slerp(
                normal == Vector3.up ? Vector3.forward : Vector3.up, normal, 0.01f).normalized;
            Vector3 right = Vector3.Cross(normal, forward).normalized;
            Vector3 up    = Vector3.Cross(right,  normal).normalized;

            var pts = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                pts[i]  = center + (right * Mathf.Cos(a) + up * Mathf.Sin(a)) * radius;
            }

            EnsureLineMat();
            _lineMat.SetPass(0);
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            GL.Color(color);
            for (int i = 0; i < segments; i++)
            {
                Vector3 a = pts[i];
                Vector3 b = pts[(i + 1) % segments];
                GL.Vertex3(a.x, a.y, a.z);
                GL.Vertex3(b.x, b.y, b.z);
            }
            GL.End();
            GL.PopMatrix();
        }

        public static void DrawPoint(Vector3 position, float radius, Color color)
        {
            EnsureSphereMesh();
            EnsureMeshMat();
            _meshMat.color = color;
            _meshMat.SetPass(0);
            Graphics.DrawMeshNow(_sphereMesh,
                Matrix4x4.TRS(position, Quaternion.identity, Vector3.one * radius * 2f));
        }

        public static void DrawCylinder(Vector3 start, Vector3 direction, float length, Color color,
                                        float shaftRadius = 0.05f)
        {
            if (direction == Vector3.zero) return;
            Vector3    dir      = direction.normalized;
            Quaternion rotation = Quaternion.LookRotation(dir);

            EnsureCylinderMesh();
            EnsureMeshMat();
            _meshMat.color = color;
            _meshMat.SetPass(0);

            Vector3   shaftPos    = start + dir * (length * 0.5f);
            Matrix4x4 shaftMatrix = Matrix4x4.TRS(
                shaftPos,
                rotation * Quaternion.Euler(90f, 0f, 0f),
                new Vector3(shaftRadius * 2f, length * 0.5f, shaftRadius * 2f));
            Graphics.DrawMeshNow(_cylinderMesh, shaftMatrix);
        }

        public static void DrawCone(Vector3 tip, Vector3 direction, Color color,
                                    float coneLength = 0.25f, float coneRadius = 0.08f)
        {
            if (direction == Vector3.zero) return;
            Vector3    dir      = direction.normalized;
            Quaternion rotation = Quaternion.LookRotation(dir);

            EnsureConeMesh();
            EnsureMeshMat();
            _meshMat.color = color;
            _meshMat.SetPass(0);

            Matrix4x4 matrix = Matrix4x4.TRS(
                tip,
                rotation * Quaternion.Euler(90f, 0f, 0f),
                new Vector3(coneRadius * 2f, coneLength * 0.5f, coneRadius * 2f));
            Graphics.DrawMeshNow(_coneMesh, matrix);
        }

        private static void EnsureLineMat()
        {
            if (_lineMat != null) return;
            var shader = Shader.Find("Hidden/Internal-Colored");
            _lineMat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            _lineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _lineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _lineMat.SetInt("_Cull",     (int)UnityEngine.Rendering.CullMode.Off);
            _lineMat.SetInt("_ZWrite",   0);
            _lineMat.SetInt("_ZTest",    (int)UnityEngine.Rendering.CompareFunction.Always);
        }

        private static void EnsureMeshMat()
        {
            if (_meshMat != null) return;
            var shader = Shader.Find("Unlit/Color");
            _meshMat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            _meshMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }

        private static void EnsureSphereMesh()
        {
            if (_sphereMesh != null) return;
            var tmp    = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _sphereMesh = tmp.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(tmp);
        }

        private static void EnsureCylinderMesh()
        {
            if (_cylinderMesh != null) return;
            var tmp       = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _cylinderMesh = tmp.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(tmp);
        }

        private static void EnsureConeMesh()
        {
            if (_coneMesh != null) return;
            _coneMesh = BuildConeMesh();
        }

        private static Mesh BuildConeMesh(int segments = 24)
        {
            var mesh  = new Mesh();
            var verts = new List<Vector3>();
            var tris  = new List<int>();

            verts.Add(Vector3.up);
            for (int i = 0; i <= segments; i++)
            {
                float a = (float)i / segments * Mathf.PI * 2f;
                verts.Add(new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)));
            }

            for (int i = 1; i <= segments; i++)
                { tris.Add(0); tris.Add(i); tris.Add(i + 1); }

            int baseCenter = verts.Count;
            verts.Add(Vector3.zero);
            for (int i = 1; i <= segments; i++)
                { tris.Add(baseCenter); tris.Add(i + 1); tris.Add(i); }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
