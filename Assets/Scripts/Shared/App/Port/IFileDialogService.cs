namespace Shared.App.Port
{
    public interface IFileDialogService
    {
        string SaveFile(string title, string defaultName, string extension);
        string OpenFile(string title, string[] extensions);
    }
}
