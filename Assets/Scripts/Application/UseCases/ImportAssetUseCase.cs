using App.Commands;
using App.Ports;
using Domain;

namespace App.UseCases
{
    /// <summary>
    /// Opens a file dialog for a glTF/GLB asset and registers it as a SceneObject on the Map
    /// via the command history so the action is undoable.
    /// The View layer is responsible for the actual Unity GameObject instantiation by observing
    /// Map.Objects for new entries where IsImported == true.
    /// </summary>
    public class ImportAssetUseCase
    {
        private readonly Map            _map;
        private readonly CommandHistory _history;
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
