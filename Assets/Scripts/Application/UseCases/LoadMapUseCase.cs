using System.IO;
using App.Ports;
using Domain;

namespace App.UseCases
{
    public class LoadMapUseCase
    {
        private readonly IMapSerializer _serializer;
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
