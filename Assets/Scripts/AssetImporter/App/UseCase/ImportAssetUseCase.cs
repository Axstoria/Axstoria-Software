using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using GLTFast.Logging;
using SceneEditor.Domain;
using Shared.App.Port;
using UnityEngine;

namespace AssetImporter.App.UseCase
{
    public class ImportAssetUseCase
    {
        private readonly IFileDialogService _dialog;

        public event Action<SceneObject, GameObject> OnImported;

        public ImportAssetUseCase(IFileDialogService dialog)
        {
            _dialog = dialog;
        }

        // Called by the user via file dialog. Copies asset to persistent storage before loading.
        public async void Execute(string category = "Imported")
        {
            string sourcePath = _dialog.OpenFile("Import 3D Asset", new[] { ".gltf", ".glb" });
            if (string.IsNullOrEmpty(sourcePath)) return;

            string id          = Guid.NewGuid().ToString();
            string displayName = Path.GetFileNameWithoutExtension(sourcePath);
            string ext         = Path.GetExtension(sourcePath).ToLowerInvariant();

            string persistentPath = CopyToPersistent(sourcePath, id, ext);

            var obj = new SceneObject
            {
                Id          = id,
                DisplayName = displayName,
                Category    = category,
                IsImported  = true,
                ImportPath  = persistentPath
            };

            WriteToManifest(obj);
            await LoadAndInstantiate(obj);
        }

        // Called on map load or startup — re-loads an asset already in persistent storage, no dialog.
        public async void ExecuteFromPath(string persistentPath, string id, string category, string displayName)
        {
            if (string.IsNullOrEmpty(persistentPath) || !File.Exists(persistentPath))
            {
                Debug.LogWarning($"[ImportAssetUseCase] Persistent asset not found: {persistentPath}");
                return;
            }

            var obj = new SceneObject
            {
                Id          = id,
                DisplayName = displayName,
                Category    = category,
                IsImported  = true,
                ImportPath  = persistentPath
            };

            await LoadAndInstantiate(obj);
        }

        // ── Manifest ──────────────────────────────────────────────────────────────

        [Serializable]
        public class ManifestEntry
        {
            public string id;
            public string displayName;
            public string category;
            public string importPath;
        }

        [Serializable]
        private class AssetManifest
        {
            public List<ManifestEntry> assets = new();
        }

        private static string ManifestPath
            => Path.Combine(Application.persistentDataPath, "ImportedAssets", "manifest.json");

        private static AssetManifest ReadManifest()
        {
            try
            {
                if (File.Exists(ManifestPath))
                    return JsonUtility.FromJson<AssetManifest>(File.ReadAllText(ManifestPath))
                           ?? new AssetManifest();
            }
            catch (Exception e) { Debug.LogWarning($"[ImportAssetUseCase] Could not read manifest: {e.Message}"); }
            return new AssetManifest();
        }

        private static void WriteToManifest(SceneObject obj)
        {
            AssetManifest manifest = ReadManifest();
            manifest.assets.RemoveAll(e => e.id == obj.Id);
            manifest.assets.Add(new ManifestEntry
            {
                id          = obj.Id,
                displayName = obj.DisplayName,
                category    = obj.Category,
                importPath  = obj.ImportPath
            });
            Directory.CreateDirectory(Path.GetDirectoryName(ManifestPath));
            File.WriteAllText(ManifestPath, JsonUtility.ToJson(manifest, true));
        }

        public ManifestEntry[] GetAllImportedAssets() => ReadManifest().assets.ToArray();

        // ── GLTFast load + instantiate ────────────────────────────────────────────

        private async Task LoadAndInstantiate(SceneObject obj)
        {
            var gltfImport = new GltfImport(logger: new ConsoleLogger());
            bool loaded = await gltfImport.Load(new Uri(obj.ImportPath).AbsoluteUri);
            if (!loaded)
            {
                Debug.LogError($"[ImportAssetUseCase] Failed to load GLTF: {obj.ImportPath}");
                return;
            }

            var root = new GameObject(obj.DisplayName);
            bool instantiated = await gltfImport.InstantiateMainSceneAsync(root.transform);
            if (!instantiated)
            {
                UnityEngine.Object.Destroy(root);
                Debug.LogError($"[ImportAssetUseCase] Failed to instantiate GLTF: {obj.DisplayName}");
                return;
            }

            OnImported?.Invoke(obj, root);
        }

        // GLB  → single self-contained file, copy directly.
        // GLTF → JSON referencing external .bin buffers and textures; parse and copy only those files.
        private static string CopyToPersistent(string sourcePath, string id, string ext)
        {
            string baseDir = Path.Combine(Application.persistentDataPath, "ImportedAssets");

            if (ext == ".glb")
            {
                string dest = Path.Combine(baseDir, id + ".glb");
                Directory.CreateDirectory(baseDir);
                File.Copy(sourcePath, dest, overwrite: true);
                return dest;
            }
            else
            {
                string destDir   = Path.Combine(baseDir, id);
                string sourceDir = Path.GetDirectoryName(sourcePath);
                Directory.CreateDirectory(destDir);

                string destGltf = Path.Combine(destDir, Path.GetFileName(sourcePath));
                File.Copy(sourcePath, destGltf, overwrite: true);

                // Parse the gltf JSON and copy only the files it references
                var gltfDoc = JsonUtility.FromJson<GltfManifest>(File.ReadAllText(sourcePath));

                var refs = new List<string>();
                if (gltfDoc.buffers != null)
                    foreach (var b in gltfDoc.buffers)
                        if (!string.IsNullOrEmpty(b.uri) && !b.uri.StartsWith("data:"))
                            refs.Add(b.uri);
                if (gltfDoc.images != null)
                    foreach (var img in gltfDoc.images)
                        if (!string.IsNullOrEmpty(img.uri) && !img.uri.StartsWith("data:"))
                            refs.Add(img.uri);

                foreach (string relPath in refs)
                {
                    string src  = Path.Combine(sourceDir, relPath);
                    string dest = Path.Combine(destDir, relPath);
                    if (!File.Exists(src)) continue;
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    File.Copy(src, dest, overwrite: true);
                }

                return destGltf;
            }
        }

        [Serializable] private class GltfManifest { public GltfBuffer[] buffers; public GltfImage[] images; }
        [Serializable] private class GltfBuffer    { public string uri; }
        [Serializable] private class GltfImage     { public string uri; }
    }
}
