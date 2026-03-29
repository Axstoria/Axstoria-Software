using System.Collections.Generic;
using UnityEngine;
using VTT.Grid;

namespace VTT.Persistence
{
    /// <summary>
    /// Converts scene state ↔ MapData.
    /// Added automatically by MapSaveLoad — do not add manually.
    /// </summary>
    public class MapSerializer : MonoBehaviour
    {
        private static readonly int SH_CellSize = Shader.PropertyToID("_Cell_Size");
        private static readonly int SH_Thick    = Shader.PropertyToID("_Grid_Thickness");
        private static readonly int SH_BgCol    = Shader.PropertyToID("_Background_Color");
        private static readonly int SH_GridCol  = Shader.PropertyToID("_Grid_Color");
        private static readonly int SH_Transp   = Shader.PropertyToID("_Transparent");
        private static readonly int SH_Opacity  = Shader.PropertyToID("_Grid_Opacity");

        // ── Collect ───────────────────────────────────────────────────────────

        public MapData Collect(TerrainBuilder tb, MeshRenderer gridRenderer)
        {
            var data = new MapData();
            CollectTerrain(tb, data.terrain);
            CollectGrid(gridRenderer, data.grid);
            CollectObjects(data.objects);
            return data;
        }

        private static void CollectTerrain(TerrainBuilder tb, TerrainData td)
        {
            if (tb == null) return;
            td.width        = tb.width;
            td.depth        = tb.depth;
            td.thickness    = Mathf.RoundToInt(tb.thickness);
            td.baseHeight   = tb.baseHeight;
            td.terrainColor = SColor.From(tb.terrainColor);
        }

        private void CollectGrid(MeshRenderer gr, GridShaderData gd)
        {
            if (gr == null) return;
            var mat = gr.material;
            var cs  = mat.GetVector(SH_CellSize);
            gd.cellSizeX     = cs.x;
            gd.cellSizeY     = cs.y;
            gd.gridThickness = mat.GetFloat(SH_Thick);
            gd.bgColor       = SColor.From(mat.GetColor(SH_BgCol));
            gd.gridColor     = SColor.From(mat.GetColor(SH_GridCol));
            gd.gridOpacity   = mat.GetFloat(SH_Opacity);
            gd.transparent   = mat.GetFloat(SH_Transp) > 0.5f;
        }

        private static void CollectObjects(List<DecorObjectData> list)
        {
            foreach (var decor in DecorObject.All)
            {
                if (decor == null) continue;
                var t = decor.transform;
                list.Add(new DecorObjectData
                {
                    guid        = decor.GetInstanceID().ToString(),
                    displayName = decor.displayName,
                    category    = decor.category,
                    prefabName  = decor.prefabName,
                    isImported  = decor.isImported,
                    importPath  = decor.importPath,
                    position    = SVector3.From(t.position),
                    rotation    = SVector3.From(t.eulerAngles),
                    scale       = SVector3.From(t.localScale),
                    gridCellX   = decor.gridCell.x,
                    gridCellZ   = decor.gridCell.y,
                });
            }
        }

        // ── Apply ─────────────────────────────────────────────────────────────

        public void ApplyTerrain(TerrainData td, TerrainBuilder tb)
        {
            if (tb == null) return;
            tb.width        = td.width;
            tb.depth        = td.depth;
            tb.thickness    = td.thickness;
            tb.baseHeight   = td.baseHeight;
            tb.terrainColor = td.terrainColor.ToColor();
            tb.GenerateTerrain();
        }

        public void ApplyGrid(GridShaderData gd, MeshRenderer gr)
        {
            if (gr == null) return;
            var mat = gr.material;
            mat.SetVector(SH_CellSize, new Vector4(gd.cellSizeX, gd.cellSizeY));
            mat.SetFloat(SH_Thick,   gd.gridThickness);
            mat.SetColor(SH_BgCol,   gd.bgColor.ToColor());
            mat.SetColor(SH_GridCol, gd.gridColor.ToColor());
            mat.SetFloat(SH_Opacity, gd.gridOpacity);
            mat.SetFloat(SH_Transp,  gd.transparent ? 1f : 0f);
        }

        public static void ClearDecorObjects()
        {
            var toDelete = new List<DecorObject>(DecorObject.All);
            foreach (var d in toDelete)
                if (d != null) d.Delete();
        }

        public void SpawnAndRegister(GameObject prefab, DecorObjectData od, Transform container)
        {
            var inst = Instantiate(prefab,
                od.position.ToVector3(),
                Quaternion.Euler(od.rotation.ToVector3()));
            inst.transform.localScale = od.scale.ToVector3();
            inst.SetActive(true);

            if (container != null)
                inst.transform.SetParent(container, worldPositionStays: true);

            var decor         = inst.GetComponent<DecorObject>() ?? inst.AddComponent<DecorObject>();
            decor.displayName = od.displayName;
            decor.category    = od.category;
            decor.prefabName  = od.prefabName;
            decor.isImported  = od.isImported;
            decor.importPath  = od.importPath;
            decor.gridCell    = new Vector2Int(od.gridCellX, od.gridCellZ);

            var po = inst.GetComponent<PlaceableObject>() ?? inst.AddComponent<PlaceableObject>();
            PlacementSystem.Instance?.Place(po, decor.gridCell);
        }
    }
}
