using System.IO;
using Shared.Domain;
using UnityEngine;

namespace CharacterSheet.Infrastructure
{
    public class SheetFolderRepository : ISaveRepository
    {
        private const string JSON_FILE_NAME = "sheet.json";
        private readonly string _folder;

        public SheetFolderRepository(string folder = "")
        {
            _folder = Path.Combine(Application.persistentDataPath, folder);
            
            if (!Directory.Exists(_folder))
                Directory.CreateDirectory(_folder);
        }
        
        public void Save(string container, string content)
        {
            string folderPath = Path.Combine(_folder, "Sheets", container);
            
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            
            string fullPath = Path.Combine(folderPath, JSON_FILE_NAME);
            File.WriteAllText(fullPath, content);
        }

        public string Load(string container)
        {
            string folderPath = Path.Combine(_folder, "Sheets", container);
            string fullPath = Path.Combine(folderPath, JSON_FILE_NAME);
            
            return File.ReadAllText(fullPath);
        }

        public bool Exists(string container)
        {
            string folderPath = Path.Combine(_folder, "Sheets", container);
            string fullPath = Path.Combine(folderPath, JSON_FILE_NAME);
            return File.Exists(fullPath);
        }
    }
}