using System;
using System.Collections;
using System.IO;
using UnityEngine;
using VTT.IO;

namespace VTT.Persistence
{
    /// <summary>
    /// Handles the async load coroutine for MapSaveLoad.
    /// </summary>
    public class MapLoader : MonoBehaviour
    {
        private readonly GltfImporter _importer = new();

        public IEnumerator Load(string path, MapSaveLoad owner,
            TerrainBuilder tb, MeshRenderer gr,
            Transform decorContainer, PrefabRegistry registry,
            MapSerializer serializer)
        {
            // ── Parse ─────────────────────────────────────────────────────────
            MapData data = null;
            try
            {
                if (!File.Exists(path)) throw new FileNotFoundException($"File not found: {path}");
                data = UnityEngine.JsonUtility.FromJson<MapData>(File.ReadAllText(path));
                if (data == null) throw new Exception("JSON null — file may be corrupt.");
            }
            catch (Exception e)
            {
                owner.SetStatus($"Load failed: {e.Message}", busy: false);
                Debug.LogError($"[VTT] Load: {e}");
                yield break;
            }

            // ── Apply terrain & grid ──────────────────────────────────────────
            serializer.ApplyTerrain(data.terrain, tb);
            serializer.ApplyGrid(data.grid, gr);
            yield return null;

            // ── Clear existing objects ────────────────────────────────────────
            MapSerializer.ClearDecorObjects();
            yield return null;

            // ── Restore objects ───────────────────────────────────────────────
            int ok = 0, fail = 0;
            foreach (var od in data.objects)
            {
                if (od.isImported && !string.IsNullOrEmpty(od.importPath))
                {
                    yield return StartCoroutine(RestoreImported(od, registry,
                        serializer, decorContainer, s => { if (s) ok++; else fail++; }));
                }
                else
                {
                    if (RestorePrefab(od, registry, serializer, decorContainer)) ok++;
                    else fail++;
                }
            }

            owner.SetStatus(
                $"Loaded: {data.mapName}  ({ok} objects{(fail > 0 ? $", {fail} failed" : "")})",
                busy: false,
                mapName: data.mapName);
            Debug.Log($"[VTT] Map loaded — {ok} ok, {fail} failed");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static bool RestorePrefab(DecorObjectData od, PrefabRegistry registry,
            MapSerializer serializer, Transform container)
        {
            var prefab = registry?.Find(od.prefabName);

            if (prefab == null)
            {
                var panel = FindObjectOfType<UI.VTTPanelUI>();
                if (panel != null)
                    foreach (var cat in panel.Categories)
                        if (cat?.prefabs != null)
                            foreach (var pf in cat.prefabs)
                                if (pf != null && pf.name == od.prefabName)
                                { prefab = pf; break; }
            }

            if (prefab == null)
            {
                Debug.LogWarning($"[VTT] Prefab '{od.prefabName}' not found.");
                return false;
            }

            serializer.SpawnAndRegister(prefab, od, container);
            return true;
        }

        private IEnumerator RestoreImported(DecorObjectData od, PrefabRegistry registry,
            MapSerializer serializer, Transform container, Action<bool> onDone)
        {
            var existing = registry?.Find(od.prefabName);
            if (existing != null)
            {
                serializer.SpawnAndRegister(existing, od, container);
                onDone(true);
                yield break;
            }

            if (!File.Exists(od.importPath))
            {
                Debug.LogWarning($"[VTT] Import path not found: {od.importPath}");
                onDone(false);
                yield break;
            }

            bool done = false;
            GameObject loaded = null;
            _importer.LoadFromFileAsync(od.importPath, go => { loaded = go; done = true; });
            while (!done) yield return null;

            if (loaded == null) { onDone(false); yield break; }

            loaded.SetActive(false);
            registry?.Register(loaded.name, loaded);
            serializer.SpawnAndRegister(loaded, od, container);
            onDone(true);
        }
    }
}
