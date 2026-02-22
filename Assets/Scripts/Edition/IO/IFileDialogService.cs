namespace HexGrid.IO
{
    public interface IFileDialogService
    {
        string SaveFile(string title, string defaultName, string extension); // returns full path or null
        string OpenFile(string title, string extension);                     // returns full path or null
    }
}
