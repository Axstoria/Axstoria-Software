using System.IO;
using Shared.App.Port;

#if UNITY_EDITOR
using UnityEditor;
#else
using SFB;
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
            string ext  = extension.TrimStart('.');
            string path = StandaloneFileBrowser.SaveFilePanel(
                title, "", defaultName,
                new[] { new ExtensionFilter("", ext) });
            return string.IsNullOrEmpty(path) ? null : path;
#endif
        }

        public string OpenFile(string title, string[] extensions)
        {
#if UNITY_EDITOR
            string filter = string.Join(",", System.Array.ConvertAll(extensions, e => e.TrimStart('.')));
            string path   = EditorUtility.OpenFilePanel(title, "", filter);
            return string.IsNullOrEmpty(path) ? null : path;
#else
            var filters = System.Array.ConvertAll(extensions,
                e => new ExtensionFilter("", e.TrimStart('.')));
            string[] paths = StandaloneFileBrowser.OpenFilePanel(title, "", filters, false);
            return paths.Length > 0 ? paths[0] : null;
#endif
        }
    }
}
