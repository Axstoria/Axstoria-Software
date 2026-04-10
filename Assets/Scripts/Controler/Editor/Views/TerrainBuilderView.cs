using System;
using System.Collections.Generic;
using Controler.Editor.ViewModels;
using Loxodon.Framework.Contexts;
using UnityEngine;

namespace Controler.Editor.Views
{
    /// <summary>
    /// Procedurally generates the terrain mesh based on TerrainLayoutViewModel properties.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class TerrainBuilderView : MonoBehaviour
    {
        [Header("Table")]
        [SerializeField] private Transform tableTransform;
        [SerializeField] private float     tableThickness = 1f;
        [SerializeField] private float     tablePadding   = 0.1f;

        private MapEditorViewModel     _vm;
        private TerrainLayoutViewModel _terrain;
        private Mesh                   _mesh;
        private EventHandler           _onTerrainChanged;

        private void Start()
        {
            _vm = Context.GetApplicationContext()
                         .GetContainer()
                         .Resolve<MapEditorViewModel>();

            if (_vm == null)
            {
                Debug.LogError("[TerrainBuilderView] MapEditorViewModel not found.");
                enabled = false;
                return;
            }

            _terrain = _vm.Map.Terrain;
            if (_terrain == null) return;

            _onTerrainChanged = (_, __) => Rebuild();
            _terrain.Width.ValueChanged     += _onTerrainChanged;
            _terrain.Depth.ValueChanged     += _onTerrainChanged;
            _terrain.Thickness.ValueChanged += _onTerrainChanged;
            _terrain.Height.ValueChanged    += _onTerrainChanged;

            Rebuild();
        }

        private void OnDestroy()
        {
            if (_terrain != null && _onTerrainChanged != null)
            {
                _terrain.Width.ValueChanged     -= _onTerrainChanged;
                _terrain.Depth.ValueChanged     -= _onTerrainChanged;
                _terrain.Thickness.ValueChanged -= _onTerrainChanged;
                _terrain.Height.ValueChanged    -= _onTerrainChanged;
            }

            if (_mesh != null) Destroy(_mesh);
        }

        private void Rebuild()
        {
            if (_terrain == null) return;

            int   w   = _terrain.Width.Value;
            int   d   = _terrain.Depth.Value;
            int   th  = _terrain.Thickness.Value;
            float h   = _terrain.Height.Value;
            var   col = _terrain.Model.Color;
            var   c   = col != null && col.Length >= 4
                ? new Color(col[0], col[1], col[2], col[3])
                : new Color(0.6f, 0.4f, 0.2f);

            GenerateMesh(w, d, th, h, c);

            // Sync surface Y back to Grid so cell world positions are correct
            if (_vm.Grid != null) _vm.Grid.SurfaceY = h;

            UpdateTable(w, d, th, h);
        }

        private void GenerateMesh(int width, int depth, int thickness, float baseHeight, Color color)
        {
            if (_mesh != null) Destroy(_mesh);
            _mesh      = new Mesh { name = "Terrain" };
            int gw     = width  + 1;
            int gd     = depth  + 1;
            int topCount = gw * gd;

            var vertices = new Vector3[topCount * 2];
            var colors   = new Color[vertices.Length];
            float ox     = width  / 2f;
            float oz     = depth  / 2f;

            for (int z = 0; z < gd; z++)
            for (int x = 0; x < gw; x++)
            {
                int i = z * gw + x;
                vertices[i]            = new Vector3(x - ox, baseHeight,              z - oz);
                vertices[i + topCount] = new Vector3(x - ox, baseHeight - thickness,  z - oz);
                colors[i] = colors[i + topCount] = color;
            }

            var tris = new List<int>();
            for (int z = 0; z < depth; z++)
            for (int x = 0; x < width; x++)
            {
                int i = z * gw + x;
                tris.Add(i); tris.Add(i + gw); tris.Add(i + 1);
                tris.Add(i + 1); tris.Add(i + gw); tris.Add(i + gw + 1);

                if (x == 0)          AddQuad(tris, i + gw, i, i + topCount + gw, i + topCount);
                if (x == width - 1)  AddQuad(tris, i + 1, i + 1 + gw, i + 1 + topCount, i + 1 + topCount + gw);
                if (z == 0)          AddQuad(tris, i, i + 1, i + topCount, i + 1 + topCount);
                if (z == depth - 1)  AddQuad(tris, i + gw + 1, i + gw, i + topCount + gw + 1, i + topCount + gw);
            }

            _mesh.vertices  = vertices;
            _mesh.colors    = colors;
            _mesh.triangles = tris.ToArray();
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();

            GetComponent<MeshFilter>().sharedMesh = _mesh;

            var col = GetComponent<MeshCollider>();
            col.sharedMesh = null;   // force physics re-cook
            col.sharedMesh = _mesh;
        }

        private void UpdateTable(int width, int depth, int thickness, float baseHeight)
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
}
