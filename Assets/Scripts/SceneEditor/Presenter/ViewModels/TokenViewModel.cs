using Domain;
using Loxodon.Framework.Observables;

namespace Controler.Editor.ViewModels
{
    /// <summary>
    /// ViewModel for Token entities in the map editor.
    /// </summary>
    public class TokenViewModel : SceneViewModel
    {

        public readonly ObservableProperty<string> Faction = new ObservableProperty<string>("");
        
        private readonly Token _token;
        
         public Token Model => _token;

         /// <summary>
         /// Initializes a new instance of the TokenViewModel class with the given Token model.
         /// </summary>
         /// <param name="token"></param>
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