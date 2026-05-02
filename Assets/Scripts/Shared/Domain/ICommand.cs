namespace Shared.Domain
{
    public interface ICommand
    {
        string Label { get; }
        void Execute();
        void Undo();
        void Redo();
    }
}
