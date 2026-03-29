using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if USE_SFB
using SFB;
#endif

namespace VTT.IO
{
    // ── Editor ────────────────────────────────────────────────────────────────
#if UNITY_EDITOR
    public class EditorFileDialogService : IFileDialogService
    {
        public string SaveFile(string title, string defaultName, string extension)
        {
            var path = EditorUtility.SaveFilePanel(title, "", defaultName, extension);
            if (string.IsNullOrEmpty(path)) return null;
            if (!path.EndsWith($".{extension}")) path += $".{extension}";
            return path;
        }

        public string OpenFile(string title, string extension) =>
            NullIfEmpty(EditorUtility.OpenFilePanel(title, "", extension));

        public string OpenFile(string title, string[] extensions) =>
            NullIfEmpty(EditorUtility.OpenFilePanel(title, "", string.Join(",", extensions)));

        private static string NullIfEmpty(string s) =>
            string.IsNullOrEmpty(s) ? null : s;
    }
#endif

    // ── StandaloneFileBrowser (USE_SFB) ───────────────────────────────────────
#if USE_SFB
    public class SFBFileDialogService : IFileDialogService
    {
        public string SaveFile(string title, string defaultName, string extension)
        {
            var path = StandaloneFileBrowser.SaveFilePanel(title, "", defaultName,
                new[] { new ExtensionFilter(extension.ToUpper(), extension) });
            return string.IsNullOrEmpty(path) ? null : path;
        }

        public string OpenFile(string title, string extension)
        {
            var paths = StandaloneFileBrowser.OpenFilePanel(title, "",
                new[] { new ExtensionFilter(extension.ToUpper(), extension) }, false);
            return (paths == null || paths.Length == 0) ? null : paths[0];
        }

        public string OpenFile(string title, string[] extensions)
        {
            var paths = StandaloneFileBrowser.OpenFilePanel(title, "",
                new[] { new ExtensionFilter("3D Models", extensions) }, false);
            return (paths == null || paths.Length == 0) ? null : paths[0];
        }
    }
#endif

    // ── Fallback ──────────────────────────────────────────────────────────────
    public class FallbackFileDialogService : IFileDialogService
    {
        public string SaveFile(string title, string defaultName, string extension)
        {
            var path = Path.Combine(Application.persistentDataPath, defaultName);
            if (!path.EndsWith($".{extension}")) path += $".{extension}";
            Debug.LogWarning($"[VTT] No dialog available — saving to {path}");
            return path;
        }

        public string OpenFile(string title, string extension)
        {
            var candidate = Path.Combine(Application.persistentDataPath, $"file.{extension}");
            if (File.Exists(candidate)) return candidate;
            Debug.LogWarning("[VTT] No dialog and no fallback file found.");
            return null;
        }

        public string OpenFile(string title, string[] extensions)
        {
            foreach (var ext in extensions)
            {
                var files = Directory.GetFiles(Application.persistentDataPath, $"*.{ext}");
                if (files.Length > 0) { Debug.LogWarning($"[VTT] Fallback: {files[0]}"); return files[0]; }
            }
            Debug.LogWarning("[VTT] No fallback files found.");
            return null;
        }
    }
}
