using Domain;
using Loxodon.Framework.Observables;

namespace Controler.Editor.ViewModels
{
    /// <summary>
    /// View model for managing architectural structures and environmental elements within the map editor.
    /// Extends SceneViewModel to add properties specific to structures such as destructibility.
    /// </summary>
    /// <remarks>
    /// This view model handles the presentation and binding of structure data to the UI,
    /// allowing editors to configure whether structures can be destroyed and manage other structural properties.
    /// </remarks>
    public class StructureViewModel : SceneViewModel
    {
        /// <summary>
        /// The underlying Structure model that this view model represents.
        /// </summary>
        private readonly Structure _structure;
        
        /// <summary>
        /// Gets the observable property indicating whether this structure is destructible.
        /// </summary>
        /// <remarks>
        /// Changes to this property are automatically synchronized back to the underlying Structure model.
        /// Initialized to false (indestructible by default).
        /// </remarks>
        public ObservableProperty<bool> IsDestructible = new ObservableProperty<bool>(false);
        
        /// <summary>
        /// Gets the underlying Structure model.
        /// </summary>
        public Structure Model => _structure;

        /// <summary>
        /// Initializes a new instance of the StructureViewModel class.
        /// </summary>
        /// <param name="structure">The Structure model to be wrapped by this view model.</param>
        /// <remarks>
        /// This constructor initializes the IsDestructible observable property with the structure's current state
        /// and sets up two-way data binding between the view model and the model.
        /// </remarks>
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