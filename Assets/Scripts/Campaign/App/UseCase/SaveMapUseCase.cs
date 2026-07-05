using System.IO;
using Campaign.App.Port;
using MapEditor.Domain;
using Shared.App.Port;

namespace Campaign.App.UseCase
{
    public class SaveMapUseCase
    {
        private readonly IMapSerializer    _serializer;
        private readonly IFileDialogService _dialog;

        public SaveMapUseCase(IMapSerializer serializer, IFileDialogService dialog)
        {
            _serializer = serializer;
            _dialog     = dialog;
        }

        public bool Execute(Map map)
        {
            string path = _dialog.SaveFile("Save Map", map.Name, ".json");
            if (string.IsNullOrEmpty(path)) return false;

            string json = _serializer.Serialize(map);
            File.WriteAllText(path, json);
            return true;
        }
    }
}
