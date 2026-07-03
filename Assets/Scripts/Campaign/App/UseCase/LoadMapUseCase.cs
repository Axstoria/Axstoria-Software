using System.IO;
using Campaign.App.Port;
using MapEditor.Domain;
using Shared.App.Port;

namespace Campaign.App.UseCase
{
    public class LoadMapUseCase
    {
        private readonly IMapSerializer    _serializer;
        private readonly IFileDialogService _dialog;

        public LoadMapUseCase(IMapSerializer serializer, IFileDialogService dialog)
        {
            _serializer = serializer;
            _dialog     = dialog;
        }

        public Map Execute()
        {
            string path = _dialog.OpenFile("Open Map", new[] { ".json" });
            if (string.IsNullOrEmpty(path)) return null;

            string json = File.ReadAllText(path);
            return _serializer.Deserialize(json);
        }
    }
}
