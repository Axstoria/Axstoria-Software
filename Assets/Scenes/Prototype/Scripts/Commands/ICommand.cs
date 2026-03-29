namespace VTT
{
    /// <summary>
    /// Contract for every undoable action.
    /// To add a new command: implement this interface, capture before-state
    /// in the constructor, then call CommandHistory.Instance.Record(new YourCommand()).
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Shown in the undo history UI.
        /// </summary>
        string Label { get; }
        
        /// <summary>
        /// Performs the action. Called once by CommandHistory.Record().
        /// </summary>
        void Execute();

        void Undo();
        void Redo();
    }
}
 