using System.IO;
using Shared.Domain;
using UnityEngine;

namespace Shared.Infrastructure
{
    public class JsonFileSaveRepository : ISaveRepository
    {
        private readonly string _saveFolder;
        
        public JsonFileSaveRepository(string folder)
        {
            _saveFolder = Path.Combine(Application.persistentDataPath, folder);
            if (!Directory.Exists(_saveFolder))
                Directory.CreateDirectory(_saveFolder);
        }
        public void Save(string fileName, string content)
        {
            string fullPath = Path.Combine(_saveFolder, $"{fileName}.json");
            File.WriteAllText(fullPath, content);
        }

        public string Load(string fileName)
        {
            string fullPath = Path.Combine(_saveFolder, $"{fileName}.json");
            
            string content = File.ReadAllText(fullPath);
            return content;
        }

        public bool Exists(string fileName)
        {
            string fullPath = Path.Combine(_saveFolder, $"{fileName}.json");
            return File.Exists(fullPath);
        }
    }
}