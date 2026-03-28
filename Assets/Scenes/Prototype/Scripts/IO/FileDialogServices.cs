using System.IO;
using UnityEngine;

// ── Platform implementations ──────────────────────────────────────────────────

#if UNITY_EDITOR
using UnityEditor;
#endif

#if USE_SFB
using SFB;
#endif

namespace VTT.IO
{
    // ── Editor (runs only in the Unity Editor) ────────────────────────────────
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

        public string OpenFile(string title, string extension)
        {
            var path = EditorUtility.OpenFilePanel(title, "", extension);
            return string.IsNullOrEmpty(path) ? null : path;
        }

        public string OpenFile(string title, string[] extensions)
        {
            var path = EditorUtility.OpenFilePanel(title, "", string.Join(",", extensions));
            return string.IsNullOrEmpty(path) ? null : path;
        }
    }
#endif

    // ── StandaloneFileBrowser (define USE_SFB in Player Settings) ─────────────
#if USE_SFB
    public class SFBFileDialogService : IFileDialogService
    {
        public string SaveFile(string title, string defaultName, string extension)
        {
            var filters = new[] { new ExtensionFilter(extension.ToUpper(), extension) };
            var path    = StandaloneFileBrowser.SaveFilePanel(title, "", defaultName, filters);
            return string.IsNullOrEmpty(path) ? null : path;
        }

        public string OpenFile(string title, string extension)
        {
            var filters = new[] { new ExtensionFilter(extension.ToUpper(), extension) };
            var paths   = StandaloneFileBrowser.OpenFilePanel(title, "", filters, false);
            return (paths == null || paths.Length == 0) ? null : paths[0];
        }

        public string OpenFile(string title, string[] extensions)
        {
            var filters = new[] { new ExtensionFilter("3D Models", extensions) };
            var paths   = StandaloneFileBrowser.OpenFilePanel(title, "", filters, false);
            return (paths == null || paths.Length == 0) ? null : paths[0];
        }
    }
#endif

    // ── Fallback (persistentDataPath — always available) ──────────────────────
    public class FallbackFileDialogService : IFileDialogService
    {
        public string SaveFile(string title, string defaultName, string extension)
        {
            var path = Path.Combine(Application.persistentDataPath, defaultName);
            if (!path.EndsWith($".{extension}")) path += $".{extension}";
            Debug.LogWarning($"[VTT] No file dialog available. Saving to: {path}");
            return path;
        }

        public string OpenFile(string title, string extension)
        {
            var candidate = Path.Combine(Application.persistentDataPath, $"file.{extension}");
            if (File.Exists(candidate)) return candidate;
            Debug.LogWarning("[VTT] No file dialog available and no fallback file found.");
            return null;
        }

        public string OpenFile(string title, string[] extensions)
        {
            foreach (var ext in extensions)
            {
                var files = Directory.GetFiles(Application.persistentDataPath, $"*.{ext}");
                if (files.Length > 0)
                {
                    Debug.LogWarning($"[VTT] Fallback: using {files[0]}");
                    return files[0];
                }
            }
            Debug.LogWarning("[VTT] No fallback files found.");
            return null;
        }
    }

    // ── Factory — picks the right implementation automatically ─────────────────
    public static class FileDialogServiceFactory
    {
        public static IFileDialogService Create()
        {
#if UNITY_EDITOR
            return new EditorFileDialogService();
#elif USE_SFB
            return new SFBFileDialogService();
#else
            return new FallbackFileDialogService();
#endif
        }
    }
}