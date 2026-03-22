using System.Collections.Generic;
using UnityEngine;

namespace Edition.Systems
{
    /// <summary>
    /// Manages imported 3D assets that can be placed on the map.
    /// </summary>
    public class AssetImportManager : MonoBehaviour
    {
        [SerializeField] private Transform assetContainer;
        
        private List<GameObject> importedAssets = new List<GameObject>();
        private GameObject currentPreview;
        
        private void Awake()
        {
            if (assetContainer == null)
            {
                assetContainer = new GameObject("ImportedAssets").transform;
                assetContainer.SetParent(transform);
            }
        }

        /// <summary>
        /// Called when a glTF asset is successfully loaded
        /// </summary>
        public void OnAssetLoaded(GameObject loadedAsset, string sourcePath)
        {
            if (loadedAsset == null)
            {
                Debug.LogError("Failed to load asset.");
                return;
            }

            // Parent to container and store
            loadedAsset.transform.SetParent(assetContainer);
            importedAssets.Add(loadedAsset);

            // Position it in the scene
            loadedAsset.transform.position = Vector3.zero;
            
            Debug.Log($"Asset loaded successfully: {loadedAsset.name} from {sourcePath}");
            
            // Optional: Create preview for placement
            SetupForPlacement(loadedAsset);
        }

        private void SetupForPlacement(GameObject asset)
        {
            // Add colliders if needed for raycasting
            if (asset.GetComponentInChildren<Collider>() == null)
            {
                var meshFilters = asset.GetComponentsInChildren<MeshFilter>();
                foreach (var meshFilter in meshFilters)
                {
                    var collider = meshFilter.gameObject.AddComponent<MeshCollider>();
                    collider.convex = false;
                }
            }
        }

        /// <summary>
        /// Get all imported assets
        /// </summary>
        public List<GameObject> GetImportedAssets()
        {
            return new List<GameObject>(importedAssets);
        }

        /// <summary>
        /// Clear all imported assets
        /// </summary>
        public void ClearAllAssets()
        {
            foreach (var asset in importedAssets)
            {
                if (asset != null)
                    Destroy(asset);
            }
            importedAssets.Clear();
        }
    }
}