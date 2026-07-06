using Loxodon.Framework.Observables;
using MapEditor.Domain;
using UnityEngine;

namespace MapEditor.Presenter.ViewModels
{
    public class TerrainLayoutViewModel : ObservableObject
    {
        private readonly TerrainLayout _model;
        public TerrainLayout Model => _model;

        public ObservableProperty<int>   Width     { get; } = new();
        public ObservableProperty<int>   Depth     { get; } = new();
        public ObservableProperty<int>   Thickness { get; } = new();
        public ObservableProperty<float> Height    { get; } = new();
        public ObservableProperty<Color> Color     { get; } = new();

        public TerrainLayoutViewModel(TerrainLayout model)
        {
            _model = model;

            Width.Value     = _model.Width;
            Depth.Value     = _model.Depth;
            Thickness.Value = _model.Thickness;
            Height.Value    = _model.Height;
            Color.Value     = _model.Color != null && _model.Color.Length >= 3
                ? new Color(_model.Color[0], _model.Color[1], _model.Color[2],
                    _model.Color.Length > 3 ? _model.Color[3] : 1f)
                : new Color(0.6f, 0.4f, 0.2f);

            Width.ValueChanged     += (_, __) => _model.Width     = Width.Value;
            Depth.ValueChanged     += (_, __) => _model.Depth     = Depth.Value;
            Thickness.ValueChanged += (_, __) => _model.Thickness = Thickness.Value;
            Height.ValueChanged    += (_, __) => _model.Height    = Height.Value;
            Color.ValueChanged     += (_, __) => _model.Color     = new[]
                { Color.Value.r, Color.Value.g, Color.Value.b, Color.Value.a };
        }
    }
}
