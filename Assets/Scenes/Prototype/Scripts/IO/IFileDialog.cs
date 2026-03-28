namespace VTT.IO
{
    public interface IFileDialogService
    {
        /// <summary>Returns full path or null if cancelled.</summary>
        string SaveFile(string title, string defaultName, string extension);
        string OpenFile(string title, string extension);
        string OpenFile(string title, string[] extensions);
    }
}
 