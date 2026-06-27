namespace Shared.Domain
{
    public interface IPermissionService
    {
        bool HasAccess(IHasTags principal, IHasTags resource);
    }
}
