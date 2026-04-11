using Loxodon.Framework.Observables;
using Domain;

namespace Controler.Editor.ViewModels
{
    /// <summary>
    /// Base ViewModel for objects in the scene.
    /// </summary>
    public class SceneViewModel : ObservableObject
    {
        private readonly SceneModel _model;
        
        public TransformViewModel Transform { get; }

        public string Id => _model.Id;
        
        public string ModelPath => _model.ModelPath;

        /// <summary>
        /// Initializes a new instance of the SceneViewModel class with the given SceneModel.
        /// </summary>
        /// <param name="model"></param>
        public SceneViewModel(SceneModel model)
        {
            _model = model;

            Transform = new TransformViewModel(model.Transform);
        }
    }
}