using Loxodon.Framework.Observables;
using MapEditor.Presenter.ViewModels;
using SceneEditor.Domain;

namespace SceneEditor.Presenter.ViewModels
{
    public class SceneViewModel : ObservableObject
    {
        private readonly SceneModel _model;

        public TransformViewModel Transform { get; }

        public string Id        => _model.Id;
        public string ModelPath => _model.ModelPath;

        public SceneViewModel(SceneModel model)
        {
            _model    = model;
            Transform = new TransformViewModel(model.Transform);
        }
    }
}
