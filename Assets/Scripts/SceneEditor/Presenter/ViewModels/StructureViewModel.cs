using Loxodon.Framework.Observables;
using SceneEditor.Domain;

namespace SceneEditor.Presenter.ViewModels
{
    public class StructureViewModel : SceneViewModel
    {
        private readonly Structure _structure;
        public Structure Model => _structure;

        public ObservableProperty<bool> IsDestructible { get; } = new ObservableProperty<bool>(false);

        public StructureViewModel(Structure structure) : base(structure)
        {
            _structure = structure;
            IsDestructible.Value = structure.IsDestructible;

            IsDestructible.ValueChanged += (_, __) => _structure.IsDestructible = IsDestructible.Value;
        }
    }
}
