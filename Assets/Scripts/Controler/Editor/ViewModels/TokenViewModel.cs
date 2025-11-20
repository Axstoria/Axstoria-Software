using Domain;
using Loxodon.Framework.Observables;

namespace Controler.Editor.ViewModels
{
    public class TokenViewModel : SceneViewModel
    {
        public readonly ObservableProperty<string> Faction = new ObservableProperty<string>("");
        private readonly Token _token;
        public Token Model => _token;

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