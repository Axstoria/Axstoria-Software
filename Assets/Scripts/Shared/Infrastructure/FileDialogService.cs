using System.IO;
using Shared.App.Port;

#if UNITY_EDITOR
using UnityEditor;
#else
using UnityEngine;
#endif

namespace Shared.Infrastructure
{
    public class FileDialogService : IFileDialogService
    {
        public string SaveFile(string title, string defaultName, string extension)
        {
#if UNITY_EDITOR
            string ext  = extension.TrimStart('.');
            string path = EditorUtility.SaveFilePanel(title, "", defaultName, ext);
            if (string.IsNullOrEmpty(path)) return null;
            if (!path.EndsWith($".{ext}")) path += $".{ext}";
            return path;
#else
            var path = Path.Combine(Application.persistentDataPath, defaultName);
            if (!path.EndsWith(extension)) path += extension;
            return path;
#endif
        }

        public string OpenFile(string title, string[] extensions)
        {
#if UNITY_EDITOR
            string filter = string.Join(",", System.Array.ConvertAll(extensions, e => e.TrimStart('.')));
            string path   = EditorUtility.OpenFilePanel(title, "", filter);
            return string.IsNullOrEmpty(path) ? null : path;
#else
            var directory = Application.persistentDataPath;
            foreach (var ext in extensions)
            {
                string clean = ext.TrimStart('.');
                var files = Directory.GetFiles(directory, $"*.{clean}");
                if (files.Length > 0) return files[0];
            }
            return null;
#endif
        }
    }
}
