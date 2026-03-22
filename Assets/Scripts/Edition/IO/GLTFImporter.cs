using System;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

namespace Edition.IO
{
    /// <summary>
    /// Runtime glTF/GLB importer using glTFast library.
    /// Supports .gltf and .glb files with PBR materials.
    /// </summary>
    public class GltfImporter
    {
        /// <summary>
        /// Loads a glTF/GLB file and creates a Unity GameObject.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="onComplete"></param>
        public async void ImportModeAsync(string filepath, Action<GameObject> onComplete)
        {
            if (!File.Exists(filepath))
            {
                Debug.LogError($"File not found: {filepath}");
                onComplete?.Invoke(null);
                return;
            }
            
            var extension = Path.GetExtension(filepath).ToLower();
            if (extension != ".gltf" && extension != ".glb")
            {
                Debug.LogError($"Unsupported file format: {extension}. Only .gltf or .glb files are supported.");
                onComplete?.Invoke(null);
                return;
            }

            try
            {
                GameObject result = await LoadModelFromFile(filepath);
                onComplete?.Invoke(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error importing glTF model: {e.Message}/n{e.StackTrace}");
                onComplete?.Invoke(null);
            }
        }

        public async Task<GameObject> LoadModelFromFile(string filepath)
        {
            var gltf = new GltfImport();
            
            var success = await gltf.Load(filepath);

            if (!success)
            {
                Debug.LogError($"Failed to load glTF file.");
                return null;
            }
            
            // Create GameObject
            GameObject rootObject = new  GameObject(Path.GetFileNameWithoutExtension(filepath));
            
            var instantiator = new GameObjectInstantiator(gltf,  rootObject.transform);
            success =  await gltf.InstantiateMainSceneAsync(instantiator);

            if (!success)
            {
                Debug.LogError($"Failed to instantiate glTF scene.");
                UnityEngine.Object.Destroy(rootObject);
                return null;
            }
            return rootObject;
        }
        
        /// <summary>
        /// Loads from URL (for web-based assets).
        /// </summary>
        public async void ImportModelFromURL(string url, Action<GameObject> onComplete)
        {
            try
            {
                var gltf = new GltfImport();
                bool success = await gltf.Load(url);

                if (!success)
                {
                    Debug.LogError($"Failed to load glTF from URL: {url}");
                    onComplete?.Invoke(null);
                    return;
                }

                GameObject rootObject = new GameObject(Path.GetFileNameWithoutExtension(url));
                var instantiator = new GameObjectInstantiator(gltf, rootObject.transform);
                success = await gltf.InstantiateMainSceneAsync(instantiator);

                if (!success)
                {
                    Debug.LogError("Failed to instantiate glTF scene from URL.");
                    UnityEngine.Object.Destroy(rootObject);
                    onComplete?.Invoke(null);
                    return;
                }

                onComplete?.Invoke(rootObject);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading glTF from URL: {e.Message}");
                onComplete?.Invoke(null);
            }
        }

        /// <summary>
        /// Loads from raw byte data.
        /// </summary>
        public async Task<GameObject> LoadModelFromBytes(byte[] data, string modelName = "ImportedModel")
        {
            try
            {
                var gltf = new GltfImport();
                bool success = await gltf.LoadGltfBinary(data);

                if (!success)
                {
                    Debug.LogError("Failed to load glTF from byte data.");
                    return null;
                }

                GameObject rootObject = new GameObject(modelName);
                var instantiator = new GameObjectInstantiator(gltf, rootObject.transform);
                success = await gltf.InstantiateMainSceneAsync(instantiator);

                if (!success)
                {
                    Debug.LogError("Failed to instantiate glTF scene from bytes.");
                    UnityEngine.Object.Destroy(rootObject);
                    return null;
                }

                return rootObject;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading glTF from bytes: {e.Message}");
                return null;
            }
        }
    }
}
