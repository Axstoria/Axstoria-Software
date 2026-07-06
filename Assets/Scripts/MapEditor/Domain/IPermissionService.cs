using SceneEditor.Domain;

namespace MapEditor.Domain
{
    public interface IPermissionService
    {
        bool CanInteract(Player player, SceneObject obj);
    }
}
