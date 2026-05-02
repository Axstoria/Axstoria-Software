using MapEditor.Domain;
using SceneEditor.App.Command;
using SceneEditor.Domain;
using Shared.App.Port;
using Shared.Domain;

namespace AssetImporter.App.UseCase
{
    public class ImportAssetUseCase
    {
        private readonly Map             _map;
        private readonly CommandHistory  _history;
        private readonly IFileDialogService _dialog;

        public ImportAssetUseCase(Map map, CommandHistory history, IFileDialogService dialog)
        {
            _map     = map;
            _history = history;
            _dialog  = dialog;
        }

        public SceneObject Execute(string category = "Imported")
        {
            string path = _dialog.OpenFile("Import 3D Asset", new[] { ".gltf", ".glb" });
            if (string.IsNullOrEmpty(path)) return null;

            var obj = new SceneObject
            {
                Id          = System.Guid.NewGuid().ToString(),
                DisplayName = System.IO.Path.GetFileNameWithoutExtension(path),
                Category    = category,
                IsImported  = true,
                ImportPath  = path
            };

            _history.Record(new AddObjectCommand(_map, obj));
            return obj;
        }
    }
}
