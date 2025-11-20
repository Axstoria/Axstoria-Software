using Domain;
using Loxodon.Framework.Observables;

namespace Controler.Editor.ViewModels
{
    public class StructureViewModel : SceneViewModel
    {
        private readonly Structure _structure;
        public ObservableProperty<bool> IsDestructible = new ObservableProperty<bool>(false);
        public Structure Model => _structure;

        public StructureViewModel(Structure structure) : base(structure)
        {
            _structure = structure;
            IsDestructible.Value = structure.IsDestructible;

            IsDestructible.ValueChanged += (sender, args) =>
            {
                _structure.IsDestructible = IsDestructible.Value;
            };
        }
    }
}