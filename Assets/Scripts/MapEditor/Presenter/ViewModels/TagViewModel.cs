using Loxodon.Framework.Observables;
using SceneEditor.Domain;

namespace MapEditor.Presenter.ViewModels
{
    public class TagViewModel : ObservableObject
    {
        public MetadataEntry Entry { get; }
        public TagValue Model => (TagValue)Entry.EntryValue;

        public ObservableProperty<string> Name     { get; } = new ObservableProperty<string>("");
        public ObservableProperty<string> HexColor { get; } = new ObservableProperty<string>("#FFFFFF");

        public TagViewModel(MetadataEntry entry)
        {
            Entry = entry;
            Refresh();
        }

        public void Refresh()
        {
            Name.Value     = Model.Name;
            HexColor.Value = Model.HexColor;
        }
    }
}
