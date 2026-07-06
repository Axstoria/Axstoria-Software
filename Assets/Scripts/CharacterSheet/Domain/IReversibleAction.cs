namespace CharacterSheet.Domain
{
    public interface IReversibleAction
    {
        void Execute();
        void Undo();
    }
}