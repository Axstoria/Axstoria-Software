using Domain;
using Loxodon.Framework.Observables;

namespace Controler.Editor.ViewModels
{
    /// <summary>
    /// ViewModel for Structure entities in the map editor.
    /// </summary>
    public class StructureViewModel : SceneViewModel
    {
        private readonly Structure _structure;
        
        public ObservableProperty<bool> IsDestructible = new ObservableProperty<bool>(false);
        
        public Structure Model => _structure;

        /// <summary>
        /// Initializes a new instance of the StructureViewModel class with the given Structure model.
        /// </summary>
        /// <param name="structure"></param>
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