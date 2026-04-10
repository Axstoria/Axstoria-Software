using Domain;
using Loxodon.Framework.Observables;

namespace Controler.Editor.ViewModels
{
    /// <summary>
    /// ViewModel for TerrainLayout entities in the map editor.
    /// </summary>
    public class TerrainLayoutViewModel : ObservableObject
    {
        private readonly TerrainLayout _model;
        public TerrainLayout Model => _model;

        public ObservableProperty<int>   Width     { get; } = new();
        public ObservableProperty<int>   Depth     { get; } = new();
        public ObservableProperty<int>   Thickness { get; } = new();
        public ObservableProperty<float> Height    { get; } = new();

        /// <summary>
        /// Initializes a new instance of the TerrainLayoutViewModel class with the given TerrainLayout model.
        /// </summary>
        /// <param name="model"></param>
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
