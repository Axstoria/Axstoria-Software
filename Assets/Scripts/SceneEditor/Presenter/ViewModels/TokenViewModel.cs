using Loxodon.Framework.Observables;
using SceneEditor.Domain;

namespace SceneEditor.Presenter.ViewModels
{
    public class TokenViewModel : SceneViewModel
    {
        private readonly Token _token;
        public Token Model => _token;

        public ObservableProperty<string> Faction { get; } = new ObservableProperty<string>("");

        public TokenViewModel(Token token) : base(token)
        {
            _token = token;
            Faction.Value = _token.Faction;

            Faction.ValueChanged += (_, __) => _token.Faction = Faction.Value;
        }
    }
}
