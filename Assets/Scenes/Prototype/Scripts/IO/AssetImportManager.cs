using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VTT.IO;
using VTT.UI;

namespace VTT
{
    /// <summary>
    /// Manages runtime-imported 3D assets (glTF/GLB).
    /// Imported objects are added to a dedicated "Imported" PrefabCategory in
    /// VTTPanelUI so they appear in the prefab browser and can be placed on the map.
    ///
    /// Usage:
    ///   — Attach to any persistent GameObject.
    ///   — The Import button is drawn inside the Prefabs section automatically.
    ///   — Supports Editor file dialog, StandaloneFileBrowser (define USE_SFB),
    ///     and a persistentDataPath fallback for builds.
    /// </summary>
    [AddComponentMenu("VTT/Asset Import Manager")]
    public class AssetImportManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        public static AssetImportManager Instance { get; private set; }

        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Container")]
        [Tooltip("Parent transform for all imported GameObjects. Auto-created if empty.")]
        [SerializeField] private Transform assetContainer;

        [Header("Prefab Browser Integration")]
        [Tooltip("Name of the category that appears in the VTTPanelUI prefab browser.")]
        [SerializeField] private string importCategoryName = "Imported";

        // ── State ─────────────────────────────────────────────────────────────
        private readonly GltfImporter         _importer = new();
        private readonly IFileDialogService   _dialog   = FileDialogServiceFactory.Create();
        private readonly List<GameObject>     _assets   = new();
        private readonly Dictionary<GameObject, string> _importPaths = new();

        // The live category entry in VTTPanelUI (injected at Start)
        private PrefabCategory _importCategory;

        // UI feedback
        private bool   _importing;
        private string _importStatus = "";

        // ── Events ────────────────────────────────────────────────────────────
        /// <summary>Fired after a model is successfully imported.</summary>
        public event System.Action<GameObject> OnAssetImported;

        // ── Lifecycle ─────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (assetContainer == null)
            {
                assetContainer = new GameObject("VTT_ImportedAssets").transform;
                assetContainer.SetParent(transform);
            }
        }

        private void Start()
        {
            RegisterImportCategory();
        }

        // ── Category registration ─────────────────────────────────────────────
        /// <summary>
        /// Creates (or finds) the "Imported" PrefabCategory in VTTPanelUI
        /// so that every imported model appears in the prefab browser automatically.
        /// </summary>
        private void RegisterImportCategory()
        {
            var panel = FindFirstObjectByType<VTTPanelUI>();
            if (panel == null)
            {
                Debug.LogWarning("[VTT] AssetImportManager: VTTPanelUI not found — " +
                    "imported assets won't appear in the prefab browser.");
                return;
            }

            // Look for an existing category with our name
            foreach (var cat in panel.Categories)
            {
                if (cat.name == importCategoryName)
                {
                    _importCategory = cat;
                    return;
                }
            }

            // Create a fresh one and append it
            _importCategory = new PrefabCategory { name = importCategoryName };
            panel.Categories.Add(_importCategory);
            Debug.Log($"[VTT] Created '{importCategoryName}' category in prefab browser.");
        }

        // ── Public import API ─────────────────────────────────────────────────

        /// <summary>Open the OS file dialog and import the selected glTF/GLB.</summary>
        public async void ImportFromFileDialog()
        {
            if (_importing) return;

            string path = _dialog.OpenFile("Import 3D Model", new[] { "gltf", "glb" });
            if (string.IsNullOrEmpty(path)) return;

            await ImportFile(path);
        }

        /// <summary>Import from an explicit path (called from code or drag-and-drop).</summary>
        public async System.Threading.Tasks.Task ImportFile(string filepath)
        {
            if (_importing)
            {
                Debug.LogWarning("[VTT] Import already in progress.");
                return;
            }

            _importing    = true;
            _importStatus = $"Importing {Path.GetFileName(filepath)}…";

            var go = await _importer.LoadFromFile(filepath);

            _importing = false;

            if (go == null)
            {
                _importStatus = $"Failed: {Path.GetFileName(filepath)}";
                return;
            }

            RegisterAsset(go, filepath);
            _importStatus = $"Imported: {go.name}";
        }

        // ── Internal registration ─────────────────────────────────────────────
        private void RegisterAsset(GameObject go, string sourcePath)
        {
            // Parent into the hidden container — not visible in the scene
            go.transform.SetParent(assetContainer);
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            // Deactivate immediately — the object is a template only.
            // VTTPanelUI instantiates a fresh copy when the user clicks Place.
            go.SetActive(false);

            // Ensure colliders exist so placement raycasts work on the copy
            EnsureColliders(go);

            _assets.Add(go);
            _importPaths[go] = sourcePath;

            // Add to the "Imported" folder in the prefab browser
            if (_importCategory != null)
                _importCategory.prefabs.Add(go);
            else
                Debug.LogWarning("[VTT] Import category not found — call RegisterImportCategory first.");

            Debug.Log($"[VTT] Asset registered as template: {go.name} (from {sourcePath})");
            OnAssetImported?.Invoke(go);
        }

        private static void EnsureColliders(GameObject root)
        {
            if (root.GetComponentInChildren<Collider>() != null) return;
            foreach (var mf in root.GetComponentsInChildren<MeshFilter>())
            {
                var mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.convex = false;
            }
        }

        // ── Accessors ─────────────────────────────────────────────────────────
        public IReadOnlyList<GameObject> ImportedAssets => _assets;

        /// <summary>Returns the original file path for an imported asset.</summary>
        public string GetImportPath(GameObject go) =>
            _importPaths.TryGetValue(go, out var p) ? p : "";
        public bool                      IsImporting    => _importing;
        public string                    ImportStatus   => _importStatus;

        public void ClearAll()
        {
            foreach (var a in _assets) if (a != null) Destroy(a);
            _assets.Clear();
            _importCategory?.prefabs.Clear();
            _importStatus = "";
        }
    }
}
