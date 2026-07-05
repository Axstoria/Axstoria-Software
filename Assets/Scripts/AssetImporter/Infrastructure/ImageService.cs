using System;
using System.IO;
using AssetImporter.AssetImporter.App;
using AssetImporter.AssetImporter.App.UseCase;
using Shared.App.Port;
using UnityEngine;
using Application = UnityEngine.Application;

namespace AssetImporter.AssetImporter.Infrastructure
{
    public class ImageService : IImageImportService, IImageLoaderService
    {
        private readonly IFileDialogService _dialog;
        private readonly string _saveFolder;

        public ImageService(IFileDialogService dialog, string directory = "")
        {
            _dialog = dialog;

            _saveFolder = Path.Combine(Application.persistentDataPath, directory);
            if (!Directory.Exists(_saveFolder))
                Directory.CreateDirectory(_saveFolder);
        }

        public string ImportImageFromDisk(string containerId = "")
        {
            string path = _dialog.OpenFile("Import Sprite", new[] { ".png", ".jpg" });
            if (string.IsNullOrEmpty(path)) return null;

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(path);
            string folder = Path.Combine(_saveFolder, containerId, "Assets");
            
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            
            string dest = Path.Combine(folder, fileName);

            File.Copy(path, dest);
            return fileName;
        }

        public Sprite LoadSprite(string fileName, string containerId = "")
        {
            if (string.IsNullOrEmpty(fileName)) return null;

            string path = Path.Combine(_saveFolder, containerId, "Assets", fileName);

            if (!File.Exists(path)) {
                Debug.LogError($"Canot find : {path}");
                return null;
            }
            byte[] fileData = File.ReadAllBytes(path);
            
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
            {
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                return sprite;
            }

            Debug.LogError("Error: cannot convert file");
            return null;
        }
    }
}