#if UNITY_EDITOR
using UnityEditor;

namespace HexGrid.IO
{
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
    }
}
#endif
