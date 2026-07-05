using Loxodon.Framework.Observables;
using MapEditor.Domain;

namespace MapEditor.Presenter.ViewModels
{
    public class PlayerViewModel : ObservableObject
    {
        public Player Model { get; }

        public ObservableProperty<string> Name         { get; } = new ObservableProperty<string>("");
        public ObservableProperty<string> PawnId        { get; } = new ObservableProperty<string>("");
        public ObservableProperty<bool>   IsGameMaster { get; } = new ObservableProperty<bool>(false);
        public ObservableProperty<string> HexColor     { get; } = new ObservableProperty<string>("#3399FF");

        public PlayerViewModel(Player player)
        {
            Model = player;

            Name.Value         = player.Name;
            PawnId.Value       = player.PawnId;
            IsGameMaster.Value = player.IsGameMaster;
            HexColor.Value     = player.HexColor;

            Name.ValueChanged     += (_, __) => player.Name     = Name.Value;
            PawnId.ValueChanged   += (_, __) => player.PawnId   = PawnId.Value;
            HexColor.ValueChanged += (_, __) => player.HexColor = HexColor.Value;
        }
    }
}
