using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

// Requires the glTFast package: com.unity.cloud.gltfast
// Install via: Window → Package Manager → Add by name → com.unity.cloud.gltfast
using GLTFast;

namespace VTT.IO
{
    /// <summary>
    /// Runtime glTF / GLB importer backed by glTFast.
    /// Supports .gltf and .glb files with full PBR materials.
    ///
    /// Usage:
    ///   var importer = new GltfImporter();
    ///   var go = await importer.LoadFromFile("/path/to/model.glb");
    /// </summary>
    public class GltfImporter
    {
        private static readonly string[] SupportedExtensions = { ".gltf", ".glb" };

        // ── File ─────────────────────────────────────────────────────────────

        /// <summary>Load from an absolute file path. Returns null on failure.</summary>
        public async Task<GameObject> LoadFromFile(string filepath)
        {
            if (!File.Exists(filepath))
            {
                Debug.LogError($"[VTT] glTF file not found: {filepath}");
                return null;
            }

            string ext = Path.GetExtension(filepath).ToLower();
            if (Array.IndexOf(SupportedExtensions, ext) < 0)
            {
                Debug.LogError($"[VTT] Unsupported format '{ext}'. Use .gltf or .glb.");
                return null;
            }

            return await ImportCore(filepath, Path.GetFileNameWithoutExtension(filepath));
        }

        /// <summary>
        /// Fire-and-forget wrapper for callers that can't await.
        /// The callback receives the loaded GameObject or null on failure.
        /// </summary>
        public async void LoadFromFileAsync(string filepath, Action<GameObject> onComplete)
        {
            var result = await LoadFromFile(filepath);
            onComplete?.Invoke(result);
        }

        // ── URL ───────────────────────────────────────────────────────────────

        /// <summary>Load from a remote URL (http/https or file://).</summary>
        public async Task<GameObject> LoadFromUrl(string url)
        {
            return await ImportCore(url, Path.GetFileNameWithoutExtension(url));
        }

        public async void LoadFromUrlAsync(string url, Action<GameObject> onComplete)
        {
            var result = await LoadFromUrl(url);
            onComplete?.Invoke(result);
        }

        // ── Raw bytes ─────────────────────────────────────────────────────────

        /// <summary>Load from a raw GLB byte array (e.g. downloaded from network).</summary>
        public async Task<GameObject> LoadFromBytes(byte[] data, string modelName = "ImportedModel")
        {
            if (data == null || data.Length == 0)
            {
                Debug.LogError("[VTT] glTF byte data is null or empty.");
                return null;
            }

            try
            {
                var gltf    = new GltfImport();
                bool success = await gltf.LoadGltfBinary(data);
                if (!success) { Debug.LogError("[VTT] Failed to parse glTF binary data."); return null; }
                return await Instantiate(gltf, modelName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[VTT] glTF byte load error: {e.Message}");
                return null;
            }
        }

        // ── Core ──────────────────────────────────────────────────────────────

        private static async Task<GameObject> ImportCore(string source, string name)
        {
            try
            {
                var  gltf    = new GltfImport();
                bool success = await gltf.Load(source);
                if (!success) { Debug.LogError($"[VTT] glTFast failed to load: {source}"); return null; }
                return await Instantiate(gltf, name);
            }
            catch (Exception e)
            {
                Debug.LogError($"[VTT] glTF import exception: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }

        private static async Task<GameObject> Instantiate(GltfImport gltf, string name)
        {
            var root        = new GameObject(name);
            var instantiator = new GameObjectInstantiator(gltf, root.transform);
            bool success    = await gltf.InstantiateMainSceneAsync(instantiator);

            if (!success)
            {
                Debug.LogError($"[VTT] glTFast failed to instantiate scene for '{name}'.");
                UnityEngine.Object.Destroy(root);
                return null;
            }

            return root;
        }
    }
}