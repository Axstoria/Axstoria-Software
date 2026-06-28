using System;
using System.IO;
using AssetImporter.AssetImporter.App;
using Shared.App.Port;
using Application = UnityEngine.Application;

namespace AssetImporter.AssetImporter.Infrastructure
{
    public class ImageImportService : IImageImportService
    {
        private readonly IFileDialogService _dialog;

        public ImageImportService(IFileDialogService dialog)
        {
            _dialog = dialog;
        }
        
        public string ImportImageFromDisk()
        {
            string path = _dialog.OpenFile("Import Sprite", new [] {".png", ".jpg"});
            if (string.IsNullOrEmpty(path)) return null;
            
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(path);
            string dest = Path.Combine(Application.persistentDataPath, "Assets",  fileName);
            
            File.Copy(path, dest);
            return fileName;
        }
    }
}