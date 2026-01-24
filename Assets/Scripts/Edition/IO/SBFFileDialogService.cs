#if USE_SFB
using SFB;

namespace HexGrid.IO
{
    /// Runtime OS dialogs via StandaloneFileBrowser (define USE_SFB).
    public class SFBFileDialogService : IFileDialogService
    {
        public string SaveFile(string title, string defaultName, string extension)
        {
            var filters = new[] { new ExtensionFilter("JSON", extension) };
            var path = StandaloneFileBrowser.SaveFilePanel(title, "", defaultName, filters);
            return string.IsNullOrEmpty(path) ? null : path;
        }

        public string OpenFile(string title, string extension)
        {
            var filters = new[] { new ExtensionFilter("JSON", extension) };
            var paths = StandaloneFileBrowser.OpenFilePanel(title, "", filters, false);
            return (paths == null || paths.Length == 0) ? null : paths[0];
        }
    }
}
#endif
