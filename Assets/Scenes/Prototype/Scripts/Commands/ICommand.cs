namespace VTT
{
    /// <summary>
    /// Implement this interface to add new command types.
    /// </summary>
    public interface ICommand
    {
        string Label { get; }
        void Undo();
        void Redo();
    }
}