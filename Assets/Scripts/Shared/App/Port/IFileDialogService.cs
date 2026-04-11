namespace App.Ports
{
    /// <summary>
    /// Interface for file dialog services, providing methods to open and save files with specified titles and extensions.
    /// </summary>
    public interface IFileDialogService
    {
        string SaveFile(string title, string defaultName, string extension);
        string OpenFile(string title, string[] extensions);
    }
}
