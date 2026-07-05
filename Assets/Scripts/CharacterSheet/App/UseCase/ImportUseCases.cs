using System;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using Shared.App.Port;
using UnityEngine;

namespace CharacterSheet.App.UseCase
{
    public class ExportUseCases
    {
        private readonly IFileDialogService _dialog;

        public ExportUseCases(IFileDialogService dialog)
        {
            _dialog = dialog;
        }

        public void Execute(string target)
        {
            string dest = _dialog.SaveFile("Select folder", "CharacterSheet", ".axsheet");
            if (string.IsNullOrEmpty(dest)) return;

            var targetFolder = Path.Combine(Application.persistentDataPath, "Workspace/Sheets", target);

            ZipFile.CreateFromDirectory(targetFolder, dest);
        }
    }

    public class ImportUseCase
    {
        private readonly IFileDialogService _dialog;

        public ImportUseCase(IFileDialogService dialog)
        {
            _dialog = dialog;
        }

        public string Execute()
        {
            var importedFilePath = _dialog.OpenFile("Import Sheet", new[] { ".axsheet" });
            try {
                string sheetId = null;
                using (var archive = ZipFile.OpenRead(importedFilePath)) {
                    var jsonEntry = archive.GetEntry("sheet.json");
                    if (jsonEntry == null) {
                        Debug.LogWarning("Fichier invalide : Aucun sheet.json trouvé.");
                        return null;
                    }

                    using (StreamReader reader = new StreamReader(jsonEntry.Open())) {
                        string jsonContent = reader.ReadToEnd();

                        var header = JsonConvert.DeserializeObject<MinimalSheetDTO>(jsonContent);
                        sheetId = header?.Id;

                        /*if (header == null || header.FileType != "AxstoriaSheet")
                        {
                            Debug.LogWarning("Invalid file");
                            return null;
                        }

                        if (header.Version != "1.0")
                        {
                            Debug.LogWarning("Deprecated version file");
                            return null;
                        }*/
                    }
                }

                if (string.IsNullOrEmpty(sheetId)) {
                    Debug.LogWarning("Invalid file: cannot read sheet id");
                    return null;
                }

                string targetFolder = Path.Combine(Application.persistentDataPath, "Workspace/Sheets", sheetId);
                if (Directory.Exists(targetFolder)) {
                    Directory.Delete(targetFolder, true);
                }

                ZipFile.ExtractToDirectory(importedFilePath, targetFolder);

                return sheetId;
            }
            catch (Exception ex) {
                Debug.LogError($"Error during file validation : {ex.Message}");
                return null;
            }
        }

        public class MinimalSheetDTO
        {
            [JsonProperty("Id")] public string Id { get; set; }
        }
    }
}