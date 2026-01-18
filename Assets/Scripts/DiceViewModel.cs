namespace DefaultNamespace
{
    using Loxodon.Framework.Observables;
    
    public class DiceViewModel: ObservableObject
    {
        private readonly Dice _model;

        public ObservableProperty<int> Result { get; }
        public ObservableList<int> History { get; }

        public DiceViewModel(Dice model)
        {
            _model = model;
            Result = new ObservableProperty<int>(0);
            History = new ObservableList<int>();
        }

        public void Roll()
        {
            var value = _model.RollD20();
            Result.Value = value;
            History.Add(value);
        }
    }
}