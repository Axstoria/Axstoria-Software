using System;
using System.IO;
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

        public async void Execute(string category = "Imported")
        {
            string path = _dialog.OpenFile("Import 3D Asset", new[] { ".gltf", ".glb" });
            if (string.IsNullOrEmpty(path)) return;

            var obj = new SceneObject
            {
                Id          = Guid.NewGuid().ToString(),
                DisplayName = Path.GetFileNameWithoutExtension(path),
                Category    = category,
                IsImported  = true,
                ImportPath  = path
            };

            var gltfImport = new GltfImport(logger: new ConsoleLogger());
            bool loaded = await gltfImport.Load(new Uri(path).AbsoluteUri);
            if (!loaded)
            {
                Debug.LogError($"[ImportAssetUseCase] Failed to load GLTF: {path}");
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
    }
}
