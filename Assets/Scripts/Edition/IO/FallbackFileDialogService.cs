using System.IO;
using UnityEngine;

namespace HexGrid.IO
{
    /// Fallback when no native dialog is available (uses persistentDataPath).
    public class FallbackFileDialogService : IFileDialogService
    {
        public string SaveFile(string title, string defaultName, string extension)
        {
            var path = Path.Combine(Application.persistentDataPath, defaultName);
            if (!path.EndsWith($".{extension}")) path += $".{extension}";
            Debug.LogWarning($"No runtime file dialog available. Saving to: {path}");
            return path;
        }

        public string OpenFile(string title, string extension)
        {
            var candidate = Path.Combine(Application.persistentDataPath, $"map.{extension}");
            if (File.Exists(candidate)) return candidate;
            Debug.LogWarning("No runtime file dialog available and no fallback file found.");
            return null;
        }
    }
}
