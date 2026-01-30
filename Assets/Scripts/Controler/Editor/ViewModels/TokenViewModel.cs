using Domain;
using Loxodon.Framework.Observables;

namespace Controler.Editor.ViewModels
{
    /// <summary>
    /// View model for managing character, NPC, or creature tokens within the map editor.
    /// Extends SceneViewModel to add token-specific properties like faction information.
    /// </summary>
    /// <remarks>
    /// This view model handles the presentation and binding of token data to the UI,
    /// ensuring that changes to token properties are automatically synchronized with the underlying model.
    /// </remarks>
    public class TokenViewModel : SceneViewModel
    {
        /// <summary>
        /// Gets the observable property for the token's faction or allegiance.
        /// </summary>
        /// <remarks>
        /// Changes to this property are automatically synchronized back to the underlying Token model.
        /// Initialized to an empty string.
        /// </remarks>
        public readonly ObservableProperty<string> Faction = new ObservableProperty<string>("");
        
        /// <summary>
        /// The underlying Token model that this view model represents.
        /// </summary>
        private readonly Token _token;
        
        /// <summary>
        /// Gets the underlying Token model.
        /// </summary>
        public Token Model => _token;

        /// <summary>
        /// Initializes a new instance of the TokenViewModel class.
        /// </summary>
        /// <param name="token">The Token model to be wrapped by this view model.</param>
        /// <remarks>
        /// This constructor initializes the Faction observable property with the token's faction value
        /// and sets up two-way data binding between the view model and the model.
        /// </remarks>
        public TokenViewModel(Token token) : base(token)
        {
            _token = token;
            Faction.Value = _token.Faction;
            
            Faction.ValueChanged += (sender, args) =>
            {
                _token.Faction = Faction.Value;
            };
        }
    }
}