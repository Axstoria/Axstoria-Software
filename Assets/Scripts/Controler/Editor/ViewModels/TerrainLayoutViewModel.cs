using Domain;
using Loxodon.Framework.Observables;

namespace Controler.Editor.ViewModels
{
    public class TerrainLayoutViewModel : ObservableObject
    {
        private readonly TerrainLayout _model;
        public TerrainLayout Model => _model;

        public ObservableProperty<int>   Width     { get; } = new();
        public ObservableProperty<int>   Depth     { get; } = new();
        public ObservableProperty<int>   Thickness { get; } = new();
        public ObservableProperty<float> Height    { get; } = new();

        public TerrainLayoutViewModel(TerrainLayout model)
        {
            _model = model;

            Width.Value     = _model.Width;
            Depth.Value     = _model.Depth;
            Thickness.Value = _model.Thickness;
            Height.Value    = _model.Height;

            Width.ValueChanged     += (_, __) => _model.Width     = Width.Value;
            Depth.ValueChanged     += (_, __) => _model.Depth     = Depth.Value;
            Thickness.ValueChanged += (_, __) => _model.Thickness = Thickness.Value;
            Height.ValueChanged    += (_, __) => _model.Height    = Height.Value;
        }
    }
}
