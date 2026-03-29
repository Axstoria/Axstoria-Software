using System;
using System.IO;
using UnityEngine;
using VTT.IO;
using VTT.UI;

namespace VTT.Persistence
{
    /// <summary>
    /// Public entry point for saving and loading maps.
    /// Delegates serialization to MapSerializer and async loading to MapLoader.
    /// Add this component to any persistent GameObject.
    /// </summary>
    [AddComponentMenu("VTT/Map Save Load")]
    public class MapSaveLoad : MonoBehaviour
    {
        public static MapSaveLoad Instance { get; private set; }

        [Header("Scene References (auto-found if empty)")]
        [SerializeField] private TerrainBuilder terrainBuilder;
        [SerializeField] private MeshRenderer   gridRenderer;
        [SerializeField] private Transform      decorContainer;

        [Header("Registry")]
        [SerializeField] private PrefabRegistry prefabRegistry;

        public string Status         { get; private set; } = "";
        public bool   IsBusy         { get; private set; } = false;
        public string CurrentMapName { get; private set; } = "Unnamed Map";

        private readonly IFileDialogService _dialog = FileDialogServiceFactory.Create();
        private MapSerializer _serializer;
        private MapLoader     _loader;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _serializer = gameObject.AddComponent<MapSerializer>();
            _loader     = gameObject.AddComponent<MapLoader>();
        }

        private void Start()
        {
            if (terrainBuilder == null) terrainBuilder = FindFirstObjectByType<TerrainBuilder>();
            if (gridRenderer   == null && terrainBuilder != null)
                gridRenderer = terrainBuilder.GetComponent<MeshRenderer>();

            var panel = FindFirstObjectByType<VTTPanelUI>();
            if (panel != null && prefabRegistry != null)
                prefabRegistry.RegisterFromCategories(panel.Categories);

            var aim = AssetImportManager.Instance;
            if (aim != null)
                aim.OnAssetImported += go => prefabRegistry?.Register(go.name, go);
        }

        // ── Save ──────────────────────────────────────────────────────────────

        public void SaveWithDialog()
        {
            if (IsBusy) return;
            string path = _dialog.SaveFile("Save Map", CurrentMapName, "json");
            if (!string.IsNullOrEmpty(path)) SaveToPath(path);
        }

        public void SaveToPath(string path)
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                Status = "Saving…";

                var data     = _serializer.Collect(terrainBuilder, gridRenderer);
                data.mapName = Path.GetFileNameWithoutExtension(path);
                data.savedAt = DateTime.UtcNow.ToString("o");

                File.WriteAllText(path, JsonUtility.ToJson(data, prettyPrint: true));
                CurrentMapName = data.mapName;
                Status = $"Saved: {data.mapName}  ({data.objects.Count} objects)";
                Debug.Log($"[VTT] Saved to {path}");
            }
            catch (Exception e)
            {
                Status = $"Save failed: {e.Message}";
                Debug.LogError($"[VTT] Save failed: {e}");
            }
            finally { IsBusy = false; }
        }

        // ── Load ──────────────────────────────────────────────────────────────

        public void LoadWithDialog()
        {
            if (IsBusy) return;
            string path = _dialog.OpenFile("Load Map", "json");
            if (!string.IsNullOrEmpty(path)) LoadFromPath(path);
        }

        public void LoadFromPath(string path)
        {
            if (IsBusy) return;
            IsBusy = true;
            Status = "Loading…";
            StartCoroutine(_loader.Load(path, this,
                terrainBuilder, gridRenderer, decorContainer,
                prefabRegistry, _serializer));
        }

        /// <summary>Called by MapLoader when the coroutine finishes.</summary>
        public void SetStatus(string status, bool busy, string mapName = null)
        {
            Status = status;
            IsBusy = busy;
            if (mapName != null) CurrentMapName = mapName;
        }
    }
}