namespace VTT.IO
{
    /// <summary>
    /// Platform-agnostic file dialog abstraction.
    /// </summary>
    public interface IFileDialogService
    {
        string SaveFile(string title, string defaultName, string extension);
        string OpenFile(string title, string extension);
        string OpenFile(string title, string[] extensions);
    }
}
