using Domain;
using Loxodon.Framework.Observables;

namespace Controler.Editor.ViewModels
{
    public class ObjectViewModel : SceneViewModel
    {
        public readonly ObservableProperty<bool> IsInteractable =  new ObservableProperty<bool>(false);
        private readonly SceneObject _object;
        public SceneObject Model => _object;

        public ObjectViewModel(SceneObject sceneObject) : base(sceneObject)
        {
            _object = sceneObject;
            IsInteractable.Value = _object.IsInteractable;
            
            IsInteractable.ValueChanged += (sender, args) =>
            {
                _object.IsInteractable = IsInteractable.Value;
            };
        }
    }
}